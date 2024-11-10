using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GameKit.EntityComponent;

internal static class TypeId
{
    public const int Null = 0;
    private static int _nextId = 0;
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

internal static class TypeId<T>
{
    public static readonly int Id;
    public static readonly string Name;

    static TypeId()
    {
        Id = TypeId.GetId(typeof(T));
        Name = typeof(T).Name;
    }
}