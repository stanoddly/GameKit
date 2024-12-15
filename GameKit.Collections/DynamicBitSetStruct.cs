using System.Numerics;

namespace GameKit.Collections;

using System.Runtime.CompilerServices;

public struct DynamicBitSetStruct
{
    private nuint[] _bits;
    private static readonly nuint BitsPerElement = (nuint)(Unsafe.SizeOf<nuint>() * 8);
    private static readonly int ShiftAmount = BitOperations.Log2((uint)BitsPerElement);
    private static readonly nuint BitMask = BitsPerElement - 1;

    public DynamicBitSetStruct(nuint initialCapacity = 64)
    {
        nuint arraySize = (initialCapacity + BitsPerElement - 1) / BitsPerElement;
        _bits = new nuint[(int)arraySize];
    }

    private void EnsureCapacity(nuint index)
    {
        nuint requiredArraySize = (index + BitsPerElement) >> ShiftAmount;
        if (requiredArraySize > (nuint)_bits.Length)
        {
            nuint newArraySize = Math.Max((nuint)_bits.Length * 2, requiredArraySize);
            nuint[] newBits = new nuint[(int)newArraySize];
            Array.Copy(_bits, newBits, _bits.Length);
            _bits = newBits;
        }
    }

    public void Set(nuint index, bool value)
    {
        EnsureCapacity(index);
        nuint arrayIndex = index >> ShiftAmount;
        nuint bitIndex = index & BitMask;

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
        if (index >= Capacity)
            return false;

        nuint arrayIndex = index >> ShiftAmount;
        nuint bitIndex = index & BitMask;
        return (_bits[(int)arrayIndex] & ((nuint)1 << (int)bitIndex)) != 0;
    }

    public bool GetSet(nuint index, bool value)
    {
        nuint arrayIndex = index >> ShiftAmount;
        nuint bitIndex = index & BitMask;
        
        bool previous = false;
        if (index < Capacity)
        {
            nuint mask = (nuint)1 << (int)bitIndex;
            previous = (_bits[(int)arrayIndex] & mask) != 0;
        }

        EnsureCapacity(index);
        
        nuint newMask = (nuint)1 << (int)bitIndex;
        if (value)
            _bits[(int)arrayIndex] |= newMask;
        else
            _bits[(int)arrayIndex] &= ~newMask;

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

    public void TrimExcess()
    {
        int lastUsedArrayIndex = 0;
        for (int i = _bits.Length - 1; i >= 0; i--)
        {
            if (_bits[i] != 0)
            {
                lastUsedArrayIndex = i;
                break;
            }
        }

        int newArraySize = lastUsedArrayIndex + 1;
        if (newArraySize < _bits.Length)
        {
            Array.Resize(ref _bits, newArraySize);
        }
    }

    public nuint Capacity => (nuint)_bits.Length << ShiftAmount;
}