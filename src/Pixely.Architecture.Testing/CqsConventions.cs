using System.Reflection;
using System.Runtime.CompilerServices;
using Pixely.Architecture.Events;

namespace Pixely.Architecture.Testing;

/// <summary>
/// Verifies the CQS conventions described in docs/architecture.md against one or more Model assemblies:
/// commands and queries are behaviourless data records, handlers are internal and constructor-injected,
/// and command handlers never depend on other command handlers.
/// </summary>
/// <remarks>
/// Roles are discovered through the Pixely.Architecture contracts, not name suffixes: a command is the
/// <c>TCommand</c> of an <see cref="ICommandHandler{TCommand}"/>, a query is the <c>TQuery</c> of an
/// <see cref="IQueryHandler{TQuery, TResult}"/>, and handlers are the implementing types.
/// </remarks>
public static class CqsConventions
{
    public static ArchitectureReport Check(params Assembly[] assemblies)
    {
        return Check(null, assemblies);
    }

    public static ArchitectureReport Check(Action<CqsConventionsOptions>? configure, params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            throw new ArgumentException("At least one assembly must be supplied.", nameof(assemblies));
        }

        return CheckTypes(assemblies.SelectMany(assembly => assembly.GetTypes()), configure);
    }

    internal static ArchitectureReport CheckTypes(
        IEnumerable<Type> types,
        Action<CqsConventionsOptions>? configure = null)
    {
        CqsConventionsOptions options = new();
        configure?.Invoke(options);

        Type[] allTypes = types
            .Where(type => !type.IsDefined(typeof(CompilerGeneratedAttribute), false))
            .Distinct()
            .ToArray();

        Type[] concreteTypes = allTypes
            .Where(type => type.IsClass && !type.IsAbstract)
            .ToArray();

        Type[] commandHandlers = concreteTypes
            .Where(type => ImplementsOpenGeneric(type, typeof(ICommandHandler<>)))
            .ToArray();

        Type[] queryHandlers = concreteTypes
            .Where(type => ImplementsOpenGeneric(type, typeof(IQueryHandler<,>)))
            .ToArray();

        Type[] commandTypes = commandHandlers
            .SelectMany(handler => GenericArguments(handler, typeof(ICommandHandler<>)))
            .Distinct()
            .ToArray();

        Type[] queryTypes = queryHandlers
            .SelectMany(handler => QueryInterfaces(handler).Select(query => query.GetGenericArguments()[0]))
            .Distinct()
            .ToArray();

        Type[] queryResultTypes = queryHandlers
            .SelectMany(handler => QueryInterfaces(handler).Select(query => query.GetGenericArguments()[1]))
            .Distinct()
            .ToArray();

        Type[] eventTypes = allTypes
            .Where(type => type != typeof(DomainMessage) && typeof(DomainMessage).IsAssignableFrom(type))
            .ToArray();

        List<string> violations = new();

        CheckDataRecords(commandTypes, "Command", violations);
        CheckDataRecords(queryTypes, "Query", violations);
        CheckHandlerNaming(commandHandlers, "CommandHandler", violations);
        CheckHandlerNaming(queryHandlers, "QueryHandler", violations);
        CheckHandlersAreInternal(commandHandlers.Concat(queryHandlers), violations);
        CheckHandlersHaveNoPublicConstructors(commandHandlers.Concat(queryHandlers), violations);
        CheckCommandHandlersDoNotDependOnHandlers(commandHandlers, violations);
        CheckQueryResultsAreReadonly(queryResultTypes, violations);
        CheckBdoConventions(allTypes, commandTypes, queryTypes, queryResultTypes, eventTypes, options, violations);

        return new ArchitectureReport(violations);
    }

    private static void CheckDataRecords(Type[] types, string role, List<string> violations)
    {
        foreach (Type type in types)
        {
            if (!IsRecord(type))
            {
                violations.Add($"{role} {type.FullName} must be a record (the boundary contract is data, not behaviour).");
            }

            if (!HasNoCustomMethods(type))
            {
                violations.Add($"{role} {type.FullName} must be a plain data record with no custom methods.");
            }
        }
    }

    private static void CheckHandlerNaming(Type[] handlers, string suffix, List<string> violations)
    {
        foreach (Type handler in handlers)
        {
            if (!handler.Name.EndsWith(suffix, StringComparison.Ordinal))
            {
                violations.Add($"{handler.FullName} should end with '{suffix}'.");
            }
        }
    }

    private static void CheckHandlersAreInternal(IEnumerable<Type> handlers, List<string> violations)
    {
        foreach (Type handler in handlers)
        {
            if (handler.IsVisible)
            {
                violations.Add(
                    $"Handler {handler.FullName} must not be public — callers reach it through the dispatcher, not directly.");
            }
        }
    }

    private static void CheckHandlersHaveNoPublicConstructors(IEnumerable<Type> handlers, List<string> violations)
    {
        foreach (Type handler in handlers)
        {
            if (handler.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Length > 0)
            {
                violations.Add($"Handler {handler.FullName} must have no public constructors (use internal for DI).");
            }
        }
    }

    private static void CheckCommandHandlersDoNotDependOnHandlers(Type[] commandHandlers, List<string> violations)
    {
        foreach (Type commandHandler in commandHandlers)
        {
            ConstructorInfo[] constructors = commandHandler.GetConstructors(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (ConstructorInfo constructor in constructors)
            {
                foreach (ParameterInfo parameter in constructor.GetParameters())
                {
                    Type? dependency = FindHandlerDependency(parameter.ParameterType);
                    if (dependency != null)
                    {
                        violations.Add(
                            $"Command handler {commandHandler.FullName} depends on handler {dependency.FullName} "
                            + $"(parameter '{parameter.Name}'). Move shared behaviour into a domain service.");
                    }
                }
            }
        }
    }

    private static void CheckQueryResultsAreReadonly(Type[] resultTypes, List<string> violations)
    {
        foreach (Type resultType in resultTypes)
        {
            if (!QueryResultRules.IsReadonly(resultType, out string reason))
            {
                violations.Add($"Query result {resultType.FullName} must be readonly: {reason}");
            }
        }
    }

    private static void CheckBdoConventions(
        Type[] allTypes,
        Type[] commandTypes,
        Type[] queryTypes,
        Type[] resultTypes,
        Type[] eventTypes,
        CqsConventionsOptions options,
        List<string> violations)
    {
        if (!options.RequiresBdoSuffix)
        {
            return;
        }

        foreach (Type resultType in resultTypes)
        {
            if (HasSuffix(resultType, "Bdo"))
            {
                continue;
            }

            violations.Add(
                $"Query result {resultType.FullName} must be a boundary data object ending with 'Bdo'.");
        }

        Type[] bdoTypes = allTypes
            .Where(type => HasSuffix(type, "Bdo"))
            .ToArray();

        CheckDataRecords(bdoTypes, "BDO", violations);
        CheckBdosAreReadonly(bdoTypes.Except(resultTypes), violations);

        IEnumerable<Type> boundaryRoots = commandTypes.Concat(queryTypes).Concat(resultTypes).Concat(eventTypes);
        HashSet<Type> boundaryGraph = DataGraph(boundaryRoots, allTypes);
        foreach (Type bdoType in bdoTypes.Where(type => !boundaryGraph.Contains(type)))
        {
            violations.Add($"BDO {bdoType.FullName} must belong to a Model boundary graph.");
        }
    }

    private static void CheckBdosAreReadonly(IEnumerable<Type> bdoTypes, List<string> violations)
    {
        foreach (Type bdoType in bdoTypes)
        {
            if (!QueryResultRules.IsReadonly(bdoType, out string reason))
            {
                violations.Add($"BDO {bdoType.FullName} must be read-only to consumers: {reason}");
            }
        }
    }

    private static HashSet<Type> DataGraph(IEnumerable<Type> roots, IReadOnlyCollection<Type> allTypes)
    {
        HashSet<Type> availableTypes = allTypes.ToHashSet();
        HashSet<Type> graph = new();
        Queue<Type> toVisit = new(roots);

        while (toVisit.Count > 0)
        {
            Type current = toVisit.Dequeue();
            foreach (Type candidate in UnwrapDataType(current))
            {
                Type graphType = candidate.IsGenericType && !candidate.IsGenericTypeDefinition
                    ? candidate.GetGenericTypeDefinition()
                    : candidate;

                if (!availableTypes.Contains(graphType) || !graph.Add(graphType))
                {
                    continue;
                }

                foreach (PropertyInfo property in candidate.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (property.GetMethod != null)
                    {
                        toVisit.Enqueue(property.PropertyType);
                    }
                }

                foreach (FieldInfo field in candidate.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    toVisit.Enqueue(field.FieldType);
                }
            }
        }

        return graph;
    }

    private static bool HasSuffix(Type type, string suffix)
    {
        string name = type.Name;
        int genericAritySeparator = name.IndexOf('`');
        if (genericAritySeparator >= 0)
        {
            name = name[..genericAritySeparator];
        }

        return name.EndsWith(suffix, StringComparison.Ordinal);
    }

    private static IEnumerable<Type> UnwrapDataType(Type type)
    {
        if (type.IsByRef || type.IsPointer || type.IsArray)
        {
            Type? elementType = type.GetElementType();
            if (elementType != null)
            {
                foreach (Type nested in UnwrapDataType(elementType))
                {
                    yield return nested;
                }
            }

            yield break;
        }

        Type? nullableUnderlyingType = Nullable.GetUnderlyingType(type);
        if (nullableUnderlyingType != null)
        {
            foreach (Type nested in UnwrapDataType(nullableUnderlyingType))
            {
                yield return nested;
            }

            yield break;
        }

        yield return type;

        if (!type.IsGenericType)
        {
            yield break;
        }

        foreach (Type genericArgument in type.GetGenericArguments())
        {
            foreach (Type nested in UnwrapDataType(genericArgument))
            {
                yield return nested;
            }
        }
    }

    private static IEnumerable<Type> QueryInterfaces(Type handler) =>
        handler.GetInterfaces()
            .Where(interfaceType => interfaceType.IsGenericType
                && interfaceType.GetGenericTypeDefinition() == typeof(IQueryHandler<,>));

    private static Type? FindHandlerDependency(Type type)
    {
        if (type.IsByRef || type.IsPointer || type.IsArray)
        {
            Type? elementType = type.GetElementType();
            return elementType == null ? null : FindHandlerDependency(elementType);
        }

        Type? nullableUnderlyingType = Nullable.GetUnderlyingType(type);
        if (nullableUnderlyingType != null)
        {
            return FindHandlerDependency(nullableUnderlyingType);
        }

        if (IsCommandHandlerContract(type) || ImplementsOpenGeneric(type, typeof(ICommandHandler<>)))
        {
            return type;
        }

        if (!type.IsGenericType)
        {
            return null;
        }

        foreach (Type genericArgument in type.GetGenericArguments())
        {
            Type? dependency = FindHandlerDependency(genericArgument);
            if (dependency != null)
            {
                return dependency;
            }
        }

        return null;
    }

    private static bool IsCommandHandlerContract(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICommandHandler<>);

    private static bool ImplementsOpenGeneric(Type type, Type openGeneric) =>
        type.GetInterfaces().Any(interfaceType =>
            interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == openGeneric);

    private static IEnumerable<Type> GenericArguments(Type type, Type openGeneric) =>
        type.GetInterfaces()
            .Where(interfaceType => interfaceType.IsGenericType
                && interfaceType.GetGenericTypeDefinition() == openGeneric)
            .SelectMany(interfaceType => interfaceType.GetGenericArguments());

    private static bool IsRecord(Type type)
    {
        if (type.IsEnum)
        {
            return false;
        }

        // Both record classes and record structs carry a compiler-generated PrintMembers method.
        MethodInfo? printMembers = type.GetMethod("PrintMembers", BindingFlags.Instance | BindingFlags.NonPublic);
        return printMembers != null && printMembers.IsDefined(typeof(CompilerGeneratedAttribute), false);
    }

    private static bool HasNoCustomMethods(Type type)
    {
        MethodInfo[] methods = type.GetMethods(
            BindingFlags.Instance | BindingFlags.Static
            | BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.DeclaredOnly);

        foreach (MethodInfo method in methods)
        {
            if (method.IsSpecialName || method.IsDefined(typeof(CompilerGeneratedAttribute), false))
            {
                continue;
            }

            // Methods the record machinery emits or that records legitimately override.
            if (method.Name is "ToString" or "PrintMembers" or "GetHashCode" or "Equals" or "Deconstruct")
            {
                continue;
            }

            return false;
        }

        return true;
    }
}
