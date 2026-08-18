using System.Collections.Concurrent;

namespace Pixely;

/// <summary>Domain root for a runtime type id allocator. Supports both compile-time id lookup via <see cref="TypeIdMap{TDomain, T}.Id"/> and runtime <see cref="Type"/> → id resolution via <see cref="GetId(Type)"/>.</summary>
/// <typeparam name="TDomain">The marker class that defines this id domain; must derive from <see cref="TypeIdMap{TDomain}"/>.</typeparam>
/// <remarks>Callers that only need compile-time ids should use <see cref="StaticTypeIdMap{TDomain, T}"/> instead — it has no dictionary and no allocation per type.</remarks>
public class TypeIdMap<TDomain> where TDomain : TypeIdMap<TDomain>
{
    protected TypeIdMap() { }

    private static int _nextId;
    private static readonly ConcurrentDictionary<Type, Lazy<int>> Lookup = new();
    private static readonly Func<Type, Lazy<int>> LazyFactory =
        static _ => new Lazy<int>(static () => Interlocked.Increment(ref _nextId) - 1);

    /// <summary>Returns the id assigned to <paramref name="type"/>, allocating a new one on first call.</summary>
    /// <param name="type">The type whose id to resolve.</param>
    /// <returns>The dense, process-wide id of <paramref name="type"/> within <typeparamref name="TDomain"/>.</returns>
    /// <remarks>Thread-safe. The <see cref="Lazy{T}"/> wrapper defers the <see cref="Interlocked.Increment(ref int)"/> until after <see cref="ConcurrentDictionary{TKey, TValue}.GetOrAdd(TKey, Func{TKey, TValue})"/> picks a winner — racing factory calls only allocate throwaway <see cref="Lazy{T}"/> instances, so the counter advances exactly once per type and ids stay contiguous.</remarks>
    public static int GetId(Type type)
    {
        return Lookup.GetOrAdd(type, LazyFactory).Value;
    }
}

/// <summary>Exposes the id of <typeparamref name="T"/> within the <typeparamref name="TDomain"/> sequence, cached in <see cref="Id"/> on first access.</summary>
/// <typeparam name="TDomain">The id domain; shared with <see cref="TypeIdMap{TDomain}.GetId(Type)"/>.</typeparam>
/// <typeparam name="T">The type whose id is being allocated.</typeparam>
public class TypeIdMap<TDomain, T> : TypeIdMap<TDomain>
    where TDomain : TypeIdMap<TDomain>
    where T : allows ref struct
{
    protected TypeIdMap() { }

    /// <summary>The id assigned to <typeparamref name="T"/>. Equal to <c>TypeIdMap&lt;TDomain&gt;.GetId(typeof(T))</c> but resolved once at type-init time.</summary>
    public static readonly int Id = GetId(typeof(T));

    /// <summary>The short name of <typeparamref name="T"/>, cached here so callers don't need to re-invoke <c>typeof(T).Name</c>.</summary>
    public static readonly string Name = typeof(T).Name;
}
