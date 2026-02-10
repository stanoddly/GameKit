using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GameKit.Common;

public class TypeIdMap<TDomain> where TDomain : TypeIdMap<TDomain>
{
    private static int _nextId;
    private static readonly Dictionary<Type, int> Lookup = new();

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static int GetId(Type type)
    {
        ref int value = ref CollectionsMarshal.GetValueRefOrAddDefault(Lookup, type, out bool exists);

        if (!exists)
        {
            value = ++_nextId;
        }

        return value;
    }
}

public class TypeIdMap<TDomain, T> : TypeIdMap<TDomain>
    where TDomain : TypeIdMap<TDomain>
    where T : allows ref struct
{
    public static readonly int Id = GetId(typeof(T));
    public static readonly string Name = typeof(T).Name;
}
