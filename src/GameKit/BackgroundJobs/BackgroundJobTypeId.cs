using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GameKit.BackgroundJobs;

internal static class BackgroundJobTypeId
{
    public const int Null = 0;
    private static int _nextId = 0;
    private static readonly Dictionary<Type, int> Lookup = new();

    [MethodImpl(MethodImplOptions.Synchronized)]
    internal static int GetId(Type type)
    {
        ref int value = ref CollectionsMarshal.GetValueRefOrAddDefault(Lookup, type, out bool exists);

        if (!exists)
        {
            value = ++_nextId;
        }

        return value;
    }
}

internal static class BackgroundJobTypeId<T>
{
    public static readonly int Id;

    static BackgroundJobTypeId()
    {
        Id = BackgroundJobTypeId.GetId(typeof(T));
    }
}
