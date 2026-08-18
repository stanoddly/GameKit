namespace Pixely;

/// <summary>Domain root for a compile-time type id allocator. The self-referential <typeparamref name="TDomain"/> constraint scopes the id sequence — each closed <see cref="StaticTypeIdMap{TDomain}"/> has its own counter.</summary>
/// <typeparam name="TDomain">The marker class that defines this id domain; must derive from <see cref="StaticTypeIdMap{TDomain}"/>.</typeparam>
/// <remarks>Intended for callers that know the service type at compile time. Callers that need to resolve a runtime <see cref="Type"/> to an id should use <see cref="TypeIdMap{TDomain}.GetId(Type)"/> instead.</remarks>
public class StaticTypeIdMap<TDomain> where TDomain : StaticTypeIdMap<TDomain>
{
    protected StaticTypeIdMap() { }

    private static int _nextId;

    /// <summary>Returns the next id in this domain's sequence. Called once per closed <see cref="StaticTypeIdMap{TDomain, T}"/> from its static initializer.</summary>
    protected static int AllocateId()
    {
        return Interlocked.Increment(ref _nextId) - 1;
    }
}

/// <summary>Exposes a dense, process-wide id for <typeparamref name="T"/> within the <typeparamref name="TDomain"/> sequence. The id is assigned once by the CLR under its per-type init lock on first access and cached in <see cref="Id"/> — no dictionary, no runtime <see cref="Type"/> → id lookup thereafter.</summary>
/// <typeparam name="TDomain">The id domain; all <c>StaticTypeIdMap&lt;TDomain, …&gt;</c> share one counter.</typeparam>
/// <typeparam name="T">The type whose id is being allocated.</typeparam>
public class StaticTypeIdMap<TDomain, T> : StaticTypeIdMap<TDomain>
    where TDomain : StaticTypeIdMap<TDomain>
    where T : allows ref struct
{
    protected StaticTypeIdMap() { }

    /// <summary>The id assigned to <typeparamref name="T"/> within <typeparamref name="TDomain"/>.</summary>
    public static readonly int Id = AllocateId();

    /// <summary>The short name of <typeparamref name="T"/>, cached here so callers don't need to re-invoke <c>typeof(T).Name</c>.</summary>
    public static readonly string Name = typeof(T).Name;
}
