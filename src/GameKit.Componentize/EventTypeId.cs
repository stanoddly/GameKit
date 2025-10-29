using System.Runtime.InteropServices;

namespace GameKit.Componentize;

internal static class EventTypeId
{
    private static int _nextId = 0;
    private static readonly Dictionary<Type, int> _lookup = new();

    public static int GetId(Type type)
    {
        ref int value = ref CollectionsMarshal.GetValueRefOrAddDefault(_lookup, type, out bool exists);
        
        if (!exists)
        {
            value = ++_nextId;
        }

        return value;
    }
}

public static class EventTypeId<TEventArgs>
{
    public static readonly int Id;

    static EventTypeId()
    {
        Id = EventTypeId.GetId(typeof(TEventArgs));
    }
}