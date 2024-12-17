using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GameKit.Collections;

public static class CollectionHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T UnsafeGetItem<T>(T[] array, nint index)
    {
        return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array), index);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T UnsafeGetItem<T>(ref T reference, nint index)
    {
        return ref Unsafe.Add(ref reference, index);
    }
}
