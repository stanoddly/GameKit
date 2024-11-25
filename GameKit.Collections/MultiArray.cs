// Generated using jinja2-cli: jinja2 MultiArray.cs.jinja2 > MultiArray.cs
using System;

namespace GameKit.Collections;

public class MultiArray<TValue1>
{
    private const int DefaultCapacity = 32;
    private int _count;
    
    private TValue1[] _values1;

    public MultiArray(int initialCapacity)
    {
        if (initialCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        
        _values1 = new TValue1[initialCapacity];
    }

    public MultiArray()
    {
        _values1 = new TValue1[DefaultCapacity];
        
    }

    public int Count => _count;

    private void EnsureCapacity(int minCapacity)
    {
        int capacity = _values1.Length;
        if (minCapacity > capacity)
        {
            int newCapacity = Math.Max(capacity * 2, minCapacity);

            
            Array.Resize(ref _values1, newCapacity);
        }
    }

    public void Add(TValue1 value1)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;

        _count++;
    }

    
    public TValue1 GetValue1(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values1[index];
    }

    public void SetValue1(int index, TValue1 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values1[index] = value;
    }

    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    

    public bool SwapRemove(int index)
    {
        if (index < 0 || index >= _count)
            return false;

        int lastIndex = _count - 1;
        if (index != lastIndex)
        {
            
            _values1[index] = _values1[lastIndex];
        }
        _count--;
        return true;
    }

    public void Clear()
    {
        
        Array.Clear(_values1, 0, _count);
        _count = 0;
    }

    public void TrimExcess()
    {
        int capacity = _values1.Length;
        if (_count < capacity * 0.9)
        {
            int newCapacity = Math.Max(DefaultCapacity, _count);
            
            Array.Resize(ref _values1, newCapacity);
        }
    }
}

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

    public void Add(TValue1 value1, TValue2 value2)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;

        _count++;
    }

    
    public TValue1 GetValue1(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values1[index];
    }

    public void SetValue1(int index, TValue1 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values1[index] = value;
    }

    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public TValue2 GetValue2(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values2[index];
    }

    public void SetValue2(int index, TValue2 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values2[index] = value;
    }

    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    

    public bool SwapRemove(int index)
    {
        if (index < 0 || index >= _count)
            return false;

        int lastIndex = _count - 1;
        if (index != lastIndex)
        {
            
            _values1[index] = _values1[lastIndex];
            _values2[index] = _values2[lastIndex];
        }
        _count--;
        return true;
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

    public void Add(TValue1 value1, TValue2 value2, TValue3 value3)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;

        _count++;
    }

    
    public TValue1 GetValue1(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values1[index];
    }

    public void SetValue1(int index, TValue1 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values1[index] = value;
    }

    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public TValue2 GetValue2(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values2[index];
    }

    public void SetValue2(int index, TValue2 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values2[index] = value;
    }

    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public TValue3 GetValue3(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values3[index];
    }

    public void SetValue3(int index, TValue3 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values3[index] = value;
    }

    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    

    public bool SwapRemove(int index)
    {
        if (index < 0 || index >= _count)
            return false;

        int lastIndex = _count - 1;
        if (index != lastIndex)
        {
            
            _values1[index] = _values1[lastIndex];
            _values2[index] = _values2[lastIndex];
            _values3[index] = _values3[lastIndex];
        }
        _count--;
        return true;
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

    public void Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;
        _values4[_count] = value4;

        _count++;
    }

    
    public TValue1 GetValue1(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values1[index];
    }

    public void SetValue1(int index, TValue1 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values1[index] = value;
    }

    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public TValue2 GetValue2(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values2[index];
    }

    public void SetValue2(int index, TValue2 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values2[index] = value;
    }

    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public TValue3 GetValue3(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values3[index];
    }

    public void SetValue3(int index, TValue3 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values3[index] = value;
    }

    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public TValue4 GetValue4(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values4[index];
    }

    public void SetValue4(int index, TValue4 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values4[index] = value;
    }

    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    

    public bool SwapRemove(int index)
    {
        if (index < 0 || index >= _count)
            return false;

        int lastIndex = _count - 1;
        if (index != lastIndex)
        {
            
            _values1[index] = _values1[lastIndex];
            _values2[index] = _values2[lastIndex];
            _values3[index] = _values3[lastIndex];
            _values4[index] = _values4[lastIndex];
        }
        _count--;
        return true;
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

    public void Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;
        _values4[_count] = value4;
        _values5[_count] = value5;

        _count++;
    }

    
    public TValue1 GetValue1(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values1[index];
    }

    public void SetValue1(int index, TValue1 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values1[index] = value;
    }

    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public TValue2 GetValue2(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values2[index];
    }

    public void SetValue2(int index, TValue2 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values2[index] = value;
    }

    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public TValue3 GetValue3(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values3[index];
    }

    public void SetValue3(int index, TValue3 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values3[index] = value;
    }

    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public TValue4 GetValue4(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values4[index];
    }

    public void SetValue4(int index, TValue4 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values4[index] = value;
    }

    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    
    public TValue5 GetValue5(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values5[index];
    }

    public void SetValue5(int index, TValue5 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values5[index] = value;
    }

    public Span<TValue5> Values5 => new Span<TValue5>(_values5, 0, _count);
    

    public bool SwapRemove(int index)
    {
        if (index < 0 || index >= _count)
            return false;

        int lastIndex = _count - 1;
        if (index != lastIndex)
        {
            
            _values1[index] = _values1[lastIndex];
            _values2[index] = _values2[lastIndex];
            _values3[index] = _values3[lastIndex];
            _values4[index] = _values4[lastIndex];
            _values5[index] = _values5[lastIndex];
        }
        _count--;
        return true;
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

    public void Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;
        _values4[_count] = value4;
        _values5[_count] = value5;
        _values6[_count] = value6;

        _count++;
    }

    
    public TValue1 GetValue1(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values1[index];
    }

    public void SetValue1(int index, TValue1 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values1[index] = value;
    }

    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public TValue2 GetValue2(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values2[index];
    }

    public void SetValue2(int index, TValue2 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values2[index] = value;
    }

    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public TValue3 GetValue3(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values3[index];
    }

    public void SetValue3(int index, TValue3 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values3[index] = value;
    }

    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public TValue4 GetValue4(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values4[index];
    }

    public void SetValue4(int index, TValue4 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values4[index] = value;
    }

    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    
    public TValue5 GetValue5(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values5[index];
    }

    public void SetValue5(int index, TValue5 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values5[index] = value;
    }

    public Span<TValue5> Values5 => new Span<TValue5>(_values5, 0, _count);
    
    public TValue6 GetValue6(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values6[index];
    }

    public void SetValue6(int index, TValue6 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values6[index] = value;
    }

    public Span<TValue6> Values6 => new Span<TValue6>(_values6, 0, _count);
    

    public bool SwapRemove(int index)
    {
        if (index < 0 || index >= _count)
            return false;

        int lastIndex = _count - 1;
        if (index != lastIndex)
        {
            
            _values1[index] = _values1[lastIndex];
            _values2[index] = _values2[lastIndex];
            _values3[index] = _values3[lastIndex];
            _values4[index] = _values4[lastIndex];
            _values5[index] = _values5[lastIndex];
            _values6[index] = _values6[lastIndex];
        }
        _count--;
        return true;
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

    public void Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;
        _values4[_count] = value4;
        _values5[_count] = value5;
        _values6[_count] = value6;
        _values7[_count] = value7;

        _count++;
    }

    
    public TValue1 GetValue1(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values1[index];
    }

    public void SetValue1(int index, TValue1 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values1[index] = value;
    }

    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public TValue2 GetValue2(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values2[index];
    }

    public void SetValue2(int index, TValue2 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values2[index] = value;
    }

    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public TValue3 GetValue3(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values3[index];
    }

    public void SetValue3(int index, TValue3 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values3[index] = value;
    }

    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public TValue4 GetValue4(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values4[index];
    }

    public void SetValue4(int index, TValue4 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values4[index] = value;
    }

    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    
    public TValue5 GetValue5(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values5[index];
    }

    public void SetValue5(int index, TValue5 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values5[index] = value;
    }

    public Span<TValue5> Values5 => new Span<TValue5>(_values5, 0, _count);
    
    public TValue6 GetValue6(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values6[index];
    }

    public void SetValue6(int index, TValue6 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values6[index] = value;
    }

    public Span<TValue6> Values6 => new Span<TValue6>(_values6, 0, _count);
    
    public TValue7 GetValue7(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values7[index];
    }

    public void SetValue7(int index, TValue7 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values7[index] = value;
    }

    public Span<TValue7> Values7 => new Span<TValue7>(_values7, 0, _count);
    

    public bool SwapRemove(int index)
    {
        if (index < 0 || index >= _count)
            return false;

        int lastIndex = _count - 1;
        if (index != lastIndex)
        {
            
            _values1[index] = _values1[lastIndex];
            _values2[index] = _values2[lastIndex];
            _values3[index] = _values3[lastIndex];
            _values4[index] = _values4[lastIndex];
            _values5[index] = _values5[lastIndex];
            _values6[index] = _values6[lastIndex];
            _values7[index] = _values7[lastIndex];
        }
        _count--;
        return true;
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

    public void Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8)
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

        _count++;
    }

    
    public TValue1 GetValue1(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values1[index];
    }

    public void SetValue1(int index, TValue1 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values1[index] = value;
    }

    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public TValue2 GetValue2(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values2[index];
    }

    public void SetValue2(int index, TValue2 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values2[index] = value;
    }

    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public TValue3 GetValue3(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values3[index];
    }

    public void SetValue3(int index, TValue3 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values3[index] = value;
    }

    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public TValue4 GetValue4(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values4[index];
    }

    public void SetValue4(int index, TValue4 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values4[index] = value;
    }

    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    
    public TValue5 GetValue5(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values5[index];
    }

    public void SetValue5(int index, TValue5 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values5[index] = value;
    }

    public Span<TValue5> Values5 => new Span<TValue5>(_values5, 0, _count);
    
    public TValue6 GetValue6(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values6[index];
    }

    public void SetValue6(int index, TValue6 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values6[index] = value;
    }

    public Span<TValue6> Values6 => new Span<TValue6>(_values6, 0, _count);
    
    public TValue7 GetValue7(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values7[index];
    }

    public void SetValue7(int index, TValue7 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values7[index] = value;
    }

    public Span<TValue7> Values7 => new Span<TValue7>(_values7, 0, _count);
    
    public TValue8 GetValue8(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values8[index];
    }

    public void SetValue8(int index, TValue8 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values8[index] = value;
    }

    public Span<TValue8> Values8 => new Span<TValue8>(_values8, 0, _count);
    

    public bool SwapRemove(int index)
    {
        if (index < 0 || index >= _count)
            return false;

        int lastIndex = _count - 1;
        if (index != lastIndex)
        {
            
            _values1[index] = _values1[lastIndex];
            _values2[index] = _values2[lastIndex];
            _values3[index] = _values3[lastIndex];
            _values4[index] = _values4[lastIndex];
            _values5[index] = _values5[lastIndex];
            _values6[index] = _values6[lastIndex];
            _values7[index] = _values7[lastIndex];
            _values8[index] = _values8[lastIndex];
        }
        _count--;
        return true;
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

    public void Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8, TValue9 value9)
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

        _count++;
    }

    
    public TValue1 GetValue1(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values1[index];
    }

    public void SetValue1(int index, TValue1 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values1[index] = value;
    }

    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public TValue2 GetValue2(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values2[index];
    }

    public void SetValue2(int index, TValue2 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values2[index] = value;
    }

    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public TValue3 GetValue3(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values3[index];
    }

    public void SetValue3(int index, TValue3 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values3[index] = value;
    }

    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public TValue4 GetValue4(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values4[index];
    }

    public void SetValue4(int index, TValue4 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values4[index] = value;
    }

    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    
    public TValue5 GetValue5(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values5[index];
    }

    public void SetValue5(int index, TValue5 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values5[index] = value;
    }

    public Span<TValue5> Values5 => new Span<TValue5>(_values5, 0, _count);
    
    public TValue6 GetValue6(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values6[index];
    }

    public void SetValue6(int index, TValue6 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values6[index] = value;
    }

    public Span<TValue6> Values6 => new Span<TValue6>(_values6, 0, _count);
    
    public TValue7 GetValue7(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values7[index];
    }

    public void SetValue7(int index, TValue7 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values7[index] = value;
    }

    public Span<TValue7> Values7 => new Span<TValue7>(_values7, 0, _count);
    
    public TValue8 GetValue8(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values8[index];
    }

    public void SetValue8(int index, TValue8 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values8[index] = value;
    }

    public Span<TValue8> Values8 => new Span<TValue8>(_values8, 0, _count);
    
    public TValue9 GetValue9(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values9[index];
    }

    public void SetValue9(int index, TValue9 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values9[index] = value;
    }

    public Span<TValue9> Values9 => new Span<TValue9>(_values9, 0, _count);
    

    public bool SwapRemove(int index)
    {
        if (index < 0 || index >= _count)
            return false;

        int lastIndex = _count - 1;
        if (index != lastIndex)
        {
            
            _values1[index] = _values1[lastIndex];
            _values2[index] = _values2[lastIndex];
            _values3[index] = _values3[lastIndex];
            _values4[index] = _values4[lastIndex];
            _values5[index] = _values5[lastIndex];
            _values6[index] = _values6[lastIndex];
            _values7[index] = _values7[lastIndex];
            _values8[index] = _values8[lastIndex];
            _values9[index] = _values9[lastIndex];
        }
        _count--;
        return true;
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

    public void Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8, TValue9 value9, TValue10 value10)
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

        _count++;
    }

    
    public TValue1 GetValue1(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values1[index];
    }

    public void SetValue1(int index, TValue1 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values1[index] = value;
    }

    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public TValue2 GetValue2(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values2[index];
    }

    public void SetValue2(int index, TValue2 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values2[index] = value;
    }

    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public TValue3 GetValue3(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values3[index];
    }

    public void SetValue3(int index, TValue3 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values3[index] = value;
    }

    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public TValue4 GetValue4(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values4[index];
    }

    public void SetValue4(int index, TValue4 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values4[index] = value;
    }

    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    
    public TValue5 GetValue5(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values5[index];
    }

    public void SetValue5(int index, TValue5 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values5[index] = value;
    }

    public Span<TValue5> Values5 => new Span<TValue5>(_values5, 0, _count);
    
    public TValue6 GetValue6(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values6[index];
    }

    public void SetValue6(int index, TValue6 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values6[index] = value;
    }

    public Span<TValue6> Values6 => new Span<TValue6>(_values6, 0, _count);
    
    public TValue7 GetValue7(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values7[index];
    }

    public void SetValue7(int index, TValue7 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values7[index] = value;
    }

    public Span<TValue7> Values7 => new Span<TValue7>(_values7, 0, _count);
    
    public TValue8 GetValue8(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values8[index];
    }

    public void SetValue8(int index, TValue8 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values8[index] = value;
    }

    public Span<TValue8> Values8 => new Span<TValue8>(_values8, 0, _count);
    
    public TValue9 GetValue9(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values9[index];
    }

    public void SetValue9(int index, TValue9 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values9[index] = value;
    }

    public Span<TValue9> Values9 => new Span<TValue9>(_values9, 0, _count);
    
    public TValue10 GetValue10(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values10[index];
    }

    public void SetValue10(int index, TValue10 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values10[index] = value;
    }

    public Span<TValue10> Values10 => new Span<TValue10>(_values10, 0, _count);
    

    public bool SwapRemove(int index)
    {
        if (index < 0 || index >= _count)
            return false;

        int lastIndex = _count - 1;
        if (index != lastIndex)
        {
            
            _values1[index] = _values1[lastIndex];
            _values2[index] = _values2[lastIndex];
            _values3[index] = _values3[lastIndex];
            _values4[index] = _values4[lastIndex];
            _values5[index] = _values5[lastIndex];
            _values6[index] = _values6[lastIndex];
            _values7[index] = _values7[lastIndex];
            _values8[index] = _values8[lastIndex];
            _values9[index] = _values9[lastIndex];
            _values10[index] = _values10[lastIndex];
        }
        _count--;
        return true;
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


public struct MultiArrayStruct<TValue1>
{
    private const int DefaultCapacity = 32;
    private int _count;
    
    private TValue1[] _values1;

    public MultiArrayStruct(int initialCapacity)
    {
        if (initialCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        
        _values1 = new TValue1[initialCapacity];
    }

    public MultiArrayStruct()
    {
        _values1 = new TValue1[DefaultCapacity];
        
    }

    public int Count => _count;

    private void EnsureCapacity(int minCapacity)
    {
        int capacity = _values1.Length;
        if (minCapacity > capacity)
        {
            int newCapacity = Math.Max(capacity * 2, minCapacity);

            
            Array.Resize(ref _values1, newCapacity);
        }
    }

    public void Add(TValue1 value1)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;

        _count++;
    }

    
    public TValue1 GetValue1(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values1[index];
    }

    public void SetValue1(int index, TValue1 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values1[index] = value;
    }

    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    

    public bool SwapRemove(int index)
    {
        if (index < 0 || index >= _count)
            return false;

        int lastIndex = _count - 1;
        if (index != lastIndex)
        {
            
            _values1[index] = _values1[lastIndex];
        }
        _count--;
        return true;
    }

    public void Clear()
    {
        
        Array.Clear(_values1, 0, _count);
        _count = 0;
    }

    public void TrimExcess()
    {
        int capacity = _values1.Length;
        if (_count < capacity * 0.9)
        {
            int newCapacity = Math.Max(DefaultCapacity, _count);
            
            Array.Resize(ref _values1, newCapacity);
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

    public void Add(TValue1 value1, TValue2 value2)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;

        _count++;
    }

    
    public TValue1 GetValue1(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values1[index];
    }

    public void SetValue1(int index, TValue1 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values1[index] = value;
    }

    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public TValue2 GetValue2(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values2[index];
    }

    public void SetValue2(int index, TValue2 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values2[index] = value;
    }

    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    

    public bool SwapRemove(int index)
    {
        if (index < 0 || index >= _count)
            return false;

        int lastIndex = _count - 1;
        if (index != lastIndex)
        {
            
            _values1[index] = _values1[lastIndex];
            _values2[index] = _values2[lastIndex];
        }
        _count--;
        return true;
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

    public void Add(TValue1 value1, TValue2 value2, TValue3 value3)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;

        _count++;
    }

    
    public TValue1 GetValue1(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values1[index];
    }

    public void SetValue1(int index, TValue1 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values1[index] = value;
    }

    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public TValue2 GetValue2(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values2[index];
    }

    public void SetValue2(int index, TValue2 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values2[index] = value;
    }

    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public TValue3 GetValue3(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values3[index];
    }

    public void SetValue3(int index, TValue3 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values3[index] = value;
    }

    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    

    public bool SwapRemove(int index)
    {
        if (index < 0 || index >= _count)
            return false;

        int lastIndex = _count - 1;
        if (index != lastIndex)
        {
            
            _values1[index] = _values1[lastIndex];
            _values2[index] = _values2[lastIndex];
            _values3[index] = _values3[lastIndex];
        }
        _count--;
        return true;
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

    public void Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;
        _values4[_count] = value4;

        _count++;
    }

    
    public TValue1 GetValue1(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values1[index];
    }

    public void SetValue1(int index, TValue1 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values1[index] = value;
    }

    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public TValue2 GetValue2(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values2[index];
    }

    public void SetValue2(int index, TValue2 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values2[index] = value;
    }

    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public TValue3 GetValue3(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values3[index];
    }

    public void SetValue3(int index, TValue3 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values3[index] = value;
    }

    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public TValue4 GetValue4(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values4[index];
    }

    public void SetValue4(int index, TValue4 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values4[index] = value;
    }

    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    

    public bool SwapRemove(int index)
    {
        if (index < 0 || index >= _count)
            return false;

        int lastIndex = _count - 1;
        if (index != lastIndex)
        {
            
            _values1[index] = _values1[lastIndex];
            _values2[index] = _values2[lastIndex];
            _values3[index] = _values3[lastIndex];
            _values4[index] = _values4[lastIndex];
        }
        _count--;
        return true;
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

    public void Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;
        _values4[_count] = value4;
        _values5[_count] = value5;

        _count++;
    }

    
    public TValue1 GetValue1(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values1[index];
    }

    public void SetValue1(int index, TValue1 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values1[index] = value;
    }

    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public TValue2 GetValue2(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values2[index];
    }

    public void SetValue2(int index, TValue2 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values2[index] = value;
    }

    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public TValue3 GetValue3(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values3[index];
    }

    public void SetValue3(int index, TValue3 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values3[index] = value;
    }

    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public TValue4 GetValue4(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values4[index];
    }

    public void SetValue4(int index, TValue4 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values4[index] = value;
    }

    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    
    public TValue5 GetValue5(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values5[index];
    }

    public void SetValue5(int index, TValue5 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values5[index] = value;
    }

    public Span<TValue5> Values5 => new Span<TValue5>(_values5, 0, _count);
    

    public bool SwapRemove(int index)
    {
        if (index < 0 || index >= _count)
            return false;

        int lastIndex = _count - 1;
        if (index != lastIndex)
        {
            
            _values1[index] = _values1[lastIndex];
            _values2[index] = _values2[lastIndex];
            _values3[index] = _values3[lastIndex];
            _values4[index] = _values4[lastIndex];
            _values5[index] = _values5[lastIndex];
        }
        _count--;
        return true;
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

    public void Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;
        _values4[_count] = value4;
        _values5[_count] = value5;
        _values6[_count] = value6;

        _count++;
    }

    
    public TValue1 GetValue1(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values1[index];
    }

    public void SetValue1(int index, TValue1 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values1[index] = value;
    }

    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public TValue2 GetValue2(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values2[index];
    }

    public void SetValue2(int index, TValue2 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values2[index] = value;
    }

    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public TValue3 GetValue3(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values3[index];
    }

    public void SetValue3(int index, TValue3 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values3[index] = value;
    }

    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public TValue4 GetValue4(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values4[index];
    }

    public void SetValue4(int index, TValue4 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values4[index] = value;
    }

    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    
    public TValue5 GetValue5(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values5[index];
    }

    public void SetValue5(int index, TValue5 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values5[index] = value;
    }

    public Span<TValue5> Values5 => new Span<TValue5>(_values5, 0, _count);
    
    public TValue6 GetValue6(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values6[index];
    }

    public void SetValue6(int index, TValue6 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values6[index] = value;
    }

    public Span<TValue6> Values6 => new Span<TValue6>(_values6, 0, _count);
    

    public bool SwapRemove(int index)
    {
        if (index < 0 || index >= _count)
            return false;

        int lastIndex = _count - 1;
        if (index != lastIndex)
        {
            
            _values1[index] = _values1[lastIndex];
            _values2[index] = _values2[lastIndex];
            _values3[index] = _values3[lastIndex];
            _values4[index] = _values4[lastIndex];
            _values5[index] = _values5[lastIndex];
            _values6[index] = _values6[lastIndex];
        }
        _count--;
        return true;
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

    public void Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7)
    {
        EnsureCapacity(_count + 1);

        
        _values1[_count] = value1;
        _values2[_count] = value2;
        _values3[_count] = value3;
        _values4[_count] = value4;
        _values5[_count] = value5;
        _values6[_count] = value6;
        _values7[_count] = value7;

        _count++;
    }

    
    public TValue1 GetValue1(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values1[index];
    }

    public void SetValue1(int index, TValue1 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values1[index] = value;
    }

    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public TValue2 GetValue2(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values2[index];
    }

    public void SetValue2(int index, TValue2 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values2[index] = value;
    }

    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public TValue3 GetValue3(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values3[index];
    }

    public void SetValue3(int index, TValue3 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values3[index] = value;
    }

    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public TValue4 GetValue4(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values4[index];
    }

    public void SetValue4(int index, TValue4 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values4[index] = value;
    }

    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    
    public TValue5 GetValue5(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values5[index];
    }

    public void SetValue5(int index, TValue5 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values5[index] = value;
    }

    public Span<TValue5> Values5 => new Span<TValue5>(_values5, 0, _count);
    
    public TValue6 GetValue6(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values6[index];
    }

    public void SetValue6(int index, TValue6 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values6[index] = value;
    }

    public Span<TValue6> Values6 => new Span<TValue6>(_values6, 0, _count);
    
    public TValue7 GetValue7(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values7[index];
    }

    public void SetValue7(int index, TValue7 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values7[index] = value;
    }

    public Span<TValue7> Values7 => new Span<TValue7>(_values7, 0, _count);
    

    public bool SwapRemove(int index)
    {
        if (index < 0 || index >= _count)
            return false;

        int lastIndex = _count - 1;
        if (index != lastIndex)
        {
            
            _values1[index] = _values1[lastIndex];
            _values2[index] = _values2[lastIndex];
            _values3[index] = _values3[lastIndex];
            _values4[index] = _values4[lastIndex];
            _values5[index] = _values5[lastIndex];
            _values6[index] = _values6[lastIndex];
            _values7[index] = _values7[lastIndex];
        }
        _count--;
        return true;
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

    public void Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8)
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

        _count++;
    }

    
    public TValue1 GetValue1(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values1[index];
    }

    public void SetValue1(int index, TValue1 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values1[index] = value;
    }

    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public TValue2 GetValue2(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values2[index];
    }

    public void SetValue2(int index, TValue2 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values2[index] = value;
    }

    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public TValue3 GetValue3(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values3[index];
    }

    public void SetValue3(int index, TValue3 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values3[index] = value;
    }

    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public TValue4 GetValue4(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values4[index];
    }

    public void SetValue4(int index, TValue4 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values4[index] = value;
    }

    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    
    public TValue5 GetValue5(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values5[index];
    }

    public void SetValue5(int index, TValue5 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values5[index] = value;
    }

    public Span<TValue5> Values5 => new Span<TValue5>(_values5, 0, _count);
    
    public TValue6 GetValue6(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values6[index];
    }

    public void SetValue6(int index, TValue6 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values6[index] = value;
    }

    public Span<TValue6> Values6 => new Span<TValue6>(_values6, 0, _count);
    
    public TValue7 GetValue7(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values7[index];
    }

    public void SetValue7(int index, TValue7 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values7[index] = value;
    }

    public Span<TValue7> Values7 => new Span<TValue7>(_values7, 0, _count);
    
    public TValue8 GetValue8(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values8[index];
    }

    public void SetValue8(int index, TValue8 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values8[index] = value;
    }

    public Span<TValue8> Values8 => new Span<TValue8>(_values8, 0, _count);
    

    public bool SwapRemove(int index)
    {
        if (index < 0 || index >= _count)
            return false;

        int lastIndex = _count - 1;
        if (index != lastIndex)
        {
            
            _values1[index] = _values1[lastIndex];
            _values2[index] = _values2[lastIndex];
            _values3[index] = _values3[lastIndex];
            _values4[index] = _values4[lastIndex];
            _values5[index] = _values5[lastIndex];
            _values6[index] = _values6[lastIndex];
            _values7[index] = _values7[lastIndex];
            _values8[index] = _values8[lastIndex];
        }
        _count--;
        return true;
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

    public void Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8, TValue9 value9)
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

        _count++;
    }

    
    public TValue1 GetValue1(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values1[index];
    }

    public void SetValue1(int index, TValue1 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values1[index] = value;
    }

    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public TValue2 GetValue2(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values2[index];
    }

    public void SetValue2(int index, TValue2 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values2[index] = value;
    }

    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public TValue3 GetValue3(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values3[index];
    }

    public void SetValue3(int index, TValue3 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values3[index] = value;
    }

    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public TValue4 GetValue4(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values4[index];
    }

    public void SetValue4(int index, TValue4 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values4[index] = value;
    }

    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    
    public TValue5 GetValue5(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values5[index];
    }

    public void SetValue5(int index, TValue5 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values5[index] = value;
    }

    public Span<TValue5> Values5 => new Span<TValue5>(_values5, 0, _count);
    
    public TValue6 GetValue6(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values6[index];
    }

    public void SetValue6(int index, TValue6 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values6[index] = value;
    }

    public Span<TValue6> Values6 => new Span<TValue6>(_values6, 0, _count);
    
    public TValue7 GetValue7(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values7[index];
    }

    public void SetValue7(int index, TValue7 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values7[index] = value;
    }

    public Span<TValue7> Values7 => new Span<TValue7>(_values7, 0, _count);
    
    public TValue8 GetValue8(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values8[index];
    }

    public void SetValue8(int index, TValue8 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values8[index] = value;
    }

    public Span<TValue8> Values8 => new Span<TValue8>(_values8, 0, _count);
    
    public TValue9 GetValue9(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values9[index];
    }

    public void SetValue9(int index, TValue9 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values9[index] = value;
    }

    public Span<TValue9> Values9 => new Span<TValue9>(_values9, 0, _count);
    

    public bool SwapRemove(int index)
    {
        if (index < 0 || index >= _count)
            return false;

        int lastIndex = _count - 1;
        if (index != lastIndex)
        {
            
            _values1[index] = _values1[lastIndex];
            _values2[index] = _values2[lastIndex];
            _values3[index] = _values3[lastIndex];
            _values4[index] = _values4[lastIndex];
            _values5[index] = _values5[lastIndex];
            _values6[index] = _values6[lastIndex];
            _values7[index] = _values7[lastIndex];
            _values8[index] = _values8[lastIndex];
            _values9[index] = _values9[lastIndex];
        }
        _count--;
        return true;
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

    public void Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8, TValue9 value9, TValue10 value10)
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

        _count++;
    }

    
    public TValue1 GetValue1(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values1[index];
    }

    public void SetValue1(int index, TValue1 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values1[index] = value;
    }

    public Span<TValue1> Values1 => new Span<TValue1>(_values1, 0, _count);
    
    public TValue2 GetValue2(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values2[index];
    }

    public void SetValue2(int index, TValue2 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values2[index] = value;
    }

    public Span<TValue2> Values2 => new Span<TValue2>(_values2, 0, _count);
    
    public TValue3 GetValue3(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values3[index];
    }

    public void SetValue3(int index, TValue3 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values3[index] = value;
    }

    public Span<TValue3> Values3 => new Span<TValue3>(_values3, 0, _count);
    
    public TValue4 GetValue4(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values4[index];
    }

    public void SetValue4(int index, TValue4 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values4[index] = value;
    }

    public Span<TValue4> Values4 => new Span<TValue4>(_values4, 0, _count);
    
    public TValue5 GetValue5(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values5[index];
    }

    public void SetValue5(int index, TValue5 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values5[index] = value;
    }

    public Span<TValue5> Values5 => new Span<TValue5>(_values5, 0, _count);
    
    public TValue6 GetValue6(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values6[index];
    }

    public void SetValue6(int index, TValue6 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values6[index] = value;
    }

    public Span<TValue6> Values6 => new Span<TValue6>(_values6, 0, _count);
    
    public TValue7 GetValue7(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values7[index];
    }

    public void SetValue7(int index, TValue7 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values7[index] = value;
    }

    public Span<TValue7> Values7 => new Span<TValue7>(_values7, 0, _count);
    
    public TValue8 GetValue8(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values8[index];
    }

    public void SetValue8(int index, TValue8 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values8[index] = value;
    }

    public Span<TValue8> Values8 => new Span<TValue8>(_values8, 0, _count);
    
    public TValue9 GetValue9(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values9[index];
    }

    public void SetValue9(int index, TValue9 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values9[index] = value;
    }

    public Span<TValue9> Values9 => new Span<TValue9>(_values9, 0, _count);
    
    public TValue10 GetValue10(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _values10[index];
    }

    public void SetValue10(int index, TValue10 value)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _values10[index] = value;
    }

    public Span<TValue10> Values10 => new Span<TValue10>(_values10, 0, _count);
    

    public bool SwapRemove(int index)
    {
        if (index < 0 || index >= _count)
            return false;

        int lastIndex = _count - 1;
        if (index != lastIndex)
        {
            
            _values1[index] = _values1[lastIndex];
            _values2[index] = _values2[lastIndex];
            _values3[index] = _values3[lastIndex];
            _values4[index] = _values4[lastIndex];
            _values5[index] = _values5[lastIndex];
            _values6[index] = _values6[lastIndex];
            _values7[index] = _values7[lastIndex];
            _values8[index] = _values8[lastIndex];
            _values9[index] = _values9[lastIndex];
            _values10[index] = _values10[lastIndex];
        }
        _count--;
        return true;
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



