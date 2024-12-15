namespace GameKit.Collections;

using System.Runtime.CompilerServices;

public readonly struct BitSetStruct
{
    private readonly nuint[] _bits;
    private static readonly nuint BitsPerElement = (nuint)(Unsafe.SizeOf<nuint>() * 8);

    public BitSetStruct(nuint capacity)
    {
        nuint arraySize = (capacity + BitsPerElement - 1) / BitsPerElement;
        _bits = new nuint[(int)arraySize];
    }
    
    public void Set(nuint index, bool value)
    {
        nuint arrayIndex = index / BitsPerElement;
        nuint bitIndex = index % BitsPerElement;

        if (value)
        {
            _bits[(int)arrayIndex] |= (nuint)1 << (int)bitIndex;
        }
        else
        {
            _bits[(int)arrayIndex] &= ~((nuint)1 << (int)bitIndex);
        }
    }
    
    public bool Get(nuint index)
    {
        nuint arrayIndex = index / BitsPerElement;
        nuint bitIndex = index % BitsPerElement;
        return (_bits[(int)arrayIndex] & ((nuint)1 << (int)bitIndex)) != 0;
    }
    
    public bool GetSet(nuint index, bool value)
    {
        nuint arrayIndex = index / BitsPerElement;
        nuint bitIndex = index % BitsPerElement;
        nuint mask = (nuint)1 << (int)bitIndex;
        bool previous = (_bits[(int)arrayIndex] & mask) != 0;
   
        if (value)
            _bits[(int)arrayIndex] |= mask;
        else
            _bits[(int)arrayIndex] &= ~mask;
       
        return previous;
    }

    public bool this[nuint index]
    {
        get => Get(index);
        set => Set(index, value);
    }
    
    public void ClearAll()
    {
        Array.Clear(_bits, 0, _bits.Length);
    }
}