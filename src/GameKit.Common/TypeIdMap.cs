using System.Collections.Concurrent;

namespace GameKit.Common;

// Lazy<int> defers the Interlocked.Increment until after GetOrAdd picks a winner, so
// racing factory calls only allocate throwaway Lazy wrappers — the counter advances
// exactly once per type and IDs stay contiguous.
public class TypeIdMap<TDomain> where TDomain : TypeIdMap<TDomain>
{
    protected TypeIdMap() { }

    private static int _nextId;
    private static readonly ConcurrentDictionary<Type, Lazy<int>> Lookup = new();
    private static readonly Func<Type, Lazy<int>> LazyFactory =
        static _ => new Lazy<int>(static () => Interlocked.Increment(ref _nextId) - 1);

    public static int GetId(Type type)
    {
        return Lookup.GetOrAdd(type, LazyFactory).Value;
    }
}

public class TypeIdMap<TDomain, T> : TypeIdMap<TDomain>
    where TDomain : TypeIdMap<TDomain>
    where T : allows ref struct
{
    protected TypeIdMap() { }

    public static readonly int Id = GetId(typeof(T));
    public static readonly string Name = typeof(T).Name;
}
