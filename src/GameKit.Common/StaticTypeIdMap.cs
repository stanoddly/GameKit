namespace GameKit.Common;

// Compile-time type id allocator. The id is assigned in the static initializer of
// StaticTypeIdMap<TDomain, T> — one Interlocked.Increment per closed generic, performed
// once by the CLR under its type-init lock. No dictionary, no runtime Type → int lookup.
//
// Intended for callers that know T at compile time (generic entry points, intercepted
// registrations). Callers that need to go from a runtime Type to an id should use the
// separate TypeIdMap<TDomain>.GetId(Type) API instead.
public class StaticTypeIdMap<TDomain> where TDomain : StaticTypeIdMap<TDomain>
{
    protected StaticTypeIdMap() { }

    private static int _nextId;

    protected static int AllocateId()
    {
        return Interlocked.Increment(ref _nextId) - 1;
    }
}

public class StaticTypeIdMap<TDomain, T> : StaticTypeIdMap<TDomain>
    where TDomain : StaticTypeIdMap<TDomain>
    where T : allows ref struct
{
    protected StaticTypeIdMap() { }

    public static readonly int Id = AllocateId();
    public static readonly string Name = typeof(T).Name;
}
