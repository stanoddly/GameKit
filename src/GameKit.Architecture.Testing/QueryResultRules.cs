using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace GameKit.Architecture.Testing;

/// <summary>
/// Decides whether a query result type is readonly from the consumer's perspective: recursively, every readable
/// member is non-externally-mutable (get/init/readonly/const or non-public) and every member type is itself
/// readonly. Known-immutable scalar and collection types short-circuit the walk.
/// </summary>
internal static class QueryResultRules
{
    private static readonly HashSet<Type> KnownImmutableScalars =
    [
        typeof(string), typeof(decimal), typeof(Guid),
        typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan),
        typeof(DateOnly), typeof(TimeOnly), typeof(Uri), typeof(Version),
    ];

    private static readonly HashSet<Type> KnownImmutableGenerics =
    [
        typeof(IEnumerable<>), typeof(IReadOnlyList<>), typeof(IReadOnlyCollection<>),
        typeof(IReadOnlyDictionary<,>), typeof(IReadOnlySet<>),
        typeof(ImmutableArray<>), typeof(ImmutableList<>), typeof(ImmutableHashSet<>),
        typeof(ImmutableSortedSet<>), typeof(ImmutableDictionary<,>), typeof(ImmutableSortedDictionary<,>),
        typeof(ImmutableQueue<>), typeof(ImmutableStack<>),
        typeof(ValueTuple<>), typeof(ValueTuple<,>), typeof(ValueTuple<,,>), typeof(ValueTuple<,,,>),
        typeof(ValueTuple<,,,,>), typeof(ValueTuple<,,,,,>), typeof(ValueTuple<,,,,,,>), typeof(ValueTuple<,,,,,,,>),
    ];

    public static bool IsReadonly(Type type, out string reason) =>
        IsReadonly(type, new HashSet<Type>(), out reason);

    private static bool IsReadonly(Type type, HashSet<Type> visiting, out string reason)
    {
        reason = string.Empty;

        Type? nullableUnderlying = Nullable.GetUnderlyingType(type);
        if (nullableUnderlying != null)
        {
            type = nullableUnderlying;
        }

        if (type.IsGenericParameter || type.IsPrimitive || type.IsEnum || KnownImmutableScalars.Contains(type))
        {
            return true;
        }

        if (type.IsArray)
        {
            reason = $"{type.Name} is an array; expose IReadOnlyList<T> or ImmutableArray<T>";
            return false;
        }

        if (!visiting.Add(type))
        {
            return true;
        }

        try
        {
            if (type.IsGenericType)
            {
                foreach (Type argument in type.GetGenericArguments())
                {
                    if (!IsReadonly(argument, visiting, out reason))
                    {
                        return false;
                    }
                }

                if (KnownImmutableGenerics.Contains(type.GetGenericTypeDefinition()))
                {
                    return true;
                }
            }

            foreach (PropertyInfo property in type.GetProperties(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                MethodInfo? setter = property.SetMethod;
                if (setter != null && setter.IsPublic && !IsInitOnly(setter))
                {
                    reason = $"{type.Name}.{property.Name} has a public setter";
                    return false;
                }

                MethodInfo? getter = property.GetMethod;
                if (getter != null && getter.IsPublic && !IsReadonly(property.PropertyType, visiting, out reason))
                {
                    reason = $"{type.Name}.{property.Name}: {reason}";
                    return false;
                }
            }

            foreach (FieldInfo field in type.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (field.IsDefined(typeof(CompilerGeneratedAttribute), false))
                {
                    continue;
                }

                if (field.IsPublic && !field.IsInitOnly && !field.IsLiteral)
                {
                    reason = $"{type.Name}.{field.Name} is a public mutable field";
                    return false;
                }

                if (field.IsPublic && !IsReadonly(field.FieldType, visiting, out reason))
                {
                    reason = $"{type.Name}.{field.Name}: {reason}";
                    return false;
                }
            }

            return true;
        }
        finally
        {
            visiting.Remove(type);
        }
    }

    private static bool IsInitOnly(MethodInfo setter) =>
        setter.ReturnParameter.GetRequiredCustomModifiers()
            .Any(modifier => modifier.FullName == "System.Runtime.CompilerServices.IsExternalInit");
}
