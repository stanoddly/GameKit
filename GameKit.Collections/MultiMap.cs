using System.Runtime.CompilerServices;

// Generated using jinja2-cli: jinja2 MultiMap.cs.jinja > MultiMap.cs
namespace GameKit.Collections;
public class MultiMap<TValue1>
{
    private FastListStruct<int> _sparse;
    private MultiArrayStruct<Handle, TValue1> _dense;
    private const int TombStone = int.MaxValue;  

    public MultiMap()
    {
        _sparse = new FastListStruct<int>();
        _dense = new MultiArrayStruct<Handle, TValue1>();
    }

    public void Set(Handle handle, TValue1 value1)
    {
        if (handle.IsNull())
        {
            return;
        }

        int sparseIndex = handle;

        if (sparseIndex > _sparse.LastIndex)
        {
            _sparse.ResizeFill(sparseIndex + 1, TombStone);
        }

        ref int denseIndex = ref _sparse[sparseIndex];

        // nonexistent
        if (denseIndex == TombStone)
        {
            denseIndex = _dense.Add(handle, value1);
            return;
        }

        _dense.Set(denseIndex, handle, value1);
    }

    public bool TryGet(Handle handle, out TValue1 value1)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out value1);
        
            return true;
        }

        
        value1 = default;

        return false;
    }
    
    

    public bool Remove(Handle handle)
    {
        int index = handle;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref int denseIndex = ref _sparse[index];

        if (denseIndex == TombStone)
        {
            return false;
        }
        
        if (_dense.SwapRemoveReturnFirst(denseIndex, out Handle swappedHandle))
        {
            ref int swappedSparseIndex = ref _sparse[(int)swappedHandle];
            swappedSparseIndex = denseIndex;
        }

        denseIndex = TombStone;
        return true;
    }

    public bool Contains(Handle handle)
    {
        return Contains(handle, out _);
    }
    
    public bool Contains(Handle handle, out int handleIndex) 
    {
        int index = handle;
        if (index > _sparse.LastIndex)
        {
            handleIndex = default;
            return false;
        }
        
        handleIndex = _sparse[index];

        return handleIndex != TombStone;
    }
    
    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue2(index, out value);
    }

    
    public ref TValue1 UnsafeGetRefValue1(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue1>();
        }

        exists = true;
        return ref _dense.GetRefValue2(index);
    }
    
    
    public void GetValues1OrDefault(ReadOnlySpan<Handle> handles, Span<TValue1> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue2(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }

    public int Length => _dense.Length;

    public ReadOnlySpan<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
}

public class MultiMap<TValue1, TValue2>
{
    private FastListStruct<int> _sparse;
    private MultiArrayStruct<Handle, TValue1, TValue2> _dense;
    private const int TombStone = int.MaxValue;  

    public MultiMap()
    {
        _sparse = new FastListStruct<int>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2>();
    }

    public void Set(Handle handle, TValue1 value1, TValue2 value2)
    {
        if (handle.IsNull())
        {
            return;
        }

        int sparseIndex = handle;

        if (sparseIndex > _sparse.LastIndex)
        {
            _sparse.ResizeFill(sparseIndex + 1, TombStone);
        }

        ref int denseIndex = ref _sparse[sparseIndex];

        // nonexistent
        if (denseIndex == TombStone)
        {
            denseIndex = _dense.Add(handle, value1, value2);
            return;
        }

        _dense.Set(denseIndex, handle, value1, value2);
    }

    public bool TryGet(Handle handle, out TValue1 value1, out TValue2 value2)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out value1, out value2);
        
            return true;
        }

        
        value1 = default;
        value2 = default;

        return false;
    }
    
    
    public bool TryGet(Handle handle, out (TValue1, TValue2) tuple)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out TValue1 value1, out TValue2 value2);
            tuple = (value1, value2);

            return true;
        }

        tuple = default;

        return false;
    }
    

    public bool Remove(Handle handle)
    {
        int index = handle;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref int denseIndex = ref _sparse[index];

        if (denseIndex == TombStone)
        {
            return false;
        }
        
        if (_dense.SwapRemoveReturnFirst(denseIndex, out Handle swappedHandle))
        {
            ref int swappedSparseIndex = ref _sparse[(int)swappedHandle];
            swappedSparseIndex = denseIndex;
        }

        denseIndex = TombStone;
        return true;
    }

    public bool Contains(Handle handle)
    {
        return Contains(handle, out _);
    }
    
    public bool Contains(Handle handle, out int handleIndex) 
    {
        int index = handle;
        if (index > _sparse.LastIndex)
        {
            handleIndex = default;
            return false;
        }
        
        handleIndex = _sparse[index];

        return handleIndex != TombStone;
    }
    
    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue2(index, out value);
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue3(index, out value);
    }

    
    public ref TValue1 UnsafeGetRefValue1(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue1>();
        }

        exists = true;
        return ref _dense.GetRefValue2(index);
    }
    public ref TValue2 UnsafeGetRefValue2(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue2>();
        }

        exists = true;
        return ref _dense.GetRefValue3(index);
    }
    
    
    public void GetValues1OrDefault(ReadOnlySpan<Handle> handles, Span<TValue1> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue2(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues2OrDefault(ReadOnlySpan<Handle> handles, Span<TValue2> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue3(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }

    public int Length => _dense.Length;

    public ReadOnlySpan<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
}

public class MultiMap<TValue1, TValue2, TValue3>
{
    private FastListStruct<int> _sparse;
    private MultiArrayStruct<Handle, TValue1, TValue2, TValue3> _dense;
    private const int TombStone = int.MaxValue;  

    public MultiMap()
    {
        _sparse = new FastListStruct<int>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2, TValue3>();
    }

    public void Set(Handle handle, TValue1 value1, TValue2 value2, TValue3 value3)
    {
        if (handle.IsNull())
        {
            return;
        }

        int sparseIndex = handle;

        if (sparseIndex > _sparse.LastIndex)
        {
            _sparse.ResizeFill(sparseIndex + 1, TombStone);
        }

        ref int denseIndex = ref _sparse[sparseIndex];

        // nonexistent
        if (denseIndex == TombStone)
        {
            denseIndex = _dense.Add(handle, value1, value2, value3);
            return;
        }

        _dense.Set(denseIndex, handle, value1, value2, value3);
    }

    public bool TryGet(Handle handle, out TValue1 value1, out TValue2 value2, out TValue3 value3)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out value1, out value2, out value3);
        
            return true;
        }

        
        value1 = default;
        value2 = default;
        value3 = default;

        return false;
    }
    
    
    public bool TryGet(Handle handle, out (TValue1, TValue2, TValue3) tuple)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out TValue1 value1, out TValue2 value2, out TValue3 value3);
            tuple = (value1, value2, value3);

            return true;
        }

        tuple = default;

        return false;
    }
    

    public bool Remove(Handle handle)
    {
        int index = handle;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref int denseIndex = ref _sparse[index];

        if (denseIndex == TombStone)
        {
            return false;
        }
        
        if (_dense.SwapRemoveReturnFirst(denseIndex, out Handle swappedHandle))
        {
            ref int swappedSparseIndex = ref _sparse[(int)swappedHandle];
            swappedSparseIndex = denseIndex;
        }

        denseIndex = TombStone;
        return true;
    }

    public bool Contains(Handle handle)
    {
        return Contains(handle, out _);
    }
    
    public bool Contains(Handle handle, out int handleIndex) 
    {
        int index = handle;
        if (index > _sparse.LastIndex)
        {
            handleIndex = default;
            return false;
        }
        
        handleIndex = _sparse[index];

        return handleIndex != TombStone;
    }
    
    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue2(index, out value);
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue3(index, out value);
    }
    public bool TryGetValue3(Handle handle, out TValue3 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue4(index, out value);
    }

    
    public ref TValue1 UnsafeGetRefValue1(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue1>();
        }

        exists = true;
        return ref _dense.GetRefValue2(index);
    }
    public ref TValue2 UnsafeGetRefValue2(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue2>();
        }

        exists = true;
        return ref _dense.GetRefValue3(index);
    }
    public ref TValue3 UnsafeGetRefValue3(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue3>();
        }

        exists = true;
        return ref _dense.GetRefValue4(index);
    }
    
    
    public void GetValues1OrDefault(ReadOnlySpan<Handle> handles, Span<TValue1> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue2(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues2OrDefault(ReadOnlySpan<Handle> handles, Span<TValue2> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue3(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues3OrDefault(ReadOnlySpan<Handle> handles, Span<TValue3> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue4(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }

    public int Length => _dense.Length;

    public ReadOnlySpan<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
}

public class MultiMap<TValue1, TValue2, TValue3, TValue4>
{
    private FastListStruct<int> _sparse;
    private MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4> _dense;
    private const int TombStone = int.MaxValue;  

    public MultiMap()
    {
        _sparse = new FastListStruct<int>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4>();
    }

    public void Set(Handle handle, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4)
    {
        if (handle.IsNull())
        {
            return;
        }

        int sparseIndex = handle;

        if (sparseIndex > _sparse.LastIndex)
        {
            _sparse.ResizeFill(sparseIndex + 1, TombStone);
        }

        ref int denseIndex = ref _sparse[sparseIndex];

        // nonexistent
        if (denseIndex == TombStone)
        {
            denseIndex = _dense.Add(handle, value1, value2, value3, value4);
            return;
        }

        _dense.Set(denseIndex, handle, value1, value2, value3, value4);
    }

    public bool TryGet(Handle handle, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out value1, out value2, out value3, out value4);
        
            return true;
        }

        
        value1 = default;
        value2 = default;
        value3 = default;
        value4 = default;

        return false;
    }
    
    
    public bool TryGet(Handle handle, out (TValue1, TValue2, TValue3, TValue4) tuple)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4);
            tuple = (value1, value2, value3, value4);

            return true;
        }

        tuple = default;

        return false;
    }
    

    public bool Remove(Handle handle)
    {
        int index = handle;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref int denseIndex = ref _sparse[index];

        if (denseIndex == TombStone)
        {
            return false;
        }
        
        if (_dense.SwapRemoveReturnFirst(denseIndex, out Handle swappedHandle))
        {
            ref int swappedSparseIndex = ref _sparse[(int)swappedHandle];
            swappedSparseIndex = denseIndex;
        }

        denseIndex = TombStone;
        return true;
    }

    public bool Contains(Handle handle)
    {
        return Contains(handle, out _);
    }
    
    public bool Contains(Handle handle, out int handleIndex) 
    {
        int index = handle;
        if (index > _sparse.LastIndex)
        {
            handleIndex = default;
            return false;
        }
        
        handleIndex = _sparse[index];

        return handleIndex != TombStone;
    }
    
    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue2(index, out value);
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue3(index, out value);
    }
    public bool TryGetValue3(Handle handle, out TValue3 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue4(index, out value);
    }
    public bool TryGetValue4(Handle handle, out TValue4 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue5(index, out value);
    }

    
    public ref TValue1 UnsafeGetRefValue1(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue1>();
        }

        exists = true;
        return ref _dense.GetRefValue2(index);
    }
    public ref TValue2 UnsafeGetRefValue2(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue2>();
        }

        exists = true;
        return ref _dense.GetRefValue3(index);
    }
    public ref TValue3 UnsafeGetRefValue3(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue3>();
        }

        exists = true;
        return ref _dense.GetRefValue4(index);
    }
    public ref TValue4 UnsafeGetRefValue4(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue4>();
        }

        exists = true;
        return ref _dense.GetRefValue5(index);
    }
    
    
    public void GetValues1OrDefault(ReadOnlySpan<Handle> handles, Span<TValue1> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue2(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues2OrDefault(ReadOnlySpan<Handle> handles, Span<TValue2> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue3(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues3OrDefault(ReadOnlySpan<Handle> handles, Span<TValue3> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue4(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues4OrDefault(ReadOnlySpan<Handle> handles, Span<TValue4> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue5(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }

    public int Length => _dense.Length;

    public ReadOnlySpan<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
}

public class MultiMap<TValue1, TValue2, TValue3, TValue4, TValue5>
{
    private FastListStruct<int> _sparse;
    private MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5> _dense;
    private const int TombStone = int.MaxValue;  

    public MultiMap()
    {
        _sparse = new FastListStruct<int>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5>();
    }

    public void Set(Handle handle, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5)
    {
        if (handle.IsNull())
        {
            return;
        }

        int sparseIndex = handle;

        if (sparseIndex > _sparse.LastIndex)
        {
            _sparse.ResizeFill(sparseIndex + 1, TombStone);
        }

        ref int denseIndex = ref _sparse[sparseIndex];

        // nonexistent
        if (denseIndex == TombStone)
        {
            denseIndex = _dense.Add(handle, value1, value2, value3, value4, value5);
            return;
        }

        _dense.Set(denseIndex, handle, value1, value2, value3, value4, value5);
    }

    public bool TryGet(Handle handle, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out value1, out value2, out value3, out value4, out value5);
        
            return true;
        }

        
        value1 = default;
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;

        return false;
    }
    
    
    public bool TryGet(Handle handle, out (TValue1, TValue2, TValue3, TValue4, TValue5) tuple)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5);
            tuple = (value1, value2, value3, value4, value5);

            return true;
        }

        tuple = default;

        return false;
    }
    

    public bool Remove(Handle handle)
    {
        int index = handle;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref int denseIndex = ref _sparse[index];

        if (denseIndex == TombStone)
        {
            return false;
        }
        
        if (_dense.SwapRemoveReturnFirst(denseIndex, out Handle swappedHandle))
        {
            ref int swappedSparseIndex = ref _sparse[(int)swappedHandle];
            swappedSparseIndex = denseIndex;
        }

        denseIndex = TombStone;
        return true;
    }

    public bool Contains(Handle handle)
    {
        return Contains(handle, out _);
    }
    
    public bool Contains(Handle handle, out int handleIndex) 
    {
        int index = handle;
        if (index > _sparse.LastIndex)
        {
            handleIndex = default;
            return false;
        }
        
        handleIndex = _sparse[index];

        return handleIndex != TombStone;
    }
    
    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue2(index, out value);
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue3(index, out value);
    }
    public bool TryGetValue3(Handle handle, out TValue3 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue4(index, out value);
    }
    public bool TryGetValue4(Handle handle, out TValue4 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue5(index, out value);
    }
    public bool TryGetValue5(Handle handle, out TValue5 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue6(index, out value);
    }

    
    public ref TValue1 UnsafeGetRefValue1(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue1>();
        }

        exists = true;
        return ref _dense.GetRefValue2(index);
    }
    public ref TValue2 UnsafeGetRefValue2(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue2>();
        }

        exists = true;
        return ref _dense.GetRefValue3(index);
    }
    public ref TValue3 UnsafeGetRefValue3(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue3>();
        }

        exists = true;
        return ref _dense.GetRefValue4(index);
    }
    public ref TValue4 UnsafeGetRefValue4(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue4>();
        }

        exists = true;
        return ref _dense.GetRefValue5(index);
    }
    public ref TValue5 UnsafeGetRefValue5(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue5>();
        }

        exists = true;
        return ref _dense.GetRefValue6(index);
    }
    
    
    public void GetValues1OrDefault(ReadOnlySpan<Handle> handles, Span<TValue1> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue2(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues2OrDefault(ReadOnlySpan<Handle> handles, Span<TValue2> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue3(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues3OrDefault(ReadOnlySpan<Handle> handles, Span<TValue3> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue4(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues4OrDefault(ReadOnlySpan<Handle> handles, Span<TValue4> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue5(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues5OrDefault(ReadOnlySpan<Handle> handles, Span<TValue5> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue6(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }

    public int Length => _dense.Length;

    public ReadOnlySpan<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
}

public class MultiMap<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6>
{
    private FastListStruct<int> _sparse;
    private MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6> _dense;
    private const int TombStone = int.MaxValue;  

    public MultiMap()
    {
        _sparse = new FastListStruct<int>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6>();
    }

    public void Set(Handle handle, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6)
    {
        if (handle.IsNull())
        {
            return;
        }

        int sparseIndex = handle;

        if (sparseIndex > _sparse.LastIndex)
        {
            _sparse.ResizeFill(sparseIndex + 1, TombStone);
        }

        ref int denseIndex = ref _sparse[sparseIndex];

        // nonexistent
        if (denseIndex == TombStone)
        {
            denseIndex = _dense.Add(handle, value1, value2, value3, value4, value5, value6);
            return;
        }

        _dense.Set(denseIndex, handle, value1, value2, value3, value4, value5, value6);
    }

    public bool TryGet(Handle handle, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out value1, out value2, out value3, out value4, out value5, out value6);
        
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
    
    
    public bool TryGet(Handle handle, out (TValue1, TValue2, TValue3, TValue4, TValue5, TValue6) tuple)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6);
            tuple = (value1, value2, value3, value4, value5, value6);

            return true;
        }

        tuple = default;

        return false;
    }
    

    public bool Remove(Handle handle)
    {
        int index = handle;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref int denseIndex = ref _sparse[index];

        if (denseIndex == TombStone)
        {
            return false;
        }
        
        if (_dense.SwapRemoveReturnFirst(denseIndex, out Handle swappedHandle))
        {
            ref int swappedSparseIndex = ref _sparse[(int)swappedHandle];
            swappedSparseIndex = denseIndex;
        }

        denseIndex = TombStone;
        return true;
    }

    public bool Contains(Handle handle)
    {
        return Contains(handle, out _);
    }
    
    public bool Contains(Handle handle, out int handleIndex) 
    {
        int index = handle;
        if (index > _sparse.LastIndex)
        {
            handleIndex = default;
            return false;
        }
        
        handleIndex = _sparse[index];

        return handleIndex != TombStone;
    }
    
    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue2(index, out value);
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue3(index, out value);
    }
    public bool TryGetValue3(Handle handle, out TValue3 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue4(index, out value);
    }
    public bool TryGetValue4(Handle handle, out TValue4 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue5(index, out value);
    }
    public bool TryGetValue5(Handle handle, out TValue5 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue6(index, out value);
    }
    public bool TryGetValue6(Handle handle, out TValue6 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue7(index, out value);
    }

    
    public ref TValue1 UnsafeGetRefValue1(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue1>();
        }

        exists = true;
        return ref _dense.GetRefValue2(index);
    }
    public ref TValue2 UnsafeGetRefValue2(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue2>();
        }

        exists = true;
        return ref _dense.GetRefValue3(index);
    }
    public ref TValue3 UnsafeGetRefValue3(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue3>();
        }

        exists = true;
        return ref _dense.GetRefValue4(index);
    }
    public ref TValue4 UnsafeGetRefValue4(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue4>();
        }

        exists = true;
        return ref _dense.GetRefValue5(index);
    }
    public ref TValue5 UnsafeGetRefValue5(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue5>();
        }

        exists = true;
        return ref _dense.GetRefValue6(index);
    }
    public ref TValue6 UnsafeGetRefValue6(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue6>();
        }

        exists = true;
        return ref _dense.GetRefValue7(index);
    }
    
    
    public void GetValues1OrDefault(ReadOnlySpan<Handle> handles, Span<TValue1> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue2(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues2OrDefault(ReadOnlySpan<Handle> handles, Span<TValue2> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue3(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues3OrDefault(ReadOnlySpan<Handle> handles, Span<TValue3> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue4(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues4OrDefault(ReadOnlySpan<Handle> handles, Span<TValue4> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue5(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues5OrDefault(ReadOnlySpan<Handle> handles, Span<TValue5> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue6(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues6OrDefault(ReadOnlySpan<Handle> handles, Span<TValue6> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue7(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }

    public int Length => _dense.Length;

    public ReadOnlySpan<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
    public Span<TValue6> Values6 => _dense.Values7;
}

public class MultiMap<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7>
{
    private FastListStruct<int> _sparse;
    private MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7> _dense;
    private const int TombStone = int.MaxValue;  

    public MultiMap()
    {
        _sparse = new FastListStruct<int>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7>();
    }

    public void Set(Handle handle, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7)
    {
        if (handle.IsNull())
        {
            return;
        }

        int sparseIndex = handle;

        if (sparseIndex > _sparse.LastIndex)
        {
            _sparse.ResizeFill(sparseIndex + 1, TombStone);
        }

        ref int denseIndex = ref _sparse[sparseIndex];

        // nonexistent
        if (denseIndex == TombStone)
        {
            denseIndex = _dense.Add(handle, value1, value2, value3, value4, value5, value6, value7);
            return;
        }

        _dense.Set(denseIndex, handle, value1, value2, value3, value4, value5, value6, value7);
    }

    public bool TryGet(Handle handle, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6, out TValue7 value7)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out value1, out value2, out value3, out value4, out value5, out value6, out value7);
        
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
    
    
    public bool TryGet(Handle handle, out (TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7) tuple)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6, out TValue7 value7);
            tuple = (value1, value2, value3, value4, value5, value6, value7);

            return true;
        }

        tuple = default;

        return false;
    }
    

    public bool Remove(Handle handle)
    {
        int index = handle;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref int denseIndex = ref _sparse[index];

        if (denseIndex == TombStone)
        {
            return false;
        }
        
        if (_dense.SwapRemoveReturnFirst(denseIndex, out Handle swappedHandle))
        {
            ref int swappedSparseIndex = ref _sparse[(int)swappedHandle];
            swappedSparseIndex = denseIndex;
        }

        denseIndex = TombStone;
        return true;
    }

    public bool Contains(Handle handle)
    {
        return Contains(handle, out _);
    }
    
    public bool Contains(Handle handle, out int handleIndex) 
    {
        int index = handle;
        if (index > _sparse.LastIndex)
        {
            handleIndex = default;
            return false;
        }
        
        handleIndex = _sparse[index];

        return handleIndex != TombStone;
    }
    
    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue2(index, out value);
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue3(index, out value);
    }
    public bool TryGetValue3(Handle handle, out TValue3 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue4(index, out value);
    }
    public bool TryGetValue4(Handle handle, out TValue4 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue5(index, out value);
    }
    public bool TryGetValue5(Handle handle, out TValue5 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue6(index, out value);
    }
    public bool TryGetValue6(Handle handle, out TValue6 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue7(index, out value);
    }
    public bool TryGetValue7(Handle handle, out TValue7 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue8(index, out value);
    }

    
    public ref TValue1 UnsafeGetRefValue1(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue1>();
        }

        exists = true;
        return ref _dense.GetRefValue2(index);
    }
    public ref TValue2 UnsafeGetRefValue2(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue2>();
        }

        exists = true;
        return ref _dense.GetRefValue3(index);
    }
    public ref TValue3 UnsafeGetRefValue3(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue3>();
        }

        exists = true;
        return ref _dense.GetRefValue4(index);
    }
    public ref TValue4 UnsafeGetRefValue4(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue4>();
        }

        exists = true;
        return ref _dense.GetRefValue5(index);
    }
    public ref TValue5 UnsafeGetRefValue5(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue5>();
        }

        exists = true;
        return ref _dense.GetRefValue6(index);
    }
    public ref TValue6 UnsafeGetRefValue6(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue6>();
        }

        exists = true;
        return ref _dense.GetRefValue7(index);
    }
    public ref TValue7 UnsafeGetRefValue7(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue7>();
        }

        exists = true;
        return ref _dense.GetRefValue8(index);
    }
    
    
    public void GetValues1OrDefault(ReadOnlySpan<Handle> handles, Span<TValue1> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue2(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues2OrDefault(ReadOnlySpan<Handle> handles, Span<TValue2> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue3(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues3OrDefault(ReadOnlySpan<Handle> handles, Span<TValue3> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue4(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues4OrDefault(ReadOnlySpan<Handle> handles, Span<TValue4> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue5(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues5OrDefault(ReadOnlySpan<Handle> handles, Span<TValue5> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue6(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues6OrDefault(ReadOnlySpan<Handle> handles, Span<TValue6> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue7(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues7OrDefault(ReadOnlySpan<Handle> handles, Span<TValue7> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue8(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }

    public int Length => _dense.Length;

    public ReadOnlySpan<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
    public Span<TValue6> Values6 => _dense.Values7;
    public Span<TValue7> Values7 => _dense.Values8;
}

public class MultiMap<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>
{
    private FastListStruct<int> _sparse;
    private MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8> _dense;
    private const int TombStone = int.MaxValue;  

    public MultiMap()
    {
        _sparse = new FastListStruct<int>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>();
    }

    public void Set(Handle handle, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8)
    {
        if (handle.IsNull())
        {
            return;
        }

        int sparseIndex = handle;

        if (sparseIndex > _sparse.LastIndex)
        {
            _sparse.ResizeFill(sparseIndex + 1, TombStone);
        }

        ref int denseIndex = ref _sparse[sparseIndex];

        // nonexistent
        if (denseIndex == TombStone)
        {
            denseIndex = _dense.Add(handle, value1, value2, value3, value4, value5, value6, value7, value8);
            return;
        }

        _dense.Set(denseIndex, handle, value1, value2, value3, value4, value5, value6, value7, value8);
    }

    public bool TryGet(Handle handle, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6, out TValue7 value7, out TValue8 value8)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out value1, out value2, out value3, out value4, out value5, out value6, out value7, out value8);
        
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
    
    
    public bool TryGet(Handle handle, out (TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8) tuple)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6, out TValue7 value7, out TValue8 value8);
            tuple = (value1, value2, value3, value4, value5, value6, value7, value8);

            return true;
        }

        tuple = default;

        return false;
    }
    

    public bool Remove(Handle handle)
    {
        int index = handle;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref int denseIndex = ref _sparse[index];

        if (denseIndex == TombStone)
        {
            return false;
        }
        
        if (_dense.SwapRemoveReturnFirst(denseIndex, out Handle swappedHandle))
        {
            ref int swappedSparseIndex = ref _sparse[(int)swappedHandle];
            swappedSparseIndex = denseIndex;
        }

        denseIndex = TombStone;
        return true;
    }

    public bool Contains(Handle handle)
    {
        return Contains(handle, out _);
    }
    
    public bool Contains(Handle handle, out int handleIndex) 
    {
        int index = handle;
        if (index > _sparse.LastIndex)
        {
            handleIndex = default;
            return false;
        }
        
        handleIndex = _sparse[index];

        return handleIndex != TombStone;
    }
    
    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue2(index, out value);
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue3(index, out value);
    }
    public bool TryGetValue3(Handle handle, out TValue3 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue4(index, out value);
    }
    public bool TryGetValue4(Handle handle, out TValue4 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue5(index, out value);
    }
    public bool TryGetValue5(Handle handle, out TValue5 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue6(index, out value);
    }
    public bool TryGetValue6(Handle handle, out TValue6 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue7(index, out value);
    }
    public bool TryGetValue7(Handle handle, out TValue7 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue8(index, out value);
    }
    public bool TryGetValue8(Handle handle, out TValue8 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue9(index, out value);
    }

    
    public ref TValue1 UnsafeGetRefValue1(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue1>();
        }

        exists = true;
        return ref _dense.GetRefValue2(index);
    }
    public ref TValue2 UnsafeGetRefValue2(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue2>();
        }

        exists = true;
        return ref _dense.GetRefValue3(index);
    }
    public ref TValue3 UnsafeGetRefValue3(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue3>();
        }

        exists = true;
        return ref _dense.GetRefValue4(index);
    }
    public ref TValue4 UnsafeGetRefValue4(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue4>();
        }

        exists = true;
        return ref _dense.GetRefValue5(index);
    }
    public ref TValue5 UnsafeGetRefValue5(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue5>();
        }

        exists = true;
        return ref _dense.GetRefValue6(index);
    }
    public ref TValue6 UnsafeGetRefValue6(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue6>();
        }

        exists = true;
        return ref _dense.GetRefValue7(index);
    }
    public ref TValue7 UnsafeGetRefValue7(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue7>();
        }

        exists = true;
        return ref _dense.GetRefValue8(index);
    }
    public ref TValue8 UnsafeGetRefValue8(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue8>();
        }

        exists = true;
        return ref _dense.GetRefValue9(index);
    }
    
    
    public void GetValues1OrDefault(ReadOnlySpan<Handle> handles, Span<TValue1> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue2(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues2OrDefault(ReadOnlySpan<Handle> handles, Span<TValue2> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue3(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues3OrDefault(ReadOnlySpan<Handle> handles, Span<TValue3> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue4(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues4OrDefault(ReadOnlySpan<Handle> handles, Span<TValue4> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue5(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues5OrDefault(ReadOnlySpan<Handle> handles, Span<TValue5> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue6(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues6OrDefault(ReadOnlySpan<Handle> handles, Span<TValue6> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue7(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues7OrDefault(ReadOnlySpan<Handle> handles, Span<TValue7> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue8(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues8OrDefault(ReadOnlySpan<Handle> handles, Span<TValue8> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue9(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }

    public int Length => _dense.Length;

    public ReadOnlySpan<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
    public Span<TValue6> Values6 => _dense.Values7;
    public Span<TValue7> Values7 => _dense.Values8;
    public Span<TValue8> Values8 => _dense.Values9;
}


public struct MultiMapStruct<TValue1>
{
    private FastListStruct<int> _sparse;
    private MultiArrayStruct<Handle, TValue1> _dense;
    private const int TombStone = int.MaxValue;  

    public MultiMapStruct()
    {
        _sparse = new FastListStruct<int>();
        _dense = new MultiArrayStruct<Handle, TValue1>();
    }

    public void Set(Handle handle, TValue1 value1)
    {
        if (handle.IsNull())
        {
            return;
        }

        int sparseIndex = handle;

        if (sparseIndex > _sparse.LastIndex)
        {
            _sparse.ResizeFill(sparseIndex + 1, TombStone);
        }

        ref int denseIndex = ref _sparse[sparseIndex];

        // nonexistent
        if (denseIndex == TombStone)
        {
            denseIndex = _dense.Add(handle, value1);
            return;
        }

        _dense.Set(denseIndex, handle, value1);
    }

    public bool TryGet(Handle handle, out TValue1 value1)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out value1);
        
            return true;
        }

        
        value1 = default;

        return false;
    }
    
    

    public bool Remove(Handle handle)
    {
        int index = handle;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref int denseIndex = ref _sparse[index];

        if (denseIndex == TombStone)
        {
            return false;
        }
        
        if (_dense.SwapRemoveReturnFirst(denseIndex, out Handle swappedHandle))
        {
            ref int swappedSparseIndex = ref _sparse[(int)swappedHandle];
            swappedSparseIndex = denseIndex;
        }

        denseIndex = TombStone;
        return true;
    }

    public bool Contains(Handle handle)
    {
        return Contains(handle, out _);
    }
    
    public bool Contains(Handle handle, out int handleIndex) 
    {
        int index = handle;
        if (index > _sparse.LastIndex)
        {
            handleIndex = default;
            return false;
        }
        
        handleIndex = _sparse[index];

        return handleIndex != TombStone;
    }
    
    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue2(index, out value);
    }

    
    public ref TValue1 UnsafeGetRefValue1(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue1>();
        }

        exists = true;
        return ref _dense.GetRefValue2(index);
    }
    
    
    public void GetValues1OrDefault(ReadOnlySpan<Handle> handles, Span<TValue1> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue2(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }

    public int Length => _dense.Length;

    public ReadOnlySpan<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
}

public struct MultiMapStruct<TValue1, TValue2>
{
    private FastListStruct<int> _sparse;
    private MultiArrayStruct<Handle, TValue1, TValue2> _dense;
    private const int TombStone = int.MaxValue;  

    public MultiMapStruct()
    {
        _sparse = new FastListStruct<int>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2>();
    }

    public void Set(Handle handle, TValue1 value1, TValue2 value2)
    {
        if (handle.IsNull())
        {
            return;
        }

        int sparseIndex = handle;

        if (sparseIndex > _sparse.LastIndex)
        {
            _sparse.ResizeFill(sparseIndex + 1, TombStone);
        }

        ref int denseIndex = ref _sparse[sparseIndex];

        // nonexistent
        if (denseIndex == TombStone)
        {
            denseIndex = _dense.Add(handle, value1, value2);
            return;
        }

        _dense.Set(denseIndex, handle, value1, value2);
    }

    public bool TryGet(Handle handle, out TValue1 value1, out TValue2 value2)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out value1, out value2);
        
            return true;
        }

        
        value1 = default;
        value2 = default;

        return false;
    }
    
    
    public bool TryGet(Handle handle, out (TValue1, TValue2) tuple)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out TValue1 value1, out TValue2 value2);
            tuple = (value1, value2);

            return true;
        }

        tuple = default;

        return false;
    }
    

    public bool Remove(Handle handle)
    {
        int index = handle;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref int denseIndex = ref _sparse[index];

        if (denseIndex == TombStone)
        {
            return false;
        }
        
        if (_dense.SwapRemoveReturnFirst(denseIndex, out Handle swappedHandle))
        {
            ref int swappedSparseIndex = ref _sparse[(int)swappedHandle];
            swappedSparseIndex = denseIndex;
        }

        denseIndex = TombStone;
        return true;
    }

    public bool Contains(Handle handle)
    {
        return Contains(handle, out _);
    }
    
    public bool Contains(Handle handle, out int handleIndex) 
    {
        int index = handle;
        if (index > _sparse.LastIndex)
        {
            handleIndex = default;
            return false;
        }
        
        handleIndex = _sparse[index];

        return handleIndex != TombStone;
    }
    
    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue2(index, out value);
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue3(index, out value);
    }

    
    public ref TValue1 UnsafeGetRefValue1(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue1>();
        }

        exists = true;
        return ref _dense.GetRefValue2(index);
    }
    public ref TValue2 UnsafeGetRefValue2(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue2>();
        }

        exists = true;
        return ref _dense.GetRefValue3(index);
    }
    
    
    public void GetValues1OrDefault(ReadOnlySpan<Handle> handles, Span<TValue1> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue2(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues2OrDefault(ReadOnlySpan<Handle> handles, Span<TValue2> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue3(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }

    public int Length => _dense.Length;

    public ReadOnlySpan<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
}

public struct MultiMapStruct<TValue1, TValue2, TValue3>
{
    private FastListStruct<int> _sparse;
    private MultiArrayStruct<Handle, TValue1, TValue2, TValue3> _dense;
    private const int TombStone = int.MaxValue;  

    public MultiMapStruct()
    {
        _sparse = new FastListStruct<int>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2, TValue3>();
    }

    public void Set(Handle handle, TValue1 value1, TValue2 value2, TValue3 value3)
    {
        if (handle.IsNull())
        {
            return;
        }

        int sparseIndex = handle;

        if (sparseIndex > _sparse.LastIndex)
        {
            _sparse.ResizeFill(sparseIndex + 1, TombStone);
        }

        ref int denseIndex = ref _sparse[sparseIndex];

        // nonexistent
        if (denseIndex == TombStone)
        {
            denseIndex = _dense.Add(handle, value1, value2, value3);
            return;
        }

        _dense.Set(denseIndex, handle, value1, value2, value3);
    }

    public bool TryGet(Handle handle, out TValue1 value1, out TValue2 value2, out TValue3 value3)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out value1, out value2, out value3);
        
            return true;
        }

        
        value1 = default;
        value2 = default;
        value3 = default;

        return false;
    }
    
    
    public bool TryGet(Handle handle, out (TValue1, TValue2, TValue3) tuple)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out TValue1 value1, out TValue2 value2, out TValue3 value3);
            tuple = (value1, value2, value3);

            return true;
        }

        tuple = default;

        return false;
    }
    

    public bool Remove(Handle handle)
    {
        int index = handle;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref int denseIndex = ref _sparse[index];

        if (denseIndex == TombStone)
        {
            return false;
        }
        
        if (_dense.SwapRemoveReturnFirst(denseIndex, out Handle swappedHandle))
        {
            ref int swappedSparseIndex = ref _sparse[(int)swappedHandle];
            swappedSparseIndex = denseIndex;
        }

        denseIndex = TombStone;
        return true;
    }

    public bool Contains(Handle handle)
    {
        return Contains(handle, out _);
    }
    
    public bool Contains(Handle handle, out int handleIndex) 
    {
        int index = handle;
        if (index > _sparse.LastIndex)
        {
            handleIndex = default;
            return false;
        }
        
        handleIndex = _sparse[index];

        return handleIndex != TombStone;
    }
    
    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue2(index, out value);
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue3(index, out value);
    }
    public bool TryGetValue3(Handle handle, out TValue3 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue4(index, out value);
    }

    
    public ref TValue1 UnsafeGetRefValue1(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue1>();
        }

        exists = true;
        return ref _dense.GetRefValue2(index);
    }
    public ref TValue2 UnsafeGetRefValue2(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue2>();
        }

        exists = true;
        return ref _dense.GetRefValue3(index);
    }
    public ref TValue3 UnsafeGetRefValue3(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue3>();
        }

        exists = true;
        return ref _dense.GetRefValue4(index);
    }
    
    
    public void GetValues1OrDefault(ReadOnlySpan<Handle> handles, Span<TValue1> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue2(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues2OrDefault(ReadOnlySpan<Handle> handles, Span<TValue2> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue3(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues3OrDefault(ReadOnlySpan<Handle> handles, Span<TValue3> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue4(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }

    public int Length => _dense.Length;

    public ReadOnlySpan<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
}

public struct MultiMapStruct<TValue1, TValue2, TValue3, TValue4>
{
    private FastListStruct<int> _sparse;
    private MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4> _dense;
    private const int TombStone = int.MaxValue;  

    public MultiMapStruct()
    {
        _sparse = new FastListStruct<int>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4>();
    }

    public void Set(Handle handle, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4)
    {
        if (handle.IsNull())
        {
            return;
        }

        int sparseIndex = handle;

        if (sparseIndex > _sparse.LastIndex)
        {
            _sparse.ResizeFill(sparseIndex + 1, TombStone);
        }

        ref int denseIndex = ref _sparse[sparseIndex];

        // nonexistent
        if (denseIndex == TombStone)
        {
            denseIndex = _dense.Add(handle, value1, value2, value3, value4);
            return;
        }

        _dense.Set(denseIndex, handle, value1, value2, value3, value4);
    }

    public bool TryGet(Handle handle, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out value1, out value2, out value3, out value4);
        
            return true;
        }

        
        value1 = default;
        value2 = default;
        value3 = default;
        value4 = default;

        return false;
    }
    
    
    public bool TryGet(Handle handle, out (TValue1, TValue2, TValue3, TValue4) tuple)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4);
            tuple = (value1, value2, value3, value4);

            return true;
        }

        tuple = default;

        return false;
    }
    

    public bool Remove(Handle handle)
    {
        int index = handle;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref int denseIndex = ref _sparse[index];

        if (denseIndex == TombStone)
        {
            return false;
        }
        
        if (_dense.SwapRemoveReturnFirst(denseIndex, out Handle swappedHandle))
        {
            ref int swappedSparseIndex = ref _sparse[(int)swappedHandle];
            swappedSparseIndex = denseIndex;
        }

        denseIndex = TombStone;
        return true;
    }

    public bool Contains(Handle handle)
    {
        return Contains(handle, out _);
    }
    
    public bool Contains(Handle handle, out int handleIndex) 
    {
        int index = handle;
        if (index > _sparse.LastIndex)
        {
            handleIndex = default;
            return false;
        }
        
        handleIndex = _sparse[index];

        return handleIndex != TombStone;
    }
    
    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue2(index, out value);
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue3(index, out value);
    }
    public bool TryGetValue3(Handle handle, out TValue3 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue4(index, out value);
    }
    public bool TryGetValue4(Handle handle, out TValue4 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue5(index, out value);
    }

    
    public ref TValue1 UnsafeGetRefValue1(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue1>();
        }

        exists = true;
        return ref _dense.GetRefValue2(index);
    }
    public ref TValue2 UnsafeGetRefValue2(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue2>();
        }

        exists = true;
        return ref _dense.GetRefValue3(index);
    }
    public ref TValue3 UnsafeGetRefValue3(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue3>();
        }

        exists = true;
        return ref _dense.GetRefValue4(index);
    }
    public ref TValue4 UnsafeGetRefValue4(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue4>();
        }

        exists = true;
        return ref _dense.GetRefValue5(index);
    }
    
    
    public void GetValues1OrDefault(ReadOnlySpan<Handle> handles, Span<TValue1> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue2(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues2OrDefault(ReadOnlySpan<Handle> handles, Span<TValue2> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue3(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues3OrDefault(ReadOnlySpan<Handle> handles, Span<TValue3> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue4(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues4OrDefault(ReadOnlySpan<Handle> handles, Span<TValue4> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue5(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }

    public int Length => _dense.Length;

    public ReadOnlySpan<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
}

public struct MultiMapStruct<TValue1, TValue2, TValue3, TValue4, TValue5>
{
    private FastListStruct<int> _sparse;
    private MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5> _dense;
    private const int TombStone = int.MaxValue;  

    public MultiMapStruct()
    {
        _sparse = new FastListStruct<int>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5>();
    }

    public void Set(Handle handle, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5)
    {
        if (handle.IsNull())
        {
            return;
        }

        int sparseIndex = handle;

        if (sparseIndex > _sparse.LastIndex)
        {
            _sparse.ResizeFill(sparseIndex + 1, TombStone);
        }

        ref int denseIndex = ref _sparse[sparseIndex];

        // nonexistent
        if (denseIndex == TombStone)
        {
            denseIndex = _dense.Add(handle, value1, value2, value3, value4, value5);
            return;
        }

        _dense.Set(denseIndex, handle, value1, value2, value3, value4, value5);
    }

    public bool TryGet(Handle handle, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out value1, out value2, out value3, out value4, out value5);
        
            return true;
        }

        
        value1 = default;
        value2 = default;
        value3 = default;
        value4 = default;
        value5 = default;

        return false;
    }
    
    
    public bool TryGet(Handle handle, out (TValue1, TValue2, TValue3, TValue4, TValue5) tuple)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5);
            tuple = (value1, value2, value3, value4, value5);

            return true;
        }

        tuple = default;

        return false;
    }
    

    public bool Remove(Handle handle)
    {
        int index = handle;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref int denseIndex = ref _sparse[index];

        if (denseIndex == TombStone)
        {
            return false;
        }
        
        if (_dense.SwapRemoveReturnFirst(denseIndex, out Handle swappedHandle))
        {
            ref int swappedSparseIndex = ref _sparse[(int)swappedHandle];
            swappedSparseIndex = denseIndex;
        }

        denseIndex = TombStone;
        return true;
    }

    public bool Contains(Handle handle)
    {
        return Contains(handle, out _);
    }
    
    public bool Contains(Handle handle, out int handleIndex) 
    {
        int index = handle;
        if (index > _sparse.LastIndex)
        {
            handleIndex = default;
            return false;
        }
        
        handleIndex = _sparse[index];

        return handleIndex != TombStone;
    }
    
    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue2(index, out value);
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue3(index, out value);
    }
    public bool TryGetValue3(Handle handle, out TValue3 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue4(index, out value);
    }
    public bool TryGetValue4(Handle handle, out TValue4 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue5(index, out value);
    }
    public bool TryGetValue5(Handle handle, out TValue5 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue6(index, out value);
    }

    
    public ref TValue1 UnsafeGetRefValue1(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue1>();
        }

        exists = true;
        return ref _dense.GetRefValue2(index);
    }
    public ref TValue2 UnsafeGetRefValue2(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue2>();
        }

        exists = true;
        return ref _dense.GetRefValue3(index);
    }
    public ref TValue3 UnsafeGetRefValue3(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue3>();
        }

        exists = true;
        return ref _dense.GetRefValue4(index);
    }
    public ref TValue4 UnsafeGetRefValue4(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue4>();
        }

        exists = true;
        return ref _dense.GetRefValue5(index);
    }
    public ref TValue5 UnsafeGetRefValue5(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue5>();
        }

        exists = true;
        return ref _dense.GetRefValue6(index);
    }
    
    
    public void GetValues1OrDefault(ReadOnlySpan<Handle> handles, Span<TValue1> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue2(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues2OrDefault(ReadOnlySpan<Handle> handles, Span<TValue2> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue3(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues3OrDefault(ReadOnlySpan<Handle> handles, Span<TValue3> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue4(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues4OrDefault(ReadOnlySpan<Handle> handles, Span<TValue4> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue5(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues5OrDefault(ReadOnlySpan<Handle> handles, Span<TValue5> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue6(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }

    public int Length => _dense.Length;

    public ReadOnlySpan<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
}

public struct MultiMapStruct<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6>
{
    private FastListStruct<int> _sparse;
    private MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6> _dense;
    private const int TombStone = int.MaxValue;  

    public MultiMapStruct()
    {
        _sparse = new FastListStruct<int>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6>();
    }

    public void Set(Handle handle, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6)
    {
        if (handle.IsNull())
        {
            return;
        }

        int sparseIndex = handle;

        if (sparseIndex > _sparse.LastIndex)
        {
            _sparse.ResizeFill(sparseIndex + 1, TombStone);
        }

        ref int denseIndex = ref _sparse[sparseIndex];

        // nonexistent
        if (denseIndex == TombStone)
        {
            denseIndex = _dense.Add(handle, value1, value2, value3, value4, value5, value6);
            return;
        }

        _dense.Set(denseIndex, handle, value1, value2, value3, value4, value5, value6);
    }

    public bool TryGet(Handle handle, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out value1, out value2, out value3, out value4, out value5, out value6);
        
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
    
    
    public bool TryGet(Handle handle, out (TValue1, TValue2, TValue3, TValue4, TValue5, TValue6) tuple)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6);
            tuple = (value1, value2, value3, value4, value5, value6);

            return true;
        }

        tuple = default;

        return false;
    }
    

    public bool Remove(Handle handle)
    {
        int index = handle;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref int denseIndex = ref _sparse[index];

        if (denseIndex == TombStone)
        {
            return false;
        }
        
        if (_dense.SwapRemoveReturnFirst(denseIndex, out Handle swappedHandle))
        {
            ref int swappedSparseIndex = ref _sparse[(int)swappedHandle];
            swappedSparseIndex = denseIndex;
        }

        denseIndex = TombStone;
        return true;
    }

    public bool Contains(Handle handle)
    {
        return Contains(handle, out _);
    }
    
    public bool Contains(Handle handle, out int handleIndex) 
    {
        int index = handle;
        if (index > _sparse.LastIndex)
        {
            handleIndex = default;
            return false;
        }
        
        handleIndex = _sparse[index];

        return handleIndex != TombStone;
    }
    
    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue2(index, out value);
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue3(index, out value);
    }
    public bool TryGetValue3(Handle handle, out TValue3 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue4(index, out value);
    }
    public bool TryGetValue4(Handle handle, out TValue4 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue5(index, out value);
    }
    public bool TryGetValue5(Handle handle, out TValue5 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue6(index, out value);
    }
    public bool TryGetValue6(Handle handle, out TValue6 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue7(index, out value);
    }

    
    public ref TValue1 UnsafeGetRefValue1(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue1>();
        }

        exists = true;
        return ref _dense.GetRefValue2(index);
    }
    public ref TValue2 UnsafeGetRefValue2(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue2>();
        }

        exists = true;
        return ref _dense.GetRefValue3(index);
    }
    public ref TValue3 UnsafeGetRefValue3(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue3>();
        }

        exists = true;
        return ref _dense.GetRefValue4(index);
    }
    public ref TValue4 UnsafeGetRefValue4(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue4>();
        }

        exists = true;
        return ref _dense.GetRefValue5(index);
    }
    public ref TValue5 UnsafeGetRefValue5(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue5>();
        }

        exists = true;
        return ref _dense.GetRefValue6(index);
    }
    public ref TValue6 UnsafeGetRefValue6(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue6>();
        }

        exists = true;
        return ref _dense.GetRefValue7(index);
    }
    
    
    public void GetValues1OrDefault(ReadOnlySpan<Handle> handles, Span<TValue1> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue2(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues2OrDefault(ReadOnlySpan<Handle> handles, Span<TValue2> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue3(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues3OrDefault(ReadOnlySpan<Handle> handles, Span<TValue3> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue4(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues4OrDefault(ReadOnlySpan<Handle> handles, Span<TValue4> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue5(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues5OrDefault(ReadOnlySpan<Handle> handles, Span<TValue5> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue6(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues6OrDefault(ReadOnlySpan<Handle> handles, Span<TValue6> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue7(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }

    public int Length => _dense.Length;

    public ReadOnlySpan<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
    public Span<TValue6> Values6 => _dense.Values7;
}

public struct MultiMapStruct<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7>
{
    private FastListStruct<int> _sparse;
    private MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7> _dense;
    private const int TombStone = int.MaxValue;  

    public MultiMapStruct()
    {
        _sparse = new FastListStruct<int>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7>();
    }

    public void Set(Handle handle, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7)
    {
        if (handle.IsNull())
        {
            return;
        }

        int sparseIndex = handle;

        if (sparseIndex > _sparse.LastIndex)
        {
            _sparse.ResizeFill(sparseIndex + 1, TombStone);
        }

        ref int denseIndex = ref _sparse[sparseIndex];

        // nonexistent
        if (denseIndex == TombStone)
        {
            denseIndex = _dense.Add(handle, value1, value2, value3, value4, value5, value6, value7);
            return;
        }

        _dense.Set(denseIndex, handle, value1, value2, value3, value4, value5, value6, value7);
    }

    public bool TryGet(Handle handle, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6, out TValue7 value7)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out value1, out value2, out value3, out value4, out value5, out value6, out value7);
        
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
    
    
    public bool TryGet(Handle handle, out (TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7) tuple)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6, out TValue7 value7);
            tuple = (value1, value2, value3, value4, value5, value6, value7);

            return true;
        }

        tuple = default;

        return false;
    }
    

    public bool Remove(Handle handle)
    {
        int index = handle;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref int denseIndex = ref _sparse[index];

        if (denseIndex == TombStone)
        {
            return false;
        }
        
        if (_dense.SwapRemoveReturnFirst(denseIndex, out Handle swappedHandle))
        {
            ref int swappedSparseIndex = ref _sparse[(int)swappedHandle];
            swappedSparseIndex = denseIndex;
        }

        denseIndex = TombStone;
        return true;
    }

    public bool Contains(Handle handle)
    {
        return Contains(handle, out _);
    }
    
    public bool Contains(Handle handle, out int handleIndex) 
    {
        int index = handle;
        if (index > _sparse.LastIndex)
        {
            handleIndex = default;
            return false;
        }
        
        handleIndex = _sparse[index];

        return handleIndex != TombStone;
    }
    
    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue2(index, out value);
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue3(index, out value);
    }
    public bool TryGetValue3(Handle handle, out TValue3 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue4(index, out value);
    }
    public bool TryGetValue4(Handle handle, out TValue4 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue5(index, out value);
    }
    public bool TryGetValue5(Handle handle, out TValue5 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue6(index, out value);
    }
    public bool TryGetValue6(Handle handle, out TValue6 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue7(index, out value);
    }
    public bool TryGetValue7(Handle handle, out TValue7 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue8(index, out value);
    }

    
    public ref TValue1 UnsafeGetRefValue1(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue1>();
        }

        exists = true;
        return ref _dense.GetRefValue2(index);
    }
    public ref TValue2 UnsafeGetRefValue2(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue2>();
        }

        exists = true;
        return ref _dense.GetRefValue3(index);
    }
    public ref TValue3 UnsafeGetRefValue3(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue3>();
        }

        exists = true;
        return ref _dense.GetRefValue4(index);
    }
    public ref TValue4 UnsafeGetRefValue4(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue4>();
        }

        exists = true;
        return ref _dense.GetRefValue5(index);
    }
    public ref TValue5 UnsafeGetRefValue5(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue5>();
        }

        exists = true;
        return ref _dense.GetRefValue6(index);
    }
    public ref TValue6 UnsafeGetRefValue6(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue6>();
        }

        exists = true;
        return ref _dense.GetRefValue7(index);
    }
    public ref TValue7 UnsafeGetRefValue7(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue7>();
        }

        exists = true;
        return ref _dense.GetRefValue8(index);
    }
    
    
    public void GetValues1OrDefault(ReadOnlySpan<Handle> handles, Span<TValue1> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue2(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues2OrDefault(ReadOnlySpan<Handle> handles, Span<TValue2> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue3(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues3OrDefault(ReadOnlySpan<Handle> handles, Span<TValue3> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue4(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues4OrDefault(ReadOnlySpan<Handle> handles, Span<TValue4> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue5(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues5OrDefault(ReadOnlySpan<Handle> handles, Span<TValue5> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue6(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues6OrDefault(ReadOnlySpan<Handle> handles, Span<TValue6> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue7(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues7OrDefault(ReadOnlySpan<Handle> handles, Span<TValue7> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue8(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }

    public int Length => _dense.Length;

    public ReadOnlySpan<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
    public Span<TValue6> Values6 => _dense.Values7;
    public Span<TValue7> Values7 => _dense.Values8;
}

public struct MultiMapStruct<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>
{
    private FastListStruct<int> _sparse;
    private MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8> _dense;
    private const int TombStone = int.MaxValue;  

    public MultiMapStruct()
    {
        _sparse = new FastListStruct<int>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>();
    }

    public void Set(Handle handle, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8)
    {
        if (handle.IsNull())
        {
            return;
        }

        int sparseIndex = handle;

        if (sparseIndex > _sparse.LastIndex)
        {
            _sparse.ResizeFill(sparseIndex + 1, TombStone);
        }

        ref int denseIndex = ref _sparse[sparseIndex];

        // nonexistent
        if (denseIndex == TombStone)
        {
            denseIndex = _dense.Add(handle, value1, value2, value3, value4, value5, value6, value7, value8);
            return;
        }

        _dense.Set(denseIndex, handle, value1, value2, value3, value4, value5, value6, value7, value8);
    }

    public bool TryGet(Handle handle, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6, out TValue7 value7, out TValue8 value8)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out value1, out value2, out value3, out value4, out value5, out value6, out value7, out value8);
        
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
    
    
    public bool TryGet(Handle handle, out (TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8) tuple)
    {
        if (Contains(handle, out int index))
        {
            _dense.TryGetButFirst(index, out TValue1 value1, out TValue2 value2, out TValue3 value3, out TValue4 value4, out TValue5 value5, out TValue6 value6, out TValue7 value7, out TValue8 value8);
            tuple = (value1, value2, value3, value4, value5, value6, value7, value8);

            return true;
        }

        tuple = default;

        return false;
    }
    

    public bool Remove(Handle handle)
    {
        int index = handle;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref int denseIndex = ref _sparse[index];

        if (denseIndex == TombStone)
        {
            return false;
        }
        
        if (_dense.SwapRemoveReturnFirst(denseIndex, out Handle swappedHandle))
        {
            ref int swappedSparseIndex = ref _sparse[(int)swappedHandle];
            swappedSparseIndex = denseIndex;
        }

        denseIndex = TombStone;
        return true;
    }

    public bool Contains(Handle handle)
    {
        return Contains(handle, out _);
    }
    
    public bool Contains(Handle handle, out int handleIndex) 
    {
        int index = handle;
        if (index > _sparse.LastIndex)
        {
            handleIndex = default;
            return false;
        }
        
        handleIndex = _sparse[index];

        return handleIndex != TombStone;
    }
    
    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue2(index, out value);
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue3(index, out value);
    }
    public bool TryGetValue3(Handle handle, out TValue3 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue4(index, out value);
    }
    public bool TryGetValue4(Handle handle, out TValue4 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue5(index, out value);
    }
    public bool TryGetValue5(Handle handle, out TValue5 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue6(index, out value);
    }
    public bool TryGetValue6(Handle handle, out TValue6 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue7(index, out value);
    }
    public bool TryGetValue7(Handle handle, out TValue7 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue8(index, out value);
    }
    public bool TryGetValue8(Handle handle, out TValue8 value)
    {
        if (!Contains(handle, out int index))
        {
            value = default;
            return false;
        }
        
        return _dense.TryGetValue9(index, out value);
    }

    
    public ref TValue1 UnsafeGetRefValue1(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue1>();
        }

        exists = true;
        return ref _dense.GetRefValue2(index);
    }
    public ref TValue2 UnsafeGetRefValue2(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue2>();
        }

        exists = true;
        return ref _dense.GetRefValue3(index);
    }
    public ref TValue3 UnsafeGetRefValue3(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue3>();
        }

        exists = true;
        return ref _dense.GetRefValue4(index);
    }
    public ref TValue4 UnsafeGetRefValue4(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue4>();
        }

        exists = true;
        return ref _dense.GetRefValue5(index);
    }
    public ref TValue5 UnsafeGetRefValue5(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue5>();
        }

        exists = true;
        return ref _dense.GetRefValue6(index);
    }
    public ref TValue6 UnsafeGetRefValue6(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue6>();
        }

        exists = true;
        return ref _dense.GetRefValue7(index);
    }
    public ref TValue7 UnsafeGetRefValue7(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue7>();
        }

        exists = true;
        return ref _dense.GetRefValue8(index);
    }
    public ref TValue8 UnsafeGetRefValue8(Handle handle, out bool exists)
    {
        if (!Contains(handle, out int index))
        {
            exists = false;
            return ref Unsafe.NullRef<TValue8>();
        }

        exists = true;
        return ref _dense.GetRefValue9(index);
    }
    
    
    public void GetValues1OrDefault(ReadOnlySpan<Handle> handles, Span<TValue1> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue2(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues2OrDefault(ReadOnlySpan<Handle> handles, Span<TValue2> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue3(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues3OrDefault(ReadOnlySpan<Handle> handles, Span<TValue3> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue4(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues4OrDefault(ReadOnlySpan<Handle> handles, Span<TValue4> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue5(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues5OrDefault(ReadOnlySpan<Handle> handles, Span<TValue5> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue6(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues6OrDefault(ReadOnlySpan<Handle> handles, Span<TValue6> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue7(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues7OrDefault(ReadOnlySpan<Handle> handles, Span<TValue7> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue8(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }
    public void GetValues8OrDefault(ReadOnlySpan<Handle> handles, Span<TValue8> values)
    {
        if (handles.Length > values.Length)
        {
            throw new ArgumentException("Length of values span must be greater than or equal to length of handles span.", nameof(values));
        }
        
        for (int i = 0; i < handles.Length; i++)
        {
            if (Contains(handles[i], out int index))
            {
                values[index] = _dense.GetValue9(index);
            }
            else
            {
                values[index] = default;
            }
        }
    }

    public int Length => _dense.Length;

    public ReadOnlySpan<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
    public Span<TValue6> Values6 => _dense.Values7;
    public Span<TValue7> Values7 => _dense.Values8;
    public Span<TValue8> Values8 => _dense.Values9;
}



