using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GameKit.Encs;

internal static class ComponentTypeId
{
    public const int Null = 0;
    private static nuint _nextId = 0;
    private static readonly Dictionary<Type, nuint> Lookup = new();

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static nuint GetId(Type type)
    {
        ref nuint value = ref CollectionsMarshal.GetValueRefOrAddDefault(Lookup, type, out bool exists);
        
        if (!exists)
        {
            value = ++_nextId;
        }

        return value;
    }
}

internal static class ComponentTypeId<T>
{
    public static readonly nuint Id;
    public static readonly string Name;

    static ComponentTypeId()
    {
        Id = ComponentTypeId.GetId(typeof(T));
        Name = typeof(T).Name;
    }
}