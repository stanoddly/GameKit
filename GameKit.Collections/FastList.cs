using System.Diagnostics.CodeAnalysis;

namespace GameKit.Collections;

public class FastList<TValue>
{
    private TValue[] _items;
    public int Length { get; private set; }
    public ref TValue this[int index] => ref _items[index];

    public FastList()
    {
        _items = new TValue[64];
    }
    
    public FastList(int initialLength)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(initialLength, 1, nameof(initialLength));
        _items = new TValue[initialLength];
    }

    public int Add(in TValue value)
    {
        if (Length >= _items.Length)
        {
            TValue[] newItems = new TValue[Length + Length / 2];
            Array.Copy(_items, newItems, Length);

            _items = newItems;
        }
        
        _items[Length] = value;

        return Length++;
    }

    public void ResizeFill(int length, TValue defaultValue)
    {
        if (length <= Length)
        {
            Length = length;
            return;
        }

        int arrayLength = _items.Length;
        while (arrayLength < length)
        {
            arrayLength += arrayLength / 2;
        }
        
        Array.Resize(ref _items, arrayLength);
        
        Array.Fill(_items, defaultValue, Length, length - Length);
        
        Length = length;
    }
    
    public bool SwapRemove(int index, [NotNullWhen(true)] out TValue? swappedValue)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Length, nameof(index));

        --Length;
        
        // either empty or last one, no need for replacement
        if (Length == 0 || index == Length)
        {
            _items[Length] = default!;
            swappedValue = default;
            return false;
        }

        ref TValue item = ref _items[Length];
        _items[index] = item;
        swappedValue = item;
        item = default!;

        return true;
    }

    public void RemoveLast()
    {
        if (Length > 0)
        {
            --Length;
        }
    }
    
    public bool SwapRemove(int index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Length, nameof(index));

        --Length;
        
        // either empty or last one, no need for replacement
        if (Length == 0 || index == Length)
        {
            _items[Length] = default!;
            return false;
        }

        ref TValue item = ref _items[Length];
        _items[index] = _items[Length];
        item = default!;

        return true;
    }
    
    public int LastIndex => Length - 1;
    public int NextIndex => Length;

    public Span<TValue> AsSpan()
    {
        return new Span<TValue>(_items, 0, Length);
    }
    
    public ReadOnlySpan<TValue> AsReadOnlySpan()
    {
        return new ReadOnlySpan<TValue>(_items, 0, Length);
    }
}