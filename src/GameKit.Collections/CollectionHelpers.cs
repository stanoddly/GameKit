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
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UnsafeSetItem<T>(T[] array, nint index, in T value)
    {
        ref T item = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array), index);
        item = value;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UnsafeSetItem<T>(ref T reference, nint index, in T value)
    {
        ref T item = ref Unsafe.Add(ref reference, index);
        item = value;
    }
}
