using System.Reflection;
using System.Runtime.CompilerServices;
using GameKit.Architecture.Events;

namespace GameKit.Architecture.Testing;

/// <summary>
/// Verifies the central claim of docs/architecture.md — "the boundary contract <i>is</i> commands / queries /
/// events" — against a Model assembly: it exposes internals only to whitelisted assemblies, and every public
/// type is reachable from the CQS surface (commands, queries, events, and caller-declared surface seeds).
/// A public type that nothing on the surface references is a leak.
/// </summary>
public static class ModelBoundary
{
    public static ArchitectureReport Check(Assembly assembly, Action<ModelBoundaryOptions>? configure = null)
    {
        ModelBoundaryOptions options = new();
        configure?.Invoke(options);

        Type[] allTypes = assembly.GetTypes()
            .Where(type => !type.IsDefined(typeof(CompilerGeneratedAttribute), false))
            .ToArray();
        Type[] publicTypes = assembly.GetExportedTypes();
        string[] internalsTargets = assembly.GetCustomAttributes<InternalsVisibleToAttribute>()
            .Select(attribute => attribute.AssemblyName)
            .ToArray();

        List<string> violations = new();
        violations.AddRange(InternalsVisibleToViolations(internalsTargets, options.AllowedInternalsTargets));
        violations.AddRange(ReachabilityViolations(
            publicTypes, allTypes, type => type.Assembly == assembly, options));

        return new ArchitectureReport(violations);
    }

    internal static List<string> InternalsVisibleToViolations(
        IEnumerable<string> actualTargets, IEnumerable<string> allowedTargets)
    {
        HashSet<string> allowed = new(allowedTargets, StringComparer.Ordinal);
        return actualTargets
            .Where(target => !allowed.Contains(target))
            .Select(target => $"Model exposes internals to '{target}', which is not whitelisted. "
                + "Drive the Model through its public CQS surface instead.")
            .ToList();
    }

    internal static List<string> ReachabilityViolations(
        IReadOnlyCollection<Type> publicTypes,
        IReadOnlyCollection<Type> allTypes,
        Func<Type, bool> belongsToModel,
        ModelBoundaryOptions options)
    {
        Type[] handlers = allTypes
            .Where(type => type.IsClass && !type.IsAbstract)
            .Where(type => ImplementsOpenGeneric(type, typeof(ICommandHandler<>))
                || ImplementsOpenGeneric(type, typeof(IQueryHandler<,>)))
            .ToArray();

        HashSet<Type> commandAndQueryTypes = handlers
            .SelectMany(handler => GenericArguments(handler, typeof(ICommandHandler<>))
                .Concat(GenericArguments(handler, typeof(IQueryHandler<,>)).Take(1)))
            .ToHashSet();

        bool IsSurface(Type type) =>
            commandAndQueryTypes.Contains(type)
            || typeof(DomainMessage).IsAssignableFrom(type)
            || options.SurfaceSeeds.Any(predicate => predicate(type));

        HashSet<Type> reachable = new();
        Queue<Type> toVisit = new();

        foreach (Type seed in publicTypes.Where(IsSurface))
        {
            AddReachableType(seed, belongsToModel, allTypes, reachable, toVisit);
        }

        // A handler's Handle signature is the command/query contract: it pins down the request type and,
        // for queries, the result type. Walk it even though handlers are internal and never seeds themselves.
        foreach (Type handler in handlers)
        {
            foreach (MethodInfo handle in handler
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(method => method.Name == "Handle"))
            {
                AddSignatureTypes(handle.ReturnType, belongsToModel, allTypes, reachable, toVisit);
                foreach (ParameterInfo parameter in handle.GetParameters())
                {
                    AddSignatureTypes(parameter.ParameterType, belongsToModel, allTypes, reachable, toVisit);
                }
            }
        }

        WalkReachableGraph(belongsToModel, allTypes, reachable, toVisit);

        return publicTypes
            .Where(type => !reachable.Contains(type) && !options.ExcludedTypes.Contains(type))
            .Select(type => type.FullName ?? type.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .Select(name => $"Public type {name} is not reachable from the CQS surface (commands / queries / "
                + "events). Either it leaks Model internals, or declare it part of the surface via TreatAsSurface.")
            .ToList();
    }

    private static void WalkReachableGraph(
        Func<Type, bool> belongsToModel, IReadOnlyCollection<Type> allTypes,
        HashSet<Type> reachable, Queue<Type> toVisit)
    {
        const BindingFlags members =
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        while (toVisit.Count > 0)
        {
            Type current = toVisit.Dequeue();

            foreach (PropertyInfo property in current.GetProperties(members))
            {
                if (property.GetMethod != null)
                {
                    AddSignatureTypes(property.PropertyType, belongsToModel, allTypes, reachable, toVisit);
                }
            }

            foreach (EventInfo @event in current.GetEvents(members))
            {
                if (@event.EventHandlerType == null)
                {
                    continue;
                }

                foreach (Type argumentType in GetEventArgumentTypes(@event.EventHandlerType))
                {
                    AddSignatureTypes(argumentType, belongsToModel, allTypes, reachable, toVisit);
                }
            }

            foreach (MethodInfo method in current.GetMethods(members))
            {
                if (method.IsSpecialName)
                {
                    continue;
                }

                AddSignatureTypes(method.ReturnType, belongsToModel, allTypes, reachable, toVisit);
                foreach (ParameterInfo parameter in method.GetParameters())
                {
                    AddSignatureTypes(parameter.ParameterType, belongsToModel, allTypes, reachable, toVisit);
                }
            }
        }
    }

    private static void AddReachableType(
        Type type, Func<Type, bool> belongsToModel, IReadOnlyCollection<Type> allTypes,
        HashSet<Type> reachable, Queue<Type> toVisit)
    {
        if (type.IsGenericType && !type.IsGenericTypeDefinition)
        {
            AddReachableType(type.GetGenericTypeDefinition(), belongsToModel, allTypes, reachable, toVisit);
        }

        if (!belongsToModel(type) || !reachable.Add(type))
        {
            return;
        }

        toVisit.Enqueue(type);

        if (type.IsNested && type.DeclaringType != null)
        {
            AddReachableType(type.DeclaringType, belongsToModel, allTypes, reachable, toVisit);
        }

        if (type.BaseType != null)
        {
            AddReachableType(type.BaseType, belongsToModel, allTypes, reachable, toVisit);
        }

        foreach (Type implementedInterface in type.GetInterfaces())
        {
            AddReachableType(implementedInterface, belongsToModel, allTypes, reachable, toVisit);
        }

        foreach (Type nestedType in type.GetNestedTypes(BindingFlags.Public))
        {
            AddReachableType(nestedType, belongsToModel, allTypes, reachable, toVisit);
        }

        if (type.IsAbstract)
        {
            foreach (Type derivedType in allTypes
                .Where(candidate => candidate.IsPublic && !candidate.IsAbstract && candidate.IsSubclassOf(type)))
            {
                AddReachableType(derivedType, belongsToModel, allTypes, reachable, toVisit);
            }
        }
    }

    private static void AddSignatureTypes(
        Type type, Func<Type, bool> belongsToModel, IReadOnlyCollection<Type> allTypes,
        HashSet<Type> reachable, Queue<Type> toVisit)
    {
        foreach (Type unwrapped in UnwrapSignatureTypes(type, belongsToModel))
        {
            AddReachableType(unwrapped, belongsToModel, allTypes, reachable, toVisit);
        }
    }

    private static IEnumerable<Type> UnwrapSignatureTypes(Type type, Func<Type, bool> belongsToModel)
    {
        if (type.IsByRef || type.IsPointer || type.IsArray)
        {
            Type? elementType = type.GetElementType();
            if (elementType != null)
            {
                foreach (Type nested in UnwrapSignatureTypes(elementType, belongsToModel))
                {
                    yield return nested;
                }
            }

            yield break;
        }

        if (type.IsGenericParameter)
        {
            yield break;
        }

        Type? nullableUnderlyingType = Nullable.GetUnderlyingType(type);
        if (nullableUnderlyingType != null)
        {
            foreach (Type nested in UnwrapSignatureTypes(nullableUnderlyingType, belongsToModel))
            {
                yield return nested;
            }

            yield break;
        }

        if (belongsToModel(type))
        {
            yield return type;
        }

        if (!type.IsGenericType)
        {
            yield break;
        }

        foreach (Type genericArgument in type.GetGenericArguments())
        {
            foreach (Type nested in UnwrapSignatureTypes(genericArgument, belongsToModel))
            {
                yield return nested;
            }
        }
    }

    private static IEnumerable<Type> GetEventArgumentTypes(Type eventHandlerType)
    {
        if (eventHandlerType.IsGenericType)
        {
            return eventHandlerType.GetGenericArguments();
        }

        MethodInfo? invoke = eventHandlerType.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        if (invoke == null)
        {
            return [];
        }

        return invoke.GetParameters().Select(parameter => parameter.ParameterType).Append(invoke.ReturnType);
    }

    private static bool ImplementsOpenGeneric(Type type, Type openGeneric) =>
        type.GetInterfaces().Any(interfaceType =>
            interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == openGeneric);

    private static IEnumerable<Type> GenericArguments(Type type, Type openGeneric) =>
        type.GetInterfaces()
            .Where(interfaceType => interfaceType.IsGenericType
                && interfaceType.GetGenericTypeDefinition() == openGeneric)
            .SelectMany(interfaceType => interfaceType.GetGenericArguments());
}
