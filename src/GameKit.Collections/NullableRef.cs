using System.Numerics;
using System.Runtime.CompilerServices;

namespace GameKit.Collections;

public readonly ref struct NullableRef<T>
{
    private readonly ref T _reference;

    public NullableRef(ref T reference)
    {
        _reference = ref reference;
    }
    
    bool HasValue => !Unsafe.IsNullRef(ref _reference);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TrySetIfDifferent<TOther>(in TOther value)
        where TOther: IEqualityOperators<TOther, T, bool>, T
    {
        if (Unsafe.IsNullRef(ref _reference))
        {
            return false;
        }

        if (value == _reference)
        {
            return false;
        }

        _reference = value;

        return true;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TrySetIfExists<TOther>(in TOther value) where TOther: T
    {
        if (Unsafe.IsNullRef(ref _reference))
        {
            return false;
        }

        _reference = value;

        return true;
    }
    
    public ref T Value => ref _reference;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T TryGet(out bool exists)
    {
        exists = !Unsafe.IsNullRef(ref _reference);
        return ref (exists ?  ref _reference : ref _default);
    }

    private static T _default = default!;
    public static ref T Default => ref _default;
    public static NullableRef<T> Null => default;
}