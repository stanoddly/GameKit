// Generated using jinja2-cli: jinja2 MultiArray.cs.jinja > MultiArray.cs
using System;

namespace GameKit.Collections;

public class MultiArray<TValue1, TValue2>
{
    private const int DefaultCapacity = 32;
    private int _count;
    
    private TValue1[] _values1;
    private TValue2[] _values2;

    public MultiArray(int initialCapacity)
    {
        if (initialCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        
        _values1 = new TValue1[initialCapacity];
        _values2 = new TValue2[initialCapacity];
    }

    public MultiArray()
    {
        _values1 = new TValue1[DefaultCapacity];
        _values2 = new TValue2[DefaultCapacity];
        
    }

    public int Count => _count;

    private void EnsureCapacity(int minCapacity)
    {
        int capacity = _values1.Length;
        if (minCapacity > capacity)
        {
            int newCapacity = Math.Max(capacity * 2, minCapacity);

            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
        }
    }

    public int Add(TValue1 value1, TValue2 value2)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;

        return _count++;
    }

    public void Set(int index, TValue1 value1, TValue2 value2)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));
            
        
        _values1[index] = value1;
        
        _values2[index] = value2;
        
    }

    
    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    

    public bool SwapRemove(int index)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            return false;
        }

        _count--;
        return true;
    }
    
    public bool TryGet(int index, out TValue1 value1, out TValue2 value2)
    {
        if (index < _count)
        {
            
            value1 = _values1[index];
            value2 = _values2[index];

            return true;
        }

        
        value1 = default;
        value2 = default;
        return false;
    }
    
    public bool TryGetButFirst(int index, out TValue2 value2)
    {
        if (index < _count)
        {
            
            value2 = _values2[index];

            return true;
        }

        
        value2 = default;
        return false;
    }

    public bool SwapRemoveReturnFirst(int index, out TValue1 value1)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            value1 = _values1[index];
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            return true;
        }

        value1 = default!;
        _count--;
        return false;
    }

    public void Clear()
    {
        
        Array.Clear(_values1, 0, _count);
        Array.Clear(_values2, 0, _count);
        _count = 0;
    }

    public void TrimExcess()
    {
        int capacity = _values1.Length;
        if (_count < capacity * 0.9)
        {
            int newCapacity = Math.Max(DefaultCapacity, _count);
            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
        }
    }
}

public class MultiArray<TValue1, TValue2, TValue3>
{
    private const int DefaultCapacity = 32;
    private int _count;
    
    private TValue1[] _values1;
    private TValue2[] _values2;
    private TValue3[] _values3;

    public MultiArray(int initialCapacity)
    {
        if (initialCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        
        _values1 = new TValue1[initialCapacity];
        _values2 = new TValue2[initialCapacity];
        _values3 = new TValue3[initialCapacity];
    }

    public MultiArray()
    {
        _values1 = new TValue1[DefaultCapacity];
        _values2 = new TValue2[DefaultCapacity];
        _values3 = new TValue3[DefaultCapacity];
        
    }

    public int Count => _count;

    private void EnsureCapacity(int minCapacity)
    {
        int capacity = _values1.Length;
        if (minCapacity > capacity)
        {
            int newCapacity = Math.Max(capacity * 2, minCapacity);

            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
        }
    }

    public int Add(TValue1 value1, TValue2 value2, TValue3 value3)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;

        return _count++;
    }

    public void Set(int index, TValue1 value1, TValue2 value2, TValue3 value3)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));
            
        
        _values1[index] = value1;
        
        _values2[index] = value2;
        
        _values3[index] = value3;
        
    }

    
    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    

    public bool SwapRemove(int index)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            return false;
        }

        _count--;
        return true;
    }
    
    public bool TryGet(int index, out TValue1 value1, out TValue2 value2, out TValue3 value3)
    {
        if (index < _count)
        {
            
            value1 = _values1[index];
            value2 = _values2[index];
            value3 = _values3[index];

            return true;
        }

        
        value1 = default;
        value2 = default;
        value3 = default;
        return false;
    }
    
    public bool TryGetButFirst(int index, out TValue2 value2, out TValue3 value3)
    {
        if (index < _count)
        {
            
            value2 = _values2[index];
            value3 = _values3[index];

            return true;
        }

        
        value2 = default;
        value3 = default;
        return false;
    }

    public bool SwapRemoveReturnFirst(int index, out TValue1 value1)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            value1 = _values1[index];
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            return true;
        }

        value1 = default!;
        _count--;
        return false;
    }

    public void Clear()
    {
        
        Array.Clear(_values1, 0, _count);
        Array.Clear(_values2, 0, _count);
        Array.Clear(_values3, 0, _count);
        _count = 0;
    }

    public void TrimExcess()
    {
        int capacity = _values1.Length;
        if (_count < capacity * 0.9)
        {
            int newCapacity = Math.Max(DefaultCapacity, _count);
            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
        }
    }
}

public class MultiArray<TValue1, TValue2, TValue3, TValue4>
{
    private const int DefaultCapacity = 32;
    private int _count;
    
    private TValue1[] _values1;
    private TValue2[] _values2;
    private TValue3[] _values3;
    private TValue4[] _values4;

    public MultiArray(int initialCapacity)
    {
        if (initialCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        
        _values1 = new TValue1[initialCapacity];
        _values2 = new TValue2[initialCapacity];
        _values3 = new TValue3[initialCapacity];
        _values4 = new TValue4[initialCapacity];
    }

    public MultiArray()
    {
        _values1 = new TValue1[DefaultCapacity];
        _values2 = new TValue2[DefaultCapacity];
        _values3 = new TValue3[DefaultCapacity];
        _values4 = new TValue4[DefaultCapacity];
        
    }

    public int Count => _count;

    private void EnsureCapacity(int minCapacity)
    {
        int capacity = _values1.Length;
        if (minCapacity > capacity)
        {
            int newCapacity = Math.Max(capacity * 2, minCapacity);

            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
        }
    }

    public int Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;
        _values4[_count] = value4;

        return _count++;
    }

    public void Set(int index, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));
            
        
        _values1[index] = value1;
        
        _values2[index] = value2;
        
        _values3[index] = value3;
        
        _values4[index] = value4;
        
    }

    
    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    

    public bool SwapRemove(int index)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            return false;
        }

        _count--;
        return true;
    }
    
    public bool TryGet(int index, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4)
    {
        if (index < _count)
        {
            
            value1 = _values1[index];
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];

            return true;
        }

        
        value1 = default;
        value2 = default;
        value3 = default;
        value4 = default;
        return false;
    }
    
    public bool TryGetButFirst(int index, out TValue2 value2, out TValue3 value3, out TValue4 value4)
    {
        if (index < _count)
        {
            
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];

            return true;
        }

        
        value2 = default;
        value3 = default;
        value4 = default;
        return false;
    }

    public bool SwapRemoveReturnFirst(int index, out TValue1 value1)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            value1 = _values1[index];
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            return true;
        }

        value1 = default!;
        _count--;
        return false;
    }

    public void Clear()
    {
        
        Array.Clear(_values1, 0, _count);
        Array.Clear(_values2, 0, _count);
        Array.Clear(_values3, 0, _count);
        Array.Clear(_values4, 0, _count);
        _count = 0;
    }

    public void TrimExcess()
    {
        int capacity = _values1.Length;
        if (_count < capacity * 0.9)
        {
            int newCapacity = Math.Max(DefaultCapacity, _count);
            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
        }
    }
}

public class MultiArray<TValue1, TValue2, TValue3, TValue4, TValue5>
{
    private const int DefaultCapacity = 32;
    private int _count;
    
    private TValue1[] _values1;
    private TValue2[] _values2;
    private TValue3[] _values3;
    private TValue4[] _values4;
    private TValue5[] _values5;

    public MultiArray(int initialCapacity)
    {
        if (initialCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        
        _values1 = new TValue1[initialCapacity];
        _values2 = new TValue2[initialCapacity];
        _values3 = new TValue3[initialCapacity];
        _values4 = new TValue4[initialCapacity];
        _values5 = new TValue5[initialCapacity];
    }

    public MultiArray()
    {
        _values1 = new TValue1[DefaultCapacity];
        _values2 = new TValue2[DefaultCapacity];
        _values3 = new TValue3[DefaultCapacity];
        _values4 = new TValue4[DefaultCapacity];
        _values5 = new TValue5[DefaultCapacity];
        
    }

    public int Count => _count;

    private void EnsureCapacity(int minCapacity)
    {
        int capacity = _values1.Length;
        if (minCapacity > capacity)
        {
            int newCapacity = Math.Max(capacity * 2, minCapacity);

            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
            Array.Resize(ref _values5, newCapacity);
        }
    }

    public int Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;
        _values4[_count] = value4;
        _values5[_count] = value5;

        return _count++;
    }

    public void Set(int index, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));
            
        
        _values1[index] = value1;
        
        _values2[index] = value2;
        
        _values3[index] = value3;
        
        _values4[index] = value4;
        
        _values5[index] = value5;
        
    }

    
    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    
    public Span<TValue5> Values5 => new Span<TValue5>(_values5, 0, _count);
    

    public bool SwapRemove(int index)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            _values5[index] = _values5[swappedIndex];
            return false;
        }

        _count--;
        return true;
    }
    
    public bool TryGet(int index, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5)
    {
        if (index < _count)
        {
            
            value1 = _values1[index];
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];
            value5 = _values5[index];

            return true;
        }

        
        value1 = default;
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;
        return false;
    }
    
    public bool TryGetButFirst(int index, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5)
    {
        if (index < _count)
        {
            
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];
            value5 = _values5[index];

            return true;
        }

        
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;
        return false;
    }

    public bool SwapRemoveReturnFirst(int index, out TValue1 value1)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            value1 = _values1[index];
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            _values5[index] = _values5[swappedIndex];
            return true;
        }

        value1 = default!;
        _count--;
        return false;
    }

    public void Clear()
    {
        
        Array.Clear(_values1, 0, _count);
        Array.Clear(_values2, 0, _count);
        Array.Clear(_values3, 0, _count);
        Array.Clear(_values4, 0, _count);
        Array.Clear(_values5, 0, _count);
        _count = 0;
    }

    public void TrimExcess()
    {
        int capacity = _values1.Length;
        if (_count < capacity * 0.9)
        {
            int newCapacity = Math.Max(DefaultCapacity, _count);
            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
            Array.Resize(ref _values5, newCapacity);
        }
    }
}

public class MultiArray<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6>
{
    private const int DefaultCapacity = 32;
    private int _count;
    
    private TValue1[] _values1;
    private TValue2[] _values2;
    private TValue3[] _values3;
    private TValue4[] _values4;
    private TValue5[] _values5;
    private TValue6[] _values6;

    public MultiArray(int initialCapacity)
    {
        if (initialCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        
        _values1 = new TValue1[initialCapacity];
        _values2 = new TValue2[initialCapacity];
        _values3 = new TValue3[initialCapacity];
        _values4 = new TValue4[initialCapacity];
        _values5 = new TValue5[initialCapacity];
        _values6 = new TValue6[initialCapacity];
    }

    public MultiArray()
    {
        _values1 = new TValue1[DefaultCapacity];
        _values2 = new TValue2[DefaultCapacity];
        _values3 = new TValue3[DefaultCapacity];
        _values4 = new TValue4[DefaultCapacity];
        _values5 = new TValue5[DefaultCapacity];
        _values6 = new TValue6[DefaultCapacity];
        
    }

    public int Count => _count;

    private void EnsureCapacity(int minCapacity)
    {
        int capacity = _values1.Length;
        if (minCapacity > capacity)
        {
            int newCapacity = Math.Max(capacity * 2, minCapacity);

            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
            Array.Resize(ref _values5, newCapacity);
            Array.Resize(ref _values6, newCapacity);
        }
    }

    public int Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;
        _values4[_count] = value4;
        _values5[_count] = value5;
        _values6[_count] = value6;

        return _count++;
    }

    public void Set(int index, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));
            
        
        _values1[index] = value1;
        
        _values2[index] = value2;
        
        _values3[index] = value3;
        
        _values4[index] = value4;
        
        _values5[index] = value5;
        
        _values6[index] = value6;
        
    }

    
    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    
    public Span<TValue5> Values5 => new Span<TValue5>(_values5, 0, _count);
    
    public Span<TValue6> Values6 => new Span<TValue6>(_values6, 0, _count);
    

    public bool SwapRemove(int index)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            _values5[index] = _values5[swappedIndex];
            _values6[index] = _values6[swappedIndex];
            return false;
        }

        _count--;
        return true;
    }
    
    public bool TryGet(int index, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6)
    {
        if (index < _count)
        {
            
            value1 = _values1[index];
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];
            value5 = _values5[index];
            value6 = _values6[index];

            return true;
        }

        
        value1 = default;
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;
        value6 = default;
        return false;
    }
    
    public bool TryGetButFirst(int index, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6)
    {
        if (index < _count)
        {
            
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];
            value5 = _values5[index];
            value6 = _values6[index];

            return true;
        }

        
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;
        value6 = default;
        return false;
    }

    public bool SwapRemoveReturnFirst(int index, out TValue1 value1)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            value1 = _values1[index];
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            _values5[index] = _values5[swappedIndex];
            _values6[index] = _values6[swappedIndex];
            return true;
        }

        value1 = default!;
        _count--;
        return false;
    }

    public void Clear()
    {
        
        Array.Clear(_values1, 0, _count);
        Array.Clear(_values2, 0, _count);
        Array.Clear(_values3, 0, _count);
        Array.Clear(_values4, 0, _count);
        Array.Clear(_values5, 0, _count);
        Array.Clear(_values6, 0, _count);
        _count = 0;
    }

    public void TrimExcess()
    {
        int capacity = _values1.Length;
        if (_count < capacity * 0.9)
        {
            int newCapacity = Math.Max(DefaultCapacity, _count);
            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
            Array.Resize(ref _values5, newCapacity);
            Array.Resize(ref _values6, newCapacity);
        }
    }
}

public class MultiArray<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7>
{
    private const int DefaultCapacity = 32;
    private int _count;
    
    private TValue1[] _values1;
    private TValue2[] _values2;
    private TValue3[] _values3;
    private TValue4[] _values4;
    private TValue5[] _values5;
    private TValue6[] _values6;
    private TValue7[] _values7;

    public MultiArray(int initialCapacity)
    {
        if (initialCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        
        _values1 = new TValue1[initialCapacity];
        _values2 = new TValue2[initialCapacity];
        _values3 = new TValue3[initialCapacity];
        _values4 = new TValue4[initialCapacity];
        _values5 = new TValue5[initialCapacity];
        _values6 = new TValue6[initialCapacity];
        _values7 = new TValue7[initialCapacity];
    }

    public MultiArray()
    {
        _values1 = new TValue1[DefaultCapacity];
        _values2 = new TValue2[DefaultCapacity];
        _values3 = new TValue3[DefaultCapacity];
        _values4 = new TValue4[DefaultCapacity];
        _values5 = new TValue5[DefaultCapacity];
        _values6 = new TValue6[DefaultCapacity];
        _values7 = new TValue7[DefaultCapacity];
        
    }

    public int Count => _count;

    private void EnsureCapacity(int minCapacity)
    {
        int capacity = _values1.Length;
        if (minCapacity > capacity)
        {
            int newCapacity = Math.Max(capacity * 2, minCapacity);

            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
            Array.Resize(ref _values5, newCapacity);
            Array.Resize(ref _values6, newCapacity);
            Array.Resize(ref _values7, newCapacity);
        }
    }

    public int Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;
        _values4[_count] = value4;
        _values5[_count] = value5;
        _values6[_count] = value6;
        _values7[_count] = value7;

        return _count++;
    }

    public void Set(int index, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));
            
        
        _values1[index] = value1;
        
        _values2[index] = value2;
        
        _values3[index] = value3;
        
        _values4[index] = value4;
        
        _values5[index] = value5;
        
        _values6[index] = value6;
        
        _values7[index] = value7;
        
    }

    
    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    
    public Span<TValue5> Values5 => new Span<TValue5>(_values5, 0, _count);
    
    public Span<TValue6> Values6 => new Span<TValue6>(_values6, 0, _count);
    
    public Span<TValue7> Values7 => new Span<TValue7>(_values7, 0, _count);
    

    public bool SwapRemove(int index)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            _values5[index] = _values5[swappedIndex];
            _values6[index] = _values6[swappedIndex];
            _values7[index] = _values7[swappedIndex];
            return false;
        }

        _count--;
        return true;
    }
    
    public bool TryGet(int index, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6, out TValue7 value7)
    {
        if (index < _count)
        {
            
            value1 = _values1[index];
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];
            value5 = _values5[index];
            value6 = _values6[index];
            value7 = _values7[index];

            return true;
        }

        
        value1 = default;
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;
        value6 = default;
        value7 = default;
        return false;
    }
    
    public bool TryGetButFirst(int index, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6, out TValue7 value7)
    {
        if (index < _count)
        {
            
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];
            value5 = _values5[index];
            value6 = _values6[index];
            value7 = _values7[index];

            return true;
        }

        
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;
        value6 = default;
        value7 = default;
        return false;
    }

    public bool SwapRemoveReturnFirst(int index, out TValue1 value1)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            value1 = _values1[index];
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            _values5[index] = _values5[swappedIndex];
            _values6[index] = _values6[swappedIndex];
            _values7[index] = _values7[swappedIndex];
            return true;
        }

        value1 = default!;
        _count--;
        return false;
    }

    public void Clear()
    {
        
        Array.Clear(_values1, 0, _count);
        Array.Clear(_values2, 0, _count);
        Array.Clear(_values3, 0, _count);
        Array.Clear(_values4, 0, _count);
        Array.Clear(_values5, 0, _count);
        Array.Clear(_values6, 0, _count);
        Array.Clear(_values7, 0, _count);
        _count = 0;
    }

    public void TrimExcess()
    {
        int capacity = _values1.Length;
        if (_count < capacity * 0.9)
        {
            int newCapacity = Math.Max(DefaultCapacity, _count);
            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
            Array.Resize(ref _values5, newCapacity);
            Array.Resize(ref _values6, newCapacity);
            Array.Resize(ref _values7, newCapacity);
        }
    }
}

public class MultiArray<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>
{
    private const int DefaultCapacity = 32;
    private int _count;
    
    private TValue1[] _values1;
    private TValue2[] _values2;
    private TValue3[] _values3;
    private TValue4[] _values4;
    private TValue5[] _values5;
    private TValue6[] _values6;
    private TValue7[] _values7;
    private TValue8[] _values8;

    public MultiArray(int initialCapacity)
    {
        if (initialCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        
        _values1 = new TValue1[initialCapacity];
        _values2 = new TValue2[initialCapacity];
        _values3 = new TValue3[initialCapacity];
        _values4 = new TValue4[initialCapacity];
        _values5 = new TValue5[initialCapacity];
        _values6 = new TValue6[initialCapacity];
        _values7 = new TValue7[initialCapacity];
        _values8 = new TValue8[initialCapacity];
    }

    public MultiArray()
    {
        _values1 = new TValue1[DefaultCapacity];
        _values2 = new TValue2[DefaultCapacity];
        _values3 = new TValue3[DefaultCapacity];
        _values4 = new TValue4[DefaultCapacity];
        _values5 = new TValue5[DefaultCapacity];
        _values6 = new TValue6[DefaultCapacity];
        _values7 = new TValue7[DefaultCapacity];
        _values8 = new TValue8[DefaultCapacity];
        
    }

    public int Count => _count;

    private void EnsureCapacity(int minCapacity)
    {
        int capacity = _values1.Length;
        if (minCapacity > capacity)
        {
            int newCapacity = Math.Max(capacity * 2, minCapacity);

            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
            Array.Resize(ref _values5, newCapacity);
            Array.Resize(ref _values6, newCapacity);
            Array.Resize(ref _values7, newCapacity);
            Array.Resize(ref _values8, newCapacity);
        }
    }

    public int Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;
        _values4[_count] = value4;
        _values5[_count] = value5;
        _values6[_count] = value6;
        _values7[_count] = value7;
        _values8[_count] = value8;

        return _count++;
    }

    public void Set(int index, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));
            
        
        _values1[index] = value1;
        
        _values2[index] = value2;
        
        _values3[index] = value3;
        
        _values4[index] = value4;
        
        _values5[index] = value5;
        
        _values6[index] = value6;
        
        _values7[index] = value7;
        
        _values8[index] = value8;
        
    }

    
    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    
    public Span<TValue5> Values5 => new Span<TValue5>(_values5, 0, _count);
    
    public Span<TValue6> Values6 => new Span<TValue6>(_values6, 0, _count);
    
    public Span<TValue7> Values7 => new Span<TValue7>(_values7, 0, _count);
    
    public Span<TValue8> Values8 => new Span<TValue8>(_values8, 0, _count);
    

    public bool SwapRemove(int index)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            _values5[index] = _values5[swappedIndex];
            _values6[index] = _values6[swappedIndex];
            _values7[index] = _values7[swappedIndex];
            _values8[index] = _values8[swappedIndex];
            return false;
        }

        _count--;
        return true;
    }
    
    public bool TryGet(int index, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6, out TValue7 value7, out TValue8 value8)
    {
        if (index < _count)
        {
            
            value1 = _values1[index];
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];
            value5 = _values5[index];
            value6 = _values6[index];
            value7 = _values7[index];
            value8 = _values8[index];

            return true;
        }

        
        value1 = default;
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;
        value6 = default;
        value7 = default;
        value8 = default;
        return false;
    }
    
    public bool TryGetButFirst(int index, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6, out TValue7 value7, out TValue8 value8)
    {
        if (index < _count)
        {
            
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];
            value5 = _values5[index];
            value6 = _values6[index];
            value7 = _values7[index];
            value8 = _values8[index];

            return true;
        }

        
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;
        value6 = default;
        value7 = default;
        value8 = default;
        return false;
    }

    public bool SwapRemoveReturnFirst(int index, out TValue1 value1)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            value1 = _values1[index];
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            _values5[index] = _values5[swappedIndex];
            _values6[index] = _values6[swappedIndex];
            _values7[index] = _values7[swappedIndex];
            _values8[index] = _values8[swappedIndex];
            return true;
        }

        value1 = default!;
        _count--;
        return false;
    }

    public void Clear()
    {
        
        Array.Clear(_values1, 0, _count);
        Array.Clear(_values2, 0, _count);
        Array.Clear(_values3, 0, _count);
        Array.Clear(_values4, 0, _count);
        Array.Clear(_values5, 0, _count);
        Array.Clear(_values6, 0, _count);
        Array.Clear(_values7, 0, _count);
        Array.Clear(_values8, 0, _count);
        _count = 0;
    }

    public void TrimExcess()
    {
        int capacity = _values1.Length;
        if (_count < capacity * 0.9)
        {
            int newCapacity = Math.Max(DefaultCapacity, _count);
            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
            Array.Resize(ref _values5, newCapacity);
            Array.Resize(ref _values6, newCapacity);
            Array.Resize(ref _values7, newCapacity);
            Array.Resize(ref _values8, newCapacity);
        }
    }
}

public class MultiArray<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8, TValue9>
{
    private const int DefaultCapacity = 32;
    private int _count;
    
    private TValue1[] _values1;
    private TValue2[] _values2;
    private TValue3[] _values3;
    private TValue4[] _values4;
    private TValue5[] _values5;
    private TValue6[] _values6;
    private TValue7[] _values7;
    private TValue8[] _values8;
    private TValue9[] _values9;

    public MultiArray(int initialCapacity)
    {
        if (initialCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        
        _values1 = new TValue1[initialCapacity];
        _values2 = new TValue2[initialCapacity];
        _values3 = new TValue3[initialCapacity];
        _values4 = new TValue4[initialCapacity];
        _values5 = new TValue5[initialCapacity];
        _values6 = new TValue6[initialCapacity];
        _values7 = new TValue7[initialCapacity];
        _values8 = new TValue8[initialCapacity];
        _values9 = new TValue9[initialCapacity];
    }

    public MultiArray()
    {
        _values1 = new TValue1[DefaultCapacity];
        _values2 = new TValue2[DefaultCapacity];
        _values3 = new TValue3[DefaultCapacity];
        _values4 = new TValue4[DefaultCapacity];
        _values5 = new TValue5[DefaultCapacity];
        _values6 = new TValue6[DefaultCapacity];
        _values7 = new TValue7[DefaultCapacity];
        _values8 = new TValue8[DefaultCapacity];
        _values9 = new TValue9[DefaultCapacity];
        
    }

    public int Count => _count;

    private void EnsureCapacity(int minCapacity)
    {
        int capacity = _values1.Length;
        if (minCapacity > capacity)
        {
            int newCapacity = Math.Max(capacity * 2, minCapacity);

            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
            Array.Resize(ref _values5, newCapacity);
            Array.Resize(ref _values6, newCapacity);
            Array.Resize(ref _values7, newCapacity);
            Array.Resize(ref _values8, newCapacity);
            Array.Resize(ref _values9, newCapacity);
        }
    }

    public int Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8, TValue9 value9)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;
        _values4[_count] = value4;
        _values5[_count] = value5;
        _values6[_count] = value6;
        _values7[_count] = value7;
        _values8[_count] = value8;
        _values9[_count] = value9;

        return _count++;
    }

    public void Set(int index, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8, TValue9 value9)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));
            
        
        _values1[index] = value1;
        
        _values2[index] = value2;
        
        _values3[index] = value3;
        
        _values4[index] = value4;
        
        _values5[index] = value5;
        
        _values6[index] = value6;
        
        _values7[index] = value7;
        
        _values8[index] = value8;
        
        _values9[index] = value9;
        
    }

    
    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    
    public Span<TValue5> Values5 => new Span<TValue5>(_values5, 0, _count);
    
    public Span<TValue6> Values6 => new Span<TValue6>(_values6, 0, _count);
    
    public Span<TValue7> Values7 => new Span<TValue7>(_values7, 0, _count);
    
    public Span<TValue8> Values8 => new Span<TValue8>(_values8, 0, _count);
    
    public Span<TValue9> Values9 => new Span<TValue9>(_values9, 0, _count);
    

    public bool SwapRemove(int index)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            _values5[index] = _values5[swappedIndex];
            _values6[index] = _values6[swappedIndex];
            _values7[index] = _values7[swappedIndex];
            _values8[index] = _values8[swappedIndex];
            _values9[index] = _values9[swappedIndex];
            return false;
        }

        _count--;
        return true;
    }
    
    public bool TryGet(int index, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6, out TValue7 value7, out TValue8 value8, out TValue9 value9)
    {
        if (index < _count)
        {
            
            value1 = _values1[index];
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];
            value5 = _values5[index];
            value6 = _values6[index];
            value7 = _values7[index];
            value8 = _values8[index];
            value9 = _values9[index];

            return true;
        }

        
        value1 = default;
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;
        value6 = default;
        value7 = default;
        value8 = default;
        value9 = default;
        return false;
    }
    
    public bool TryGetButFirst(int index, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6, out TValue7 value7, out TValue8 value8, out TValue9 value9)
    {
        if (index < _count)
        {
            
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];
            value5 = _values5[index];
            value6 = _values6[index];
            value7 = _values7[index];
            value8 = _values8[index];
            value9 = _values9[index];

            return true;
        }

        
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;
        value6 = default;
        value7 = default;
        value8 = default;
        value9 = default;
        return false;
    }

    public bool SwapRemoveReturnFirst(int index, out TValue1 value1)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            value1 = _values1[index];
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            _values5[index] = _values5[swappedIndex];
            _values6[index] = _values6[swappedIndex];
            _values7[index] = _values7[swappedIndex];
            _values8[index] = _values8[swappedIndex];
            _values9[index] = _values9[swappedIndex];
            return true;
        }

        value1 = default!;
        _count--;
        return false;
    }

    public void Clear()
    {
        
        Array.Clear(_values1, 0, _count);
        Array.Clear(_values2, 0, _count);
        Array.Clear(_values3, 0, _count);
        Array.Clear(_values4, 0, _count);
        Array.Clear(_values5, 0, _count);
        Array.Clear(_values6, 0, _count);
        Array.Clear(_values7, 0, _count);
        Array.Clear(_values8, 0, _count);
        Array.Clear(_values9, 0, _count);
        _count = 0;
    }

    public void TrimExcess()
    {
        int capacity = _values1.Length;
        if (_count < capacity * 0.9)
        {
            int newCapacity = Math.Max(DefaultCapacity, _count);
            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
            Array.Resize(ref _values5, newCapacity);
            Array.Resize(ref _values6, newCapacity);
            Array.Resize(ref _values7, newCapacity);
            Array.Resize(ref _values8, newCapacity);
            Array.Resize(ref _values9, newCapacity);
        }
    }
}

public class MultiArray<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8, TValue9, TValue10>
{
    private const int DefaultCapacity = 32;
    private int _count;
    
    private TValue1[] _values1;
    private TValue2[] _values2;
    private TValue3[] _values3;
    private TValue4[] _values4;
    private TValue5[] _values5;
    private TValue6[] _values6;
    private TValue7[] _values7;
    private TValue8[] _values8;
    private TValue9[] _values9;
    private TValue10[] _values10;

    public MultiArray(int initialCapacity)
    {
        if (initialCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        
        _values1 = new TValue1[initialCapacity];
        _values2 = new TValue2[initialCapacity];
        _values3 = new TValue3[initialCapacity];
        _values4 = new TValue4[initialCapacity];
        _values5 = new TValue5[initialCapacity];
        _values6 = new TValue6[initialCapacity];
        _values7 = new TValue7[initialCapacity];
        _values8 = new TValue8[initialCapacity];
        _values9 = new TValue9[initialCapacity];
        _values10 = new TValue10[initialCapacity];
    }

    public MultiArray()
    {
        _values1 = new TValue1[DefaultCapacity];
        _values2 = new TValue2[DefaultCapacity];
        _values3 = new TValue3[DefaultCapacity];
        _values4 = new TValue4[DefaultCapacity];
        _values5 = new TValue5[DefaultCapacity];
        _values6 = new TValue6[DefaultCapacity];
        _values7 = new TValue7[DefaultCapacity];
        _values8 = new TValue8[DefaultCapacity];
        _values9 = new TValue9[DefaultCapacity];
        _values10 = new TValue10[DefaultCapacity];
        
    }

    public int Count => _count;

    private void EnsureCapacity(int minCapacity)
    {
        int capacity = _values1.Length;
        if (minCapacity > capacity)
        {
            int newCapacity = Math.Max(capacity * 2, minCapacity);

            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
            Array.Resize(ref _values5, newCapacity);
            Array.Resize(ref _values6, newCapacity);
            Array.Resize(ref _values7, newCapacity);
            Array.Resize(ref _values8, newCapacity);
            Array.Resize(ref _values9, newCapacity);
            Array.Resize(ref _values10, newCapacity);
        }
    }

    public int Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8, TValue9 value9, TValue10 value10)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;
        _values4[_count] = value4;
        _values5[_count] = value5;
        _values6[_count] = value6;
        _values7[_count] = value7;
        _values8[_count] = value8;
        _values9[_count] = value9;
        _values10[_count] = value10;

        return _count++;
    }

    public void Set(int index, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8, TValue9 value9, TValue10 value10)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));
            
        
        _values1[index] = value1;
        
        _values2[index] = value2;
        
        _values3[index] = value3;
        
        _values4[index] = value4;
        
        _values5[index] = value5;
        
        _values6[index] = value6;
        
        _values7[index] = value7;
        
        _values8[index] = value8;
        
        _values9[index] = value9;
        
        _values10[index] = value10;
        
    }

    
    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    
    public Span<TValue5> Values5 => new Span<TValue5>(_values5, 0, _count);
    
    public Span<TValue6> Values6 => new Span<TValue6>(_values6, 0, _count);
    
    public Span<TValue7> Values7 => new Span<TValue7>(_values7, 0, _count);
    
    public Span<TValue8> Values8 => new Span<TValue8>(_values8, 0, _count);
    
    public Span<TValue9> Values9 => new Span<TValue9>(_values9, 0, _count);
    
    public Span<TValue10> Values10 => new Span<TValue10>(_values10, 0, _count);
    

    public bool SwapRemove(int index)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            _values5[index] = _values5[swappedIndex];
            _values6[index] = _values6[swappedIndex];
            _values7[index] = _values7[swappedIndex];
            _values8[index] = _values8[swappedIndex];
            _values9[index] = _values9[swappedIndex];
            _values10[index] = _values10[swappedIndex];
            return false;
        }

        _count--;
        return true;
    }
    
    public bool TryGet(int index, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6, out TValue7 value7, out TValue8 value8, out TValue9 value9, out TValue10 value10)
    {
        if (index < _count)
        {
            
            value1 = _values1[index];
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];
            value5 = _values5[index];
            value6 = _values6[index];
            value7 = _values7[index];
            value8 = _values8[index];
            value9 = _values9[index];
            value10 = _values10[index];

            return true;
        }

        
        value1 = default;
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;
        value6 = default;
        value7 = default;
        value8 = default;
        value9 = default;
        value10 = default;
        return false;
    }
    
    public bool TryGetButFirst(int index, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6, out TValue7 value7, out TValue8 value8, out TValue9 value9, out TValue10 value10)
    {
        if (index < _count)
        {
            
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];
            value5 = _values5[index];
            value6 = _values6[index];
            value7 = _values7[index];
            value8 = _values8[index];
            value9 = _values9[index];
            value10 = _values10[index];

            return true;
        }

        
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;
        value6 = default;
        value7 = default;
        value8 = default;
        value9 = default;
        value10 = default;
        return false;
    }

    public bool SwapRemoveReturnFirst(int index, out TValue1 value1)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            value1 = _values1[index];
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            _values5[index] = _values5[swappedIndex];
            _values6[index] = _values6[swappedIndex];
            _values7[index] = _values7[swappedIndex];
            _values8[index] = _values8[swappedIndex];
            _values9[index] = _values9[swappedIndex];
            _values10[index] = _values10[swappedIndex];
            return true;
        }

        value1 = default!;
        _count--;
        return false;
    }

    public void Clear()
    {
        
        Array.Clear(_values1, 0, _count);
        Array.Clear(_values2, 0, _count);
        Array.Clear(_values3, 0, _count);
        Array.Clear(_values4, 0, _count);
        Array.Clear(_values5, 0, _count);
        Array.Clear(_values6, 0, _count);
        Array.Clear(_values7, 0, _count);
        Array.Clear(_values8, 0, _count);
        Array.Clear(_values9, 0, _count);
        Array.Clear(_values10, 0, _count);
        _count = 0;
    }

    public void TrimExcess()
    {
        int capacity = _values1.Length;
        if (_count < capacity * 0.9)
        {
            int newCapacity = Math.Max(DefaultCapacity, _count);
            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
            Array.Resize(ref _values5, newCapacity);
            Array.Resize(ref _values6, newCapacity);
            Array.Resize(ref _values7, newCapacity);
            Array.Resize(ref _values8, newCapacity);
            Array.Resize(ref _values9, newCapacity);
            Array.Resize(ref _values10, newCapacity);
        }
    }
}


public struct MultiArrayStruct<TValue1, TValue2>
{
    private const int DefaultCapacity = 32;
    private int _count;
    
    private TValue1[] _values1;
    private TValue2[] _values2;

    public MultiArrayStruct(int initialCapacity)
    {
        if (initialCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        
        _values1 = new TValue1[initialCapacity];
        _values2 = new TValue2[initialCapacity];
    }

    public MultiArrayStruct()
    {
        _values1 = new TValue1[DefaultCapacity];
        _values2 = new TValue2[DefaultCapacity];
        
    }

    public int Count => _count;

    private void EnsureCapacity(int minCapacity)
    {
        int capacity = _values1.Length;
        if (minCapacity > capacity)
        {
            int newCapacity = Math.Max(capacity * 2, minCapacity);

            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
        }
    }

    public int Add(TValue1 value1, TValue2 value2)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;

        return _count++;
    }

    public void Set(int index, TValue1 value1, TValue2 value2)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));
            
        
        _values1[index] = value1;
        
        _values2[index] = value2;
        
    }

    
    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    

    public bool SwapRemove(int index)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            return false;
        }

        _count--;
        return true;
    }
    
    public bool TryGet(int index, out TValue1 value1, out TValue2 value2)
    {
        if (index < _count)
        {
            
            value1 = _values1[index];
            value2 = _values2[index];

            return true;
        }

        
        value1 = default;
        value2 = default;
        return false;
    }
    
    public bool TryGetButFirst(int index, out TValue2 value2)
    {
        if (index < _count)
        {
            
            value2 = _values2[index];

            return true;
        }

        
        value2 = default;
        return false;
    }

    public bool SwapRemoveReturnFirst(int index, out TValue1 value1)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            value1 = _values1[index];
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            return true;
        }

        value1 = default!;
        _count--;
        return false;
    }

    public void Clear()
    {
        
        Array.Clear(_values1, 0, _count);
        Array.Clear(_values2, 0, _count);
        _count = 0;
    }

    public void TrimExcess()
    {
        int capacity = _values1.Length;
        if (_count < capacity * 0.9)
        {
            int newCapacity = Math.Max(DefaultCapacity, _count);
            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
        }
    }
}

public struct MultiArrayStruct<TValue1, TValue2, TValue3>
{
    private const int DefaultCapacity = 32;
    private int _count;
    
    private TValue1[] _values1;
    private TValue2[] _values2;
    private TValue3[] _values3;

    public MultiArrayStruct(int initialCapacity)
    {
        if (initialCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        
        _values1 = new TValue1[initialCapacity];
        _values2 = new TValue2[initialCapacity];
        _values3 = new TValue3[initialCapacity];
    }

    public MultiArrayStruct()
    {
        _values1 = new TValue1[DefaultCapacity];
        _values2 = new TValue2[DefaultCapacity];
        _values3 = new TValue3[DefaultCapacity];
        
    }

    public int Count => _count;

    private void EnsureCapacity(int minCapacity)
    {
        int capacity = _values1.Length;
        if (minCapacity > capacity)
        {
            int newCapacity = Math.Max(capacity * 2, minCapacity);

            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
        }
    }

    public int Add(TValue1 value1, TValue2 value2, TValue3 value3)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;

        return _count++;
    }

    public void Set(int index, TValue1 value1, TValue2 value2, TValue3 value3)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));
            
        
        _values1[index] = value1;
        
        _values2[index] = value2;
        
        _values3[index] = value3;
        
    }

    
    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    

    public bool SwapRemove(int index)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            return false;
        }

        _count--;
        return true;
    }
    
    public bool TryGet(int index, out TValue1 value1, out TValue2 value2, out TValue3 value3)
    {
        if (index < _count)
        {
            
            value1 = _values1[index];
            value2 = _values2[index];
            value3 = _values3[index];

            return true;
        }

        
        value1 = default;
        value2 = default;
        value3 = default;
        return false;
    }
    
    public bool TryGetButFirst(int index, out TValue2 value2, out TValue3 value3)
    {
        if (index < _count)
        {
            
            value2 = _values2[index];
            value3 = _values3[index];

            return true;
        }

        
        value2 = default;
        value3 = default;
        return false;
    }

    public bool SwapRemoveReturnFirst(int index, out TValue1 value1)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            value1 = _values1[index];
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            return true;
        }

        value1 = default!;
        _count--;
        return false;
    }

    public void Clear()
    {
        
        Array.Clear(_values1, 0, _count);
        Array.Clear(_values2, 0, _count);
        Array.Clear(_values3, 0, _count);
        _count = 0;
    }

    public void TrimExcess()
    {
        int capacity = _values1.Length;
        if (_count < capacity * 0.9)
        {
            int newCapacity = Math.Max(DefaultCapacity, _count);
            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
        }
    }
}

public struct MultiArrayStruct<TValue1, TValue2, TValue3, TValue4>
{
    private const int DefaultCapacity = 32;
    private int _count;
    
    private TValue1[] _values1;
    private TValue2[] _values2;
    private TValue3[] _values3;
    private TValue4[] _values4;

    public MultiArrayStruct(int initialCapacity)
    {
        if (initialCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        
        _values1 = new TValue1[initialCapacity];
        _values2 = new TValue2[initialCapacity];
        _values3 = new TValue3[initialCapacity];
        _values4 = new TValue4[initialCapacity];
    }

    public MultiArrayStruct()
    {
        _values1 = new TValue1[DefaultCapacity];
        _values2 = new TValue2[DefaultCapacity];
        _values3 = new TValue3[DefaultCapacity];
        _values4 = new TValue4[DefaultCapacity];
        
    }

    public int Count => _count;

    private void EnsureCapacity(int minCapacity)
    {
        int capacity = _values1.Length;
        if (minCapacity > capacity)
        {
            int newCapacity = Math.Max(capacity * 2, minCapacity);

            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
        }
    }

    public int Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;
        _values4[_count] = value4;

        return _count++;
    }

    public void Set(int index, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));
            
        
        _values1[index] = value1;
        
        _values2[index] = value2;
        
        _values3[index] = value3;
        
        _values4[index] = value4;
        
    }

    
    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    

    public bool SwapRemove(int index)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            return false;
        }

        _count--;
        return true;
    }
    
    public bool TryGet(int index, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4)
    {
        if (index < _count)
        {
            
            value1 = _values1[index];
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];

            return true;
        }

        
        value1 = default;
        value2 = default;
        value3 = default;
        value4 = default;
        return false;
    }
    
    public bool TryGetButFirst(int index, out TValue2 value2, out TValue3 value3, out TValue4 value4)
    {
        if (index < _count)
        {
            
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];

            return true;
        }

        
        value2 = default;
        value3 = default;
        value4 = default;
        return false;
    }

    public bool SwapRemoveReturnFirst(int index, out TValue1 value1)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            value1 = _values1[index];
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            return true;
        }

        value1 = default!;
        _count--;
        return false;
    }

    public void Clear()
    {
        
        Array.Clear(_values1, 0, _count);
        Array.Clear(_values2, 0, _count);
        Array.Clear(_values3, 0, _count);
        Array.Clear(_values4, 0, _count);
        _count = 0;
    }

    public void TrimExcess()
    {
        int capacity = _values1.Length;
        if (_count < capacity * 0.9)
        {
            int newCapacity = Math.Max(DefaultCapacity, _count);
            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
        }
    }
}

public struct MultiArrayStruct<TValue1, TValue2, TValue3, TValue4, TValue5>
{
    private const int DefaultCapacity = 32;
    private int _count;
    
    private TValue1[] _values1;
    private TValue2[] _values2;
    private TValue3[] _values3;
    private TValue4[] _values4;
    private TValue5[] _values5;

    public MultiArrayStruct(int initialCapacity)
    {
        if (initialCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        
        _values1 = new TValue1[initialCapacity];
        _values2 = new TValue2[initialCapacity];
        _values3 = new TValue3[initialCapacity];
        _values4 = new TValue4[initialCapacity];
        _values5 = new TValue5[initialCapacity];
    }

    public MultiArrayStruct()
    {
        _values1 = new TValue1[DefaultCapacity];
        _values2 = new TValue2[DefaultCapacity];
        _values3 = new TValue3[DefaultCapacity];
        _values4 = new TValue4[DefaultCapacity];
        _values5 = new TValue5[DefaultCapacity];
        
    }

    public int Count => _count;

    private void EnsureCapacity(int minCapacity)
    {
        int capacity = _values1.Length;
        if (minCapacity > capacity)
        {
            int newCapacity = Math.Max(capacity * 2, minCapacity);

            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
            Array.Resize(ref _values5, newCapacity);
        }
    }

    public int Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;
        _values4[_count] = value4;
        _values5[_count] = value5;

        return _count++;
    }

    public void Set(int index, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));
            
        
        _values1[index] = value1;
        
        _values2[index] = value2;
        
        _values3[index] = value3;
        
        _values4[index] = value4;
        
        _values5[index] = value5;
        
    }

    
    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    
    public Span<TValue5> Values5 => new Span<TValue5>(_values5, 0, _count);
    

    public bool SwapRemove(int index)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            _values5[index] = _values5[swappedIndex];
            return false;
        }

        _count--;
        return true;
    }
    
    public bool TryGet(int index, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5)
    {
        if (index < _count)
        {
            
            value1 = _values1[index];
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];
            value5 = _values5[index];

            return true;
        }

        
        value1 = default;
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;
        return false;
    }
    
    public bool TryGetButFirst(int index, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5)
    {
        if (index < _count)
        {
            
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];
            value5 = _values5[index];

            return true;
        }

        
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;
        return false;
    }

    public bool SwapRemoveReturnFirst(int index, out TValue1 value1)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            value1 = _values1[index];
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            _values5[index] = _values5[swappedIndex];
            return true;
        }

        value1 = default!;
        _count--;
        return false;
    }

    public void Clear()
    {
        
        Array.Clear(_values1, 0, _count);
        Array.Clear(_values2, 0, _count);
        Array.Clear(_values3, 0, _count);
        Array.Clear(_values4, 0, _count);
        Array.Clear(_values5, 0, _count);
        _count = 0;
    }

    public void TrimExcess()
    {
        int capacity = _values1.Length;
        if (_count < capacity * 0.9)
        {
            int newCapacity = Math.Max(DefaultCapacity, _count);
            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
            Array.Resize(ref _values5, newCapacity);
        }
    }
}

public struct MultiArrayStruct<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6>
{
    private const int DefaultCapacity = 32;
    private int _count;
    
    private TValue1[] _values1;
    private TValue2[] _values2;
    private TValue3[] _values3;
    private TValue4[] _values4;
    private TValue5[] _values5;
    private TValue6[] _values6;

    public MultiArrayStruct(int initialCapacity)
    {
        if (initialCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        
        _values1 = new TValue1[initialCapacity];
        _values2 = new TValue2[initialCapacity];
        _values3 = new TValue3[initialCapacity];
        _values4 = new TValue4[initialCapacity];
        _values5 = new TValue5[initialCapacity];
        _values6 = new TValue6[initialCapacity];
    }

    public MultiArrayStruct()
    {
        _values1 = new TValue1[DefaultCapacity];
        _values2 = new TValue2[DefaultCapacity];
        _values3 = new TValue3[DefaultCapacity];
        _values4 = new TValue4[DefaultCapacity];
        _values5 = new TValue5[DefaultCapacity];
        _values6 = new TValue6[DefaultCapacity];
        
    }

    public int Count => _count;

    private void EnsureCapacity(int minCapacity)
    {
        int capacity = _values1.Length;
        if (minCapacity > capacity)
        {
            int newCapacity = Math.Max(capacity * 2, minCapacity);

            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
            Array.Resize(ref _values5, newCapacity);
            Array.Resize(ref _values6, newCapacity);
        }
    }

    public int Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;
        _values4[_count] = value4;
        _values5[_count] = value5;
        _values6[_count] = value6;

        return _count++;
    }

    public void Set(int index, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));
            
        
        _values1[index] = value1;
        
        _values2[index] = value2;
        
        _values3[index] = value3;
        
        _values4[index] = value4;
        
        _values5[index] = value5;
        
        _values6[index] = value6;
        
    }

    
    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    
    public Span<TValue5> Values5 => new Span<TValue5>(_values5, 0, _count);
    
    public Span<TValue6> Values6 => new Span<TValue6>(_values6, 0, _count);
    

    public bool SwapRemove(int index)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            _values5[index] = _values5[swappedIndex];
            _values6[index] = _values6[swappedIndex];
            return false;
        }

        _count--;
        return true;
    }
    
    public bool TryGet(int index, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6)
    {
        if (index < _count)
        {
            
            value1 = _values1[index];
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];
            value5 = _values5[index];
            value6 = _values6[index];

            return true;
        }

        
        value1 = default;
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;
        value6 = default;
        return false;
    }
    
    public bool TryGetButFirst(int index, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6)
    {
        if (index < _count)
        {
            
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];
            value5 = _values5[index];
            value6 = _values6[index];

            return true;
        }

        
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;
        value6 = default;
        return false;
    }

    public bool SwapRemoveReturnFirst(int index, out TValue1 value1)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            value1 = _values1[index];
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            _values5[index] = _values5[swappedIndex];
            _values6[index] = _values6[swappedIndex];
            return true;
        }

        value1 = default!;
        _count--;
        return false;
    }

    public void Clear()
    {
        
        Array.Clear(_values1, 0, _count);
        Array.Clear(_values2, 0, _count);
        Array.Clear(_values3, 0, _count);
        Array.Clear(_values4, 0, _count);
        Array.Clear(_values5, 0, _count);
        Array.Clear(_values6, 0, _count);
        _count = 0;
    }

    public void TrimExcess()
    {
        int capacity = _values1.Length;
        if (_count < capacity * 0.9)
        {
            int newCapacity = Math.Max(DefaultCapacity, _count);
            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
            Array.Resize(ref _values5, newCapacity);
            Array.Resize(ref _values6, newCapacity);
        }
    }
}

public struct MultiArrayStruct<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7>
{
    private const int DefaultCapacity = 32;
    private int _count;
    
    private TValue1[] _values1;
    private TValue2[] _values2;
    private TValue3[] _values3;
    private TValue4[] _values4;
    private TValue5[] _values5;
    private TValue6[] _values6;
    private TValue7[] _values7;

    public MultiArrayStruct(int initialCapacity)
    {
        if (initialCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        
        _values1 = new TValue1[initialCapacity];
        _values2 = new TValue2[initialCapacity];
        _values3 = new TValue3[initialCapacity];
        _values4 = new TValue4[initialCapacity];
        _values5 = new TValue5[initialCapacity];
        _values6 = new TValue6[initialCapacity];
        _values7 = new TValue7[initialCapacity];
    }

    public MultiArrayStruct()
    {
        _values1 = new TValue1[DefaultCapacity];
        _values2 = new TValue2[DefaultCapacity];
        _values3 = new TValue3[DefaultCapacity];
        _values4 = new TValue4[DefaultCapacity];
        _values5 = new TValue5[DefaultCapacity];
        _values6 = new TValue6[DefaultCapacity];
        _values7 = new TValue7[DefaultCapacity];
        
    }

    public int Count => _count;

    private void EnsureCapacity(int minCapacity)
    {
        int capacity = _values1.Length;
        if (minCapacity > capacity)
        {
            int newCapacity = Math.Max(capacity * 2, minCapacity);

            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
            Array.Resize(ref _values5, newCapacity);
            Array.Resize(ref _values6, newCapacity);
            Array.Resize(ref _values7, newCapacity);
        }
    }

    public int Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;
        _values4[_count] = value4;
        _values5[_count] = value5;
        _values6[_count] = value6;
        _values7[_count] = value7;

        return _count++;
    }

    public void Set(int index, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));
            
        
        _values1[index] = value1;
        
        _values2[index] = value2;
        
        _values3[index] = value3;
        
        _values4[index] = value4;
        
        _values5[index] = value5;
        
        _values6[index] = value6;
        
        _values7[index] = value7;
        
    }

    
    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    
    public Span<TValue5> Values5 => new Span<TValue5>(_values5, 0, _count);
    
    public Span<TValue6> Values6 => new Span<TValue6>(_values6, 0, _count);
    
    public Span<TValue7> Values7 => new Span<TValue7>(_values7, 0, _count);
    

    public bool SwapRemove(int index)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            _values5[index] = _values5[swappedIndex];
            _values6[index] = _values6[swappedIndex];
            _values7[index] = _values7[swappedIndex];
            return false;
        }

        _count--;
        return true;
    }
    
    public bool TryGet(int index, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6, out TValue7 value7)
    {
        if (index < _count)
        {
            
            value1 = _values1[index];
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];
            value5 = _values5[index];
            value6 = _values6[index];
            value7 = _values7[index];

            return true;
        }

        
        value1 = default;
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;
        value6 = default;
        value7 = default;
        return false;
    }
    
    public bool TryGetButFirst(int index, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6, out TValue7 value7)
    {
        if (index < _count)
        {
            
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];
            value5 = _values5[index];
            value6 = _values6[index];
            value7 = _values7[index];

            return true;
        }

        
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;
        value6 = default;
        value7 = default;
        return false;
    }

    public bool SwapRemoveReturnFirst(int index, out TValue1 value1)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            value1 = _values1[index];
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            _values5[index] = _values5[swappedIndex];
            _values6[index] = _values6[swappedIndex];
            _values7[index] = _values7[swappedIndex];
            return true;
        }

        value1 = default!;
        _count--;
        return false;
    }

    public void Clear()
    {
        
        Array.Clear(_values1, 0, _count);
        Array.Clear(_values2, 0, _count);
        Array.Clear(_values3, 0, _count);
        Array.Clear(_values4, 0, _count);
        Array.Clear(_values5, 0, _count);
        Array.Clear(_values6, 0, _count);
        Array.Clear(_values7, 0, _count);
        _count = 0;
    }

    public void TrimExcess()
    {
        int capacity = _values1.Length;
        if (_count < capacity * 0.9)
        {
            int newCapacity = Math.Max(DefaultCapacity, _count);
            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
            Array.Resize(ref _values5, newCapacity);
            Array.Resize(ref _values6, newCapacity);
            Array.Resize(ref _values7, newCapacity);
        }
    }
}

public struct MultiArrayStruct<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>
{
    private const int DefaultCapacity = 32;
    private int _count;
    
    private TValue1[] _values1;
    private TValue2[] _values2;
    private TValue3[] _values3;
    private TValue4[] _values4;
    private TValue5[] _values5;
    private TValue6[] _values6;
    private TValue7[] _values7;
    private TValue8[] _values8;

    public MultiArrayStruct(int initialCapacity)
    {
        if (initialCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        
        _values1 = new TValue1[initialCapacity];
        _values2 = new TValue2[initialCapacity];
        _values3 = new TValue3[initialCapacity];
        _values4 = new TValue4[initialCapacity];
        _values5 = new TValue5[initialCapacity];
        _values6 = new TValue6[initialCapacity];
        _values7 = new TValue7[initialCapacity];
        _values8 = new TValue8[initialCapacity];
    }

    public MultiArrayStruct()
    {
        _values1 = new TValue1[DefaultCapacity];
        _values2 = new TValue2[DefaultCapacity];
        _values3 = new TValue3[DefaultCapacity];
        _values4 = new TValue4[DefaultCapacity];
        _values5 = new TValue5[DefaultCapacity];
        _values6 = new TValue6[DefaultCapacity];
        _values7 = new TValue7[DefaultCapacity];
        _values8 = new TValue8[DefaultCapacity];
        
    }

    public int Count => _count;

    private void EnsureCapacity(int minCapacity)
    {
        int capacity = _values1.Length;
        if (minCapacity > capacity)
        {
            int newCapacity = Math.Max(capacity * 2, minCapacity);

            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
            Array.Resize(ref _values5, newCapacity);
            Array.Resize(ref _values6, newCapacity);
            Array.Resize(ref _values7, newCapacity);
            Array.Resize(ref _values8, newCapacity);
        }
    }

    public int Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;
        _values4[_count] = value4;
        _values5[_count] = value5;
        _values6[_count] = value6;
        _values7[_count] = value7;
        _values8[_count] = value8;

        return _count++;
    }

    public void Set(int index, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));
            
        
        _values1[index] = value1;
        
        _values2[index] = value2;
        
        _values3[index] = value3;
        
        _values4[index] = value4;
        
        _values5[index] = value5;
        
        _values6[index] = value6;
        
        _values7[index] = value7;
        
        _values8[index] = value8;
        
    }

    
    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    
    public Span<TValue5> Values5 => new Span<TValue5>(_values5, 0, _count);
    
    public Span<TValue6> Values6 => new Span<TValue6>(_values6, 0, _count);
    
    public Span<TValue7> Values7 => new Span<TValue7>(_values7, 0, _count);
    
    public Span<TValue8> Values8 => new Span<TValue8>(_values8, 0, _count);
    

    public bool SwapRemove(int index)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            _values5[index] = _values5[swappedIndex];
            _values6[index] = _values6[swappedIndex];
            _values7[index] = _values7[swappedIndex];
            _values8[index] = _values8[swappedIndex];
            return false;
        }

        _count--;
        return true;
    }
    
    public bool TryGet(int index, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6, out TValue7 value7, out TValue8 value8)
    {
        if (index < _count)
        {
            
            value1 = _values1[index];
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];
            value5 = _values5[index];
            value6 = _values6[index];
            value7 = _values7[index];
            value8 = _values8[index];

            return true;
        }

        
        value1 = default;
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;
        value6 = default;
        value7 = default;
        value8 = default;
        return false;
    }
    
    public bool TryGetButFirst(int index, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6, out TValue7 value7, out TValue8 value8)
    {
        if (index < _count)
        {
            
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];
            value5 = _values5[index];
            value6 = _values6[index];
            value7 = _values7[index];
            value8 = _values8[index];

            return true;
        }

        
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;
        value6 = default;
        value7 = default;
        value8 = default;
        return false;
    }

    public bool SwapRemoveReturnFirst(int index, out TValue1 value1)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            value1 = _values1[index];
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            _values5[index] = _values5[swappedIndex];
            _values6[index] = _values6[swappedIndex];
            _values7[index] = _values7[swappedIndex];
            _values8[index] = _values8[swappedIndex];
            return true;
        }

        value1 = default!;
        _count--;
        return false;
    }

    public void Clear()
    {
        
        Array.Clear(_values1, 0, _count);
        Array.Clear(_values2, 0, _count);
        Array.Clear(_values3, 0, _count);
        Array.Clear(_values4, 0, _count);
        Array.Clear(_values5, 0, _count);
        Array.Clear(_values6, 0, _count);
        Array.Clear(_values7, 0, _count);
        Array.Clear(_values8, 0, _count);
        _count = 0;
    }

    public void TrimExcess()
    {
        int capacity = _values1.Length;
        if (_count < capacity * 0.9)
        {
            int newCapacity = Math.Max(DefaultCapacity, _count);
            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
            Array.Resize(ref _values5, newCapacity);
            Array.Resize(ref _values6, newCapacity);
            Array.Resize(ref _values7, newCapacity);
            Array.Resize(ref _values8, newCapacity);
        }
    }
}

public struct MultiArrayStruct<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8, TValue9>
{
    private const int DefaultCapacity = 32;
    private int _count;
    
    private TValue1[] _values1;
    private TValue2[] _values2;
    private TValue3[] _values3;
    private TValue4[] _values4;
    private TValue5[] _values5;
    private TValue6[] _values6;
    private TValue7[] _values7;
    private TValue8[] _values8;
    private TValue9[] _values9;

    public MultiArrayStruct(int initialCapacity)
    {
        if (initialCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        
        _values1 = new TValue1[initialCapacity];
        _values2 = new TValue2[initialCapacity];
        _values3 = new TValue3[initialCapacity];
        _values4 = new TValue4[initialCapacity];
        _values5 = new TValue5[initialCapacity];
        _values6 = new TValue6[initialCapacity];
        _values7 = new TValue7[initialCapacity];
        _values8 = new TValue8[initialCapacity];
        _values9 = new TValue9[initialCapacity];
    }

    public MultiArrayStruct()
    {
        _values1 = new TValue1[DefaultCapacity];
        _values2 = new TValue2[DefaultCapacity];
        _values3 = new TValue3[DefaultCapacity];
        _values4 = new TValue4[DefaultCapacity];
        _values5 = new TValue5[DefaultCapacity];
        _values6 = new TValue6[DefaultCapacity];
        _values7 = new TValue7[DefaultCapacity];
        _values8 = new TValue8[DefaultCapacity];
        _values9 = new TValue9[DefaultCapacity];
        
    }

    public int Count => _count;

    private void EnsureCapacity(int minCapacity)
    {
        int capacity = _values1.Length;
        if (minCapacity > capacity)
        {
            int newCapacity = Math.Max(capacity * 2, minCapacity);

            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
            Array.Resize(ref _values5, newCapacity);
            Array.Resize(ref _values6, newCapacity);
            Array.Resize(ref _values7, newCapacity);
            Array.Resize(ref _values8, newCapacity);
            Array.Resize(ref _values9, newCapacity);
        }
    }

    public int Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8, TValue9 value9)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;
        _values4[_count] = value4;
        _values5[_count] = value5;
        _values6[_count] = value6;
        _values7[_count] = value7;
        _values8[_count] = value8;
        _values9[_count] = value9;

        return _count++;
    }

    public void Set(int index, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8, TValue9 value9)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));
            
        
        _values1[index] = value1;
        
        _values2[index] = value2;
        
        _values3[index] = value3;
        
        _values4[index] = value4;
        
        _values5[index] = value5;
        
        _values6[index] = value6;
        
        _values7[index] = value7;
        
        _values8[index] = value8;
        
        _values9[index] = value9;
        
    }

    
    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    
    public Span<TValue5> Values5 => new Span<TValue5>(_values5, 0, _count);
    
    public Span<TValue6> Values6 => new Span<TValue6>(_values6, 0, _count);
    
    public Span<TValue7> Values7 => new Span<TValue7>(_values7, 0, _count);
    
    public Span<TValue8> Values8 => new Span<TValue8>(_values8, 0, _count);
    
    public Span<TValue9> Values9 => new Span<TValue9>(_values9, 0, _count);
    

    public bool SwapRemove(int index)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            _values5[index] = _values5[swappedIndex];
            _values6[index] = _values6[swappedIndex];
            _values7[index] = _values7[swappedIndex];
            _values8[index] = _values8[swappedIndex];
            _values9[index] = _values9[swappedIndex];
            return false;
        }

        _count--;
        return true;
    }
    
    public bool TryGet(int index, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6, out TValue7 value7, out TValue8 value8, out TValue9 value9)
    {
        if (index < _count)
        {
            
            value1 = _values1[index];
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];
            value5 = _values5[index];
            value6 = _values6[index];
            value7 = _values7[index];
            value8 = _values8[index];
            value9 = _values9[index];

            return true;
        }

        
        value1 = default;
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;
        value6 = default;
        value7 = default;
        value8 = default;
        value9 = default;
        return false;
    }
    
    public bool TryGetButFirst(int index, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6, out TValue7 value7, out TValue8 value8, out TValue9 value9)
    {
        if (index < _count)
        {
            
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];
            value5 = _values5[index];
            value6 = _values6[index];
            value7 = _values7[index];
            value8 = _values8[index];
            value9 = _values9[index];

            return true;
        }

        
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;
        value6 = default;
        value7 = default;
        value8 = default;
        value9 = default;
        return false;
    }

    public bool SwapRemoveReturnFirst(int index, out TValue1 value1)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            value1 = _values1[index];
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            _values5[index] = _values5[swappedIndex];
            _values6[index] = _values6[swappedIndex];
            _values7[index] = _values7[swappedIndex];
            _values8[index] = _values8[swappedIndex];
            _values9[index] = _values9[swappedIndex];
            return true;
        }

        value1 = default!;
        _count--;
        return false;
    }

    public void Clear()
    {
        
        Array.Clear(_values1, 0, _count);
        Array.Clear(_values2, 0, _count);
        Array.Clear(_values3, 0, _count);
        Array.Clear(_values4, 0, _count);
        Array.Clear(_values5, 0, _count);
        Array.Clear(_values6, 0, _count);
        Array.Clear(_values7, 0, _count);
        Array.Clear(_values8, 0, _count);
        Array.Clear(_values9, 0, _count);
        _count = 0;
    }

    public void TrimExcess()
    {
        int capacity = _values1.Length;
        if (_count < capacity * 0.9)
        {
            int newCapacity = Math.Max(DefaultCapacity, _count);
            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
            Array.Resize(ref _values5, newCapacity);
            Array.Resize(ref _values6, newCapacity);
            Array.Resize(ref _values7, newCapacity);
            Array.Resize(ref _values8, newCapacity);
            Array.Resize(ref _values9, newCapacity);
        }
    }
}

public struct MultiArrayStruct<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8, TValue9, TValue10>
{
    private const int DefaultCapacity = 32;
    private int _count;
    
    private TValue1[] _values1;
    private TValue2[] _values2;
    private TValue3[] _values3;
    private TValue4[] _values4;
    private TValue5[] _values5;
    private TValue6[] _values6;
    private TValue7[] _values7;
    private TValue8[] _values8;
    private TValue9[] _values9;
    private TValue10[] _values10;

    public MultiArrayStruct(int initialCapacity)
    {
        if (initialCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        
        _values1 = new TValue1[initialCapacity];
        _values2 = new TValue2[initialCapacity];
        _values3 = new TValue3[initialCapacity];
        _values4 = new TValue4[initialCapacity];
        _values5 = new TValue5[initialCapacity];
        _values6 = new TValue6[initialCapacity];
        _values7 = new TValue7[initialCapacity];
        _values8 = new TValue8[initialCapacity];
        _values9 = new TValue9[initialCapacity];
        _values10 = new TValue10[initialCapacity];
    }

    public MultiArrayStruct()
    {
        _values1 = new TValue1[DefaultCapacity];
        _values2 = new TValue2[DefaultCapacity];
        _values3 = new TValue3[DefaultCapacity];
        _values4 = new TValue4[DefaultCapacity];
        _values5 = new TValue5[DefaultCapacity];
        _values6 = new TValue6[DefaultCapacity];
        _values7 = new TValue7[DefaultCapacity];
        _values8 = new TValue8[DefaultCapacity];
        _values9 = new TValue9[DefaultCapacity];
        _values10 = new TValue10[DefaultCapacity];
        
    }

    public int Count => _count;

    private void EnsureCapacity(int minCapacity)
    {
        int capacity = _values1.Length;
        if (minCapacity > capacity)
        {
            int newCapacity = Math.Max(capacity * 2, minCapacity);

            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
            Array.Resize(ref _values5, newCapacity);
            Array.Resize(ref _values6, newCapacity);
            Array.Resize(ref _values7, newCapacity);
            Array.Resize(ref _values8, newCapacity);
            Array.Resize(ref _values9, newCapacity);
            Array.Resize(ref _values10, newCapacity);
        }
    }

    public int Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8, TValue9 value9, TValue10 value10)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;
        _values4[_count] = value4;
        _values5[_count] = value5;
        _values6[_count] = value6;
        _values7[_count] = value7;
        _values8[_count] = value8;
        _values9[_count] = value9;
        _values10[_count] = value10;

        return _count++;
    }

    public void Set(int index, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8, TValue9 value9, TValue10 value10)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));
            
        
        _values1[index] = value1;
        
        _values2[index] = value2;
        
        _values3[index] = value3;
        
        _values4[index] = value4;
        
        _values5[index] = value5;
        
        _values6[index] = value6;
        
        _values7[index] = value7;
        
        _values8[index] = value8;
        
        _values9[index] = value9;
        
        _values10[index] = value10;
        
    }

    
    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    
    public Span<TValue5> Values5 => new Span<TValue5>(_values5, 0, _count);
    
    public Span<TValue6> Values6 => new Span<TValue6>(_values6, 0, _count);
    
    public Span<TValue7> Values7 => new Span<TValue7>(_values7, 0, _count);
    
    public Span<TValue8> Values8 => new Span<TValue8>(_values8, 0, _count);
    
    public Span<TValue9> Values9 => new Span<TValue9>(_values9, 0, _count);
    
    public Span<TValue10> Values10 => new Span<TValue10>(_values10, 0, _count);
    

    public bool SwapRemove(int index)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            _values5[index] = _values5[swappedIndex];
            _values6[index] = _values6[swappedIndex];
            _values7[index] = _values7[swappedIndex];
            _values8[index] = _values8[swappedIndex];
            _values9[index] = _values9[swappedIndex];
            _values10[index] = _values10[swappedIndex];
            return false;
        }

        _count--;
        return true;
    }
    
    public bool TryGet(int index, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6, out TValue7 value7, out TValue8 value8, out TValue9 value9, out TValue10 value10)
    {
        if (index < _count)
        {
            
            value1 = _values1[index];
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];
            value5 = _values5[index];
            value6 = _values6[index];
            value7 = _values7[index];
            value8 = _values8[index];
            value9 = _values9[index];
            value10 = _values10[index];

            return true;
        }

        
        value1 = default;
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;
        value6 = default;
        value7 = default;
        value8 = default;
        value9 = default;
        value10 = default;
        return false;
    }
    
    public bool TryGetButFirst(int index, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6, out TValue7 value7, out TValue8 value8, out TValue9 value9, out TValue10 value10)
    {
        if (index < _count)
        {
            
            value2 = _values2[index];
            value3 = _values3[index];
            value4 = _values4[index];
            value5 = _values5[index];
            value6 = _values6[index];
            value7 = _values7[index];
            value8 = _values8[index];
            value9 = _values9[index];
            value10 = _values10[index];

            return true;
        }

        
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;
        value6 = default;
        value7 = default;
        value8 = default;
        value9 = default;
        value10 = default;
        return false;
    }

    public bool SwapRemoveReturnFirst(int index, out TValue1 value1)
    {
        int swappedIndex = _count - 1;
        if (index != swappedIndex)
        {
            value1 = _values1[index];
            
            _values1[index] = _values1[swappedIndex];
            _values2[index] = _values2[swappedIndex];
            _values3[index] = _values3[swappedIndex];
            _values4[index] = _values4[swappedIndex];
            _values5[index] = _values5[swappedIndex];
            _values6[index] = _values6[swappedIndex];
            _values7[index] = _values7[swappedIndex];
            _values8[index] = _values8[swappedIndex];
            _values9[index] = _values9[swappedIndex];
            _values10[index] = _values10[swappedIndex];
            return true;
        }

        value1 = default!;
        _count--;
        return false;
    }

    public void Clear()
    {
        
        Array.Clear(_values1, 0, _count);
        Array.Clear(_values2, 0, _count);
        Array.Clear(_values3, 0, _count);
        Array.Clear(_values4, 0, _count);
        Array.Clear(_values5, 0, _count);
        Array.Clear(_values6, 0, _count);
        Array.Clear(_values7, 0, _count);
        Array.Clear(_values8, 0, _count);
        Array.Clear(_values9, 0, _count);
        Array.Clear(_values10, 0, _count);
        _count = 0;
    }

    public void TrimExcess()
    {
        int capacity = _values1.Length;
        if (_count < capacity * 0.9)
        {
            int newCapacity = Math.Max(DefaultCapacity, _count);
            
            Array.Resize(ref _values1, newCapacity);
            Array.Resize(ref _values2, newCapacity);
            Array.Resize(ref _values3, newCapacity);
            Array.Resize(ref _values4, newCapacity);
            Array.Resize(ref _values5, newCapacity);
            Array.Resize(ref _values6, newCapacity);
            Array.Resize(ref _values7, newCapacity);
            Array.Resize(ref _values8, newCapacity);
            Array.Resize(ref _values9, newCapacity);
            Array.Resize(ref _values10, newCapacity);
        }
    }
}


