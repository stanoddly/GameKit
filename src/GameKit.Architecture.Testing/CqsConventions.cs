using System.Reflection;
using System.Runtime.CompilerServices;

namespace GameKit.Architecture.Testing;

/// <summary>
/// Verifies the CQS conventions described in docs/architecture.md against one or more Model assemblies:
/// commands and queries are behaviourless data records, handlers are internal and constructor-injected,
/// and command handlers never depend on other command handlers.
/// </summary>
/// <remarks>
/// Roles are discovered through the GameKit.Architecture contracts, not name suffixes: a command is the
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

        Type[] concreteTypes = types
            .Where(type => !type.IsDefined(typeof(CompilerGeneratedAttribute), false))
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

        List<string> violations = new();

        CheckDataRecords(commandTypes, "Command", violations);
        CheckDataRecords(queryTypes, "Query", violations);
        CheckHandlerNaming(commandHandlers, "CommandHandler", violations);
        CheckHandlerNaming(queryHandlers, "QueryHandler", violations);
        CheckHandlersAreInternal(commandHandlers.Concat(queryHandlers), violations);
        CheckHandlersHaveNoPublicConstructors(commandHandlers.Concat(queryHandlers), violations);
        CheckCommandHandlersDoNotDependOnHandlers(commandHandlers, violations);
        CheckQueryResultsAreReadonly(queryResultTypes, violations);
        CheckQueryResultSuffix(queryResultTypes, options, violations);

        return new ArchitectureReport(violations);
    }

    private static void CheckDataRecords(Type[] types, string role, List<string> violations)
    {
        foreach (Type type in types)
        {
            if (!IsRecord(type))
            {
                violations.Add($"{role} {type.FullName} must be a record (intent is data, not behaviour).");
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

    private static void CheckQueryResultSuffix(
        Type[] resultTypes,
        CqsConventionsOptions options,
        List<string> violations)
    {
        if (!options.RequiresQueryResultSuffix)
        {
            return;
        }

        foreach (Type resultType in resultTypes)
        {
            if (resultType.Name.EndsWith("Result", StringComparison.Ordinal))
            {
                continue;
            }

            violations.Add(
                $"Query result {resultType.FullName} should be a named result type ending with 'Result'.");
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
