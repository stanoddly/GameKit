using System.Runtime.CompilerServices;

namespace GameKit.Collections;
public class DenseSlotMap<THandle, TValue1>
    where THandle: struct, IHandle<THandle>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<THandle> _handles;
    private MultiArrayStruct<THandle, TValue1> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMap()
    {
        _handles = new FastListStruct<THandle>();
        _dense = new MultiArrayStruct<THandle, TValue1>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public THandle Add(TValue1 value1)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Length;
            THandle handle = new THandle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new THandle{ Index = handleIndex, Version = 0 }, value1);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref THandle handleToRecycle = ref _handles[handleIndex];

            bool isItTheOnlyFreeIndex = _freeIndex == _lastFreeIndex;
            if (isItTheOnlyFreeIndex)
            {
                _freeIndex = Tombstone;
                _lastFreeIndex = Tombstone;
            }
            else
            {
                _freeIndex = handleToRecycle.Index;
            }
            
            // Note the version has been incremented on removal too, so it's free to use already
            THandle finalHandle = new THandle { Index = handleIndex, Version = handleToRecycle.Version };
            uint denseIndex = (uint)_dense.Add(finalHandle, value1);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(THandle handle)
    {
        if (handle.IsNull())
        {
            return false;
        }

        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref THandle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out THandle handleThatGotMoved))
        {
            ref THandle swappedHandle = ref _handles[handleThatGotMoved.Index];
            swappedHandle = swappedHandle with { Index = handleToBeDeleted.Index };
        }

        // Increment version to invalidate existing handles
        handleToBeDeleted = handleToBeDeleted with 
        {
            // Increment version so all the previous instances are invalid and it's ready for future recycle
            Version = handle.Version + 1,
            Index = _freeIndex
        };

        // Update free list
        if (_freeIndex == Tombstone)
        {
            _lastFreeIndex = handle.Index;
        }

        _freeIndex = handle.Index;

        return true;
    }

    
    public bool TryGetValue1(THandle handle, out TValue1 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue2((int)index);
        return true;
    }
    
    
    public TValue1 GetValue1(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue2((int)index);
    }

    
    public ref TValue1 UnsafeTryGetRefValue1(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue1>();
        }
        
        value = false;
        return ref _dense.GetRefValue2((int)index);
    }

    public bool Contains(THandle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public bool Contains(THandle handle, out uint index)
    {
        if (handle.Index >= _handles.Length)
        {
            index = default;
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        index = storedHandle.Index; 
        return storedHandle.Version == handle.Version;
    }

    public int Length => _dense.Length;

    public Span<THandle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
}
public class DenseSlotMap<THandle, TValue1, TValue2>
    where THandle: struct, IHandle<THandle>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<THandle> _handles;
    private MultiArrayStruct<THandle, TValue1, TValue2> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMap()
    {
        _handles = new FastListStruct<THandle>();
        _dense = new MultiArrayStruct<THandle, TValue1, TValue2>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public THandle Add(TValue1 value1, TValue2 value2)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Length;
            THandle handle = new THandle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new THandle{ Index = handleIndex, Version = 0 }, value1, value2);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref THandle handleToRecycle = ref _handles[handleIndex];

            bool isItTheOnlyFreeIndex = _freeIndex == _lastFreeIndex;
            if (isItTheOnlyFreeIndex)
            {
                _freeIndex = Tombstone;
                _lastFreeIndex = Tombstone;
            }
            else
            {
                _freeIndex = handleToRecycle.Index;
            }
            
            // Note the version has been incremented on removal too, so it's free to use already
            THandle finalHandle = new THandle { Index = handleIndex, Version = handleToRecycle.Version };
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(THandle handle)
    {
        if (handle.IsNull())
        {
            return false;
        }

        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref THandle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out THandle handleThatGotMoved))
        {
            ref THandle swappedHandle = ref _handles[handleThatGotMoved.Index];
            swappedHandle = swappedHandle with { Index = handleToBeDeleted.Index };
        }

        // Increment version to invalidate existing handles
        handleToBeDeleted = handleToBeDeleted with 
        {
            // Increment version so all the previous instances are invalid and it's ready for future recycle
            Version = handle.Version + 1,
            Index = _freeIndex
        };

        // Update free list
        if (_freeIndex == Tombstone)
        {
            _lastFreeIndex = handle.Index;
        }

        _freeIndex = handle.Index;

        return true;
    }

    
    public bool TryGetValue1(THandle handle, out TValue1 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue2((int)index);
        return true;
    }
    public bool TryGetValue2(THandle handle, out TValue2 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue3((int)index);
        return true;
    }
    
    
    public TValue1 GetValue1(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue2((int)index);
    }
    public TValue2 GetValue2(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue3((int)index);
    }

    
    public ref TValue1 UnsafeTryGetRefValue1(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue1>();
        }
        
        value = false;
        return ref _dense.GetRefValue2((int)index);
    }
    public ref TValue2 UnsafeTryGetRefValue2(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue2>();
        }
        
        value = false;
        return ref _dense.GetRefValue3((int)index);
    }

    public bool Contains(THandle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public bool Contains(THandle handle, out uint index)
    {
        if (handle.Index >= _handles.Length)
        {
            index = default;
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        index = storedHandle.Index; 
        return storedHandle.Version == handle.Version;
    }

    public int Length => _dense.Length;

    public Span<THandle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
}
public class DenseSlotMap<THandle, TValue1, TValue2, TValue3>
    where THandle: struct, IHandle<THandle>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<THandle> _handles;
    private MultiArrayStruct<THandle, TValue1, TValue2, TValue3> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMap()
    {
        _handles = new FastListStruct<THandle>();
        _dense = new MultiArrayStruct<THandle, TValue1, TValue2, TValue3>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public THandle Add(TValue1 value1, TValue2 value2, TValue3 value3)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Length;
            THandle handle = new THandle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new THandle{ Index = handleIndex, Version = 0 }, value1, value2, value3);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref THandle handleToRecycle = ref _handles[handleIndex];

            bool isItTheOnlyFreeIndex = _freeIndex == _lastFreeIndex;
            if (isItTheOnlyFreeIndex)
            {
                _freeIndex = Tombstone;
                _lastFreeIndex = Tombstone;
            }
            else
            {
                _freeIndex = handleToRecycle.Index;
            }
            
            // Note the version has been incremented on removal too, so it's free to use already
            THandle finalHandle = new THandle { Index = handleIndex, Version = handleToRecycle.Version };
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2, value3);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(THandle handle)
    {
        if (handle.IsNull())
        {
            return false;
        }

        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref THandle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out THandle handleThatGotMoved))
        {
            ref THandle swappedHandle = ref _handles[handleThatGotMoved.Index];
            swappedHandle = swappedHandle with { Index = handleToBeDeleted.Index };
        }

        // Increment version to invalidate existing handles
        handleToBeDeleted = handleToBeDeleted with 
        {
            // Increment version so all the previous instances are invalid and it's ready for future recycle
            Version = handle.Version + 1,
            Index = _freeIndex
        };

        // Update free list
        if (_freeIndex == Tombstone)
        {
            _lastFreeIndex = handle.Index;
        }

        _freeIndex = handle.Index;

        return true;
    }

    
    public bool TryGetValue1(THandle handle, out TValue1 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue2((int)index);
        return true;
    }
    public bool TryGetValue2(THandle handle, out TValue2 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue3((int)index);
        return true;
    }
    public bool TryGetValue3(THandle handle, out TValue3 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue4((int)index);
        return true;
    }
    
    
    public TValue1 GetValue1(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue2((int)index);
    }
    public TValue2 GetValue2(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue3((int)index);
    }
    public TValue3 GetValue3(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue4((int)index);
    }

    
    public ref TValue1 UnsafeTryGetRefValue1(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue1>();
        }
        
        value = false;
        return ref _dense.GetRefValue2((int)index);
    }
    public ref TValue2 UnsafeTryGetRefValue2(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue2>();
        }
        
        value = false;
        return ref _dense.GetRefValue3((int)index);
    }
    public ref TValue3 UnsafeTryGetRefValue3(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue3>();
        }
        
        value = false;
        return ref _dense.GetRefValue4((int)index);
    }

    public bool Contains(THandle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public bool Contains(THandle handle, out uint index)
    {
        if (handle.Index >= _handles.Length)
        {
            index = default;
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        index = storedHandle.Index; 
        return storedHandle.Version == handle.Version;
    }

    public int Length => _dense.Length;

    public Span<THandle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
}
public class DenseSlotMap<THandle, TValue1, TValue2, TValue3, TValue4>
    where THandle: struct, IHandle<THandle>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<THandle> _handles;
    private MultiArrayStruct<THandle, TValue1, TValue2, TValue3, TValue4> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMap()
    {
        _handles = new FastListStruct<THandle>();
        _dense = new MultiArrayStruct<THandle, TValue1, TValue2, TValue3, TValue4>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public THandle Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Length;
            THandle handle = new THandle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new THandle{ Index = handleIndex, Version = 0 }, value1, value2, value3, value4);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref THandle handleToRecycle = ref _handles[handleIndex];

            bool isItTheOnlyFreeIndex = _freeIndex == _lastFreeIndex;
            if (isItTheOnlyFreeIndex)
            {
                _freeIndex = Tombstone;
                _lastFreeIndex = Tombstone;
            }
            else
            {
                _freeIndex = handleToRecycle.Index;
            }
            
            // Note the version has been incremented on removal too, so it's free to use already
            THandle finalHandle = new THandle { Index = handleIndex, Version = handleToRecycle.Version };
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2, value3, value4);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(THandle handle)
    {
        if (handle.IsNull())
        {
            return false;
        }

        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref THandle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out THandle handleThatGotMoved))
        {
            ref THandle swappedHandle = ref _handles[handleThatGotMoved.Index];
            swappedHandle = swappedHandle with { Index = handleToBeDeleted.Index };
        }

        // Increment version to invalidate existing handles
        handleToBeDeleted = handleToBeDeleted with 
        {
            // Increment version so all the previous instances are invalid and it's ready for future recycle
            Version = handle.Version + 1,
            Index = _freeIndex
        };

        // Update free list
        if (_freeIndex == Tombstone)
        {
            _lastFreeIndex = handle.Index;
        }

        _freeIndex = handle.Index;

        return true;
    }

    
    public bool TryGetValue1(THandle handle, out TValue1 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue2((int)index);
        return true;
    }
    public bool TryGetValue2(THandle handle, out TValue2 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue3((int)index);
        return true;
    }
    public bool TryGetValue3(THandle handle, out TValue3 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue4((int)index);
        return true;
    }
    public bool TryGetValue4(THandle handle, out TValue4 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue5((int)index);
        return true;
    }
    
    
    public TValue1 GetValue1(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue2((int)index);
    }
    public TValue2 GetValue2(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue3((int)index);
    }
    public TValue3 GetValue3(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue4((int)index);
    }
    public TValue4 GetValue4(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue5((int)index);
    }

    
    public ref TValue1 UnsafeTryGetRefValue1(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue1>();
        }
        
        value = false;
        return ref _dense.GetRefValue2((int)index);
    }
    public ref TValue2 UnsafeTryGetRefValue2(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue2>();
        }
        
        value = false;
        return ref _dense.GetRefValue3((int)index);
    }
    public ref TValue3 UnsafeTryGetRefValue3(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue3>();
        }
        
        value = false;
        return ref _dense.GetRefValue4((int)index);
    }
    public ref TValue4 UnsafeTryGetRefValue4(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue4>();
        }
        
        value = false;
        return ref _dense.GetRefValue5((int)index);
    }

    public bool Contains(THandle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public bool Contains(THandle handle, out uint index)
    {
        if (handle.Index >= _handles.Length)
        {
            index = default;
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        index = storedHandle.Index; 
        return storedHandle.Version == handle.Version;
    }

    public int Length => _dense.Length;

    public Span<THandle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
}
public class DenseSlotMap<THandle, TValue1, TValue2, TValue3, TValue4, TValue5>
    where THandle: struct, IHandle<THandle>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<THandle> _handles;
    private MultiArrayStruct<THandle, TValue1, TValue2, TValue3, TValue4, TValue5> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMap()
    {
        _handles = new FastListStruct<THandle>();
        _dense = new MultiArrayStruct<THandle, TValue1, TValue2, TValue3, TValue4, TValue5>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public THandle Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Length;
            THandle handle = new THandle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new THandle{ Index = handleIndex, Version = 0 }, value1, value2, value3, value4, value5);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref THandle handleToRecycle = ref _handles[handleIndex];

            bool isItTheOnlyFreeIndex = _freeIndex == _lastFreeIndex;
            if (isItTheOnlyFreeIndex)
            {
                _freeIndex = Tombstone;
                _lastFreeIndex = Tombstone;
            }
            else
            {
                _freeIndex = handleToRecycle.Index;
            }
            
            // Note the version has been incremented on removal too, so it's free to use already
            THandle finalHandle = new THandle { Index = handleIndex, Version = handleToRecycle.Version };
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2, value3, value4, value5);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(THandle handle)
    {
        if (handle.IsNull())
        {
            return false;
        }

        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref THandle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out THandle handleThatGotMoved))
        {
            ref THandle swappedHandle = ref _handles[handleThatGotMoved.Index];
            swappedHandle = swappedHandle with { Index = handleToBeDeleted.Index };
        }

        // Increment version to invalidate existing handles
        handleToBeDeleted = handleToBeDeleted with 
        {
            // Increment version so all the previous instances are invalid and it's ready for future recycle
            Version = handle.Version + 1,
            Index = _freeIndex
        };

        // Update free list
        if (_freeIndex == Tombstone)
        {
            _lastFreeIndex = handle.Index;
        }

        _freeIndex = handle.Index;

        return true;
    }

    
    public bool TryGetValue1(THandle handle, out TValue1 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue2((int)index);
        return true;
    }
    public bool TryGetValue2(THandle handle, out TValue2 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue3((int)index);
        return true;
    }
    public bool TryGetValue3(THandle handle, out TValue3 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue4((int)index);
        return true;
    }
    public bool TryGetValue4(THandle handle, out TValue4 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue5((int)index);
        return true;
    }
    public bool TryGetValue5(THandle handle, out TValue5 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue6((int)index);
        return true;
    }
    
    
    public TValue1 GetValue1(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue2((int)index);
    }
    public TValue2 GetValue2(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue3((int)index);
    }
    public TValue3 GetValue3(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue4((int)index);
    }
    public TValue4 GetValue4(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue5((int)index);
    }
    public TValue5 GetValue5(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue6((int)index);
    }

    
    public ref TValue1 UnsafeTryGetRefValue1(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue1>();
        }
        
        value = false;
        return ref _dense.GetRefValue2((int)index);
    }
    public ref TValue2 UnsafeTryGetRefValue2(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue2>();
        }
        
        value = false;
        return ref _dense.GetRefValue3((int)index);
    }
    public ref TValue3 UnsafeTryGetRefValue3(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue3>();
        }
        
        value = false;
        return ref _dense.GetRefValue4((int)index);
    }
    public ref TValue4 UnsafeTryGetRefValue4(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue4>();
        }
        
        value = false;
        return ref _dense.GetRefValue5((int)index);
    }
    public ref TValue5 UnsafeTryGetRefValue5(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue5>();
        }
        
        value = false;
        return ref _dense.GetRefValue6((int)index);
    }

    public bool Contains(THandle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public bool Contains(THandle handle, out uint index)
    {
        if (handle.Index >= _handles.Length)
        {
            index = default;
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        index = storedHandle.Index; 
        return storedHandle.Version == handle.Version;
    }

    public int Length => _dense.Length;

    public Span<THandle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
}
public class DenseSlotMap<THandle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6>
    where THandle: struct, IHandle<THandle>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<THandle> _handles;
    private MultiArrayStruct<THandle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMap()
    {
        _handles = new FastListStruct<THandle>();
        _dense = new MultiArrayStruct<THandle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public THandle Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Length;
            THandle handle = new THandle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new THandle{ Index = handleIndex, Version = 0 }, value1, value2, value3, value4, value5, value6);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref THandle handleToRecycle = ref _handles[handleIndex];

            bool isItTheOnlyFreeIndex = _freeIndex == _lastFreeIndex;
            if (isItTheOnlyFreeIndex)
            {
                _freeIndex = Tombstone;
                _lastFreeIndex = Tombstone;
            }
            else
            {
                _freeIndex = handleToRecycle.Index;
            }
            
            // Note the version has been incremented on removal too, so it's free to use already
            THandle finalHandle = new THandle { Index = handleIndex, Version = handleToRecycle.Version };
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2, value3, value4, value5, value6);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(THandle handle)
    {
        if (handle.IsNull())
        {
            return false;
        }

        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref THandle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out THandle handleThatGotMoved))
        {
            ref THandle swappedHandle = ref _handles[handleThatGotMoved.Index];
            swappedHandle = swappedHandle with { Index = handleToBeDeleted.Index };
        }

        // Increment version to invalidate existing handles
        handleToBeDeleted = handleToBeDeleted with 
        {
            // Increment version so all the previous instances are invalid and it's ready for future recycle
            Version = handle.Version + 1,
            Index = _freeIndex
        };

        // Update free list
        if (_freeIndex == Tombstone)
        {
            _lastFreeIndex = handle.Index;
        }

        _freeIndex = handle.Index;

        return true;
    }

    
    public bool TryGetValue1(THandle handle, out TValue1 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue2((int)index);
        return true;
    }
    public bool TryGetValue2(THandle handle, out TValue2 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue3((int)index);
        return true;
    }
    public bool TryGetValue3(THandle handle, out TValue3 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue4((int)index);
        return true;
    }
    public bool TryGetValue4(THandle handle, out TValue4 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue5((int)index);
        return true;
    }
    public bool TryGetValue5(THandle handle, out TValue5 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue6((int)index);
        return true;
    }
    public bool TryGetValue6(THandle handle, out TValue6 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue7((int)index);
        return true;
    }
    
    
    public TValue1 GetValue1(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue2((int)index);
    }
    public TValue2 GetValue2(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue3((int)index);
    }
    public TValue3 GetValue3(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue4((int)index);
    }
    public TValue4 GetValue4(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue5((int)index);
    }
    public TValue5 GetValue5(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue6((int)index);
    }
    public TValue6 GetValue6(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue7((int)index);
    }

    
    public ref TValue1 UnsafeTryGetRefValue1(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue1>();
        }
        
        value = false;
        return ref _dense.GetRefValue2((int)index);
    }
    public ref TValue2 UnsafeTryGetRefValue2(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue2>();
        }
        
        value = false;
        return ref _dense.GetRefValue3((int)index);
    }
    public ref TValue3 UnsafeTryGetRefValue3(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue3>();
        }
        
        value = false;
        return ref _dense.GetRefValue4((int)index);
    }
    public ref TValue4 UnsafeTryGetRefValue4(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue4>();
        }
        
        value = false;
        return ref _dense.GetRefValue5((int)index);
    }
    public ref TValue5 UnsafeTryGetRefValue5(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue5>();
        }
        
        value = false;
        return ref _dense.GetRefValue6((int)index);
    }
    public ref TValue6 UnsafeTryGetRefValue6(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue6>();
        }
        
        value = false;
        return ref _dense.GetRefValue7((int)index);
    }

    public bool Contains(THandle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public bool Contains(THandle handle, out uint index)
    {
        if (handle.Index >= _handles.Length)
        {
            index = default;
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        index = storedHandle.Index; 
        return storedHandle.Version == handle.Version;
    }

    public int Length => _dense.Length;

    public Span<THandle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
    public Span<TValue6> Values6 => _dense.Values7;
}
public class DenseSlotMap<THandle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7>
    where THandle: struct, IHandle<THandle>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<THandle> _handles;
    private MultiArrayStruct<THandle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMap()
    {
        _handles = new FastListStruct<THandle>();
        _dense = new MultiArrayStruct<THandle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public THandle Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Length;
            THandle handle = new THandle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new THandle{ Index = handleIndex, Version = 0 }, value1, value2, value3, value4, value5, value6, value7);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref THandle handleToRecycle = ref _handles[handleIndex];

            bool isItTheOnlyFreeIndex = _freeIndex == _lastFreeIndex;
            if (isItTheOnlyFreeIndex)
            {
                _freeIndex = Tombstone;
                _lastFreeIndex = Tombstone;
            }
            else
            {
                _freeIndex = handleToRecycle.Index;
            }
            
            // Note the version has been incremented on removal too, so it's free to use already
            THandle finalHandle = new THandle { Index = handleIndex, Version = handleToRecycle.Version };
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2, value3, value4, value5, value6, value7);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(THandle handle)
    {
        if (handle.IsNull())
        {
            return false;
        }

        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref THandle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out THandle handleThatGotMoved))
        {
            ref THandle swappedHandle = ref _handles[handleThatGotMoved.Index];
            swappedHandle = swappedHandle with { Index = handleToBeDeleted.Index };
        }

        // Increment version to invalidate existing handles
        handleToBeDeleted = handleToBeDeleted with 
        {
            // Increment version so all the previous instances are invalid and it's ready for future recycle
            Version = handle.Version + 1,
            Index = _freeIndex
        };

        // Update free list
        if (_freeIndex == Tombstone)
        {
            _lastFreeIndex = handle.Index;
        }

        _freeIndex = handle.Index;

        return true;
    }

    
    public bool TryGetValue1(THandle handle, out TValue1 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue2((int)index);
        return true;
    }
    public bool TryGetValue2(THandle handle, out TValue2 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue3((int)index);
        return true;
    }
    public bool TryGetValue3(THandle handle, out TValue3 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue4((int)index);
        return true;
    }
    public bool TryGetValue4(THandle handle, out TValue4 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue5((int)index);
        return true;
    }
    public bool TryGetValue5(THandle handle, out TValue5 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue6((int)index);
        return true;
    }
    public bool TryGetValue6(THandle handle, out TValue6 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue7((int)index);
        return true;
    }
    public bool TryGetValue7(THandle handle, out TValue7 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue8((int)index);
        return true;
    }
    
    
    public TValue1 GetValue1(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue2((int)index);
    }
    public TValue2 GetValue2(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue3((int)index);
    }
    public TValue3 GetValue3(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue4((int)index);
    }
    public TValue4 GetValue4(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue5((int)index);
    }
    public TValue5 GetValue5(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue6((int)index);
    }
    public TValue6 GetValue6(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue7((int)index);
    }
    public TValue7 GetValue7(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue8((int)index);
    }

    
    public ref TValue1 UnsafeTryGetRefValue1(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue1>();
        }
        
        value = false;
        return ref _dense.GetRefValue2((int)index);
    }
    public ref TValue2 UnsafeTryGetRefValue2(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue2>();
        }
        
        value = false;
        return ref _dense.GetRefValue3((int)index);
    }
    public ref TValue3 UnsafeTryGetRefValue3(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue3>();
        }
        
        value = false;
        return ref _dense.GetRefValue4((int)index);
    }
    public ref TValue4 UnsafeTryGetRefValue4(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue4>();
        }
        
        value = false;
        return ref _dense.GetRefValue5((int)index);
    }
    public ref TValue5 UnsafeTryGetRefValue5(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue5>();
        }
        
        value = false;
        return ref _dense.GetRefValue6((int)index);
    }
    public ref TValue6 UnsafeTryGetRefValue6(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue6>();
        }
        
        value = false;
        return ref _dense.GetRefValue7((int)index);
    }
    public ref TValue7 UnsafeTryGetRefValue7(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue7>();
        }
        
        value = false;
        return ref _dense.GetRefValue8((int)index);
    }

    public bool Contains(THandle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public bool Contains(THandle handle, out uint index)
    {
        if (handle.Index >= _handles.Length)
        {
            index = default;
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        index = storedHandle.Index; 
        return storedHandle.Version == handle.Version;
    }

    public int Length => _dense.Length;

    public Span<THandle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
    public Span<TValue6> Values6 => _dense.Values7;
    public Span<TValue7> Values7 => _dense.Values8;
}
public class DenseSlotMap<THandle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>
    where THandle: struct, IHandle<THandle>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<THandle> _handles;
    private MultiArrayStruct<THandle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMap()
    {
        _handles = new FastListStruct<THandle>();
        _dense = new MultiArrayStruct<THandle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public THandle Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Length;
            THandle handle = new THandle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new THandle{ Index = handleIndex, Version = 0 }, value1, value2, value3, value4, value5, value6, value7, value8);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref THandle handleToRecycle = ref _handles[handleIndex];

            bool isItTheOnlyFreeIndex = _freeIndex == _lastFreeIndex;
            if (isItTheOnlyFreeIndex)
            {
                _freeIndex = Tombstone;
                _lastFreeIndex = Tombstone;
            }
            else
            {
                _freeIndex = handleToRecycle.Index;
            }
            
            // Note the version has been incremented on removal too, so it's free to use already
            THandle finalHandle = new THandle { Index = handleIndex, Version = handleToRecycle.Version };
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2, value3, value4, value5, value6, value7, value8);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(THandle handle)
    {
        if (handle.IsNull())
        {
            return false;
        }

        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref THandle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out THandle handleThatGotMoved))
        {
            ref THandle swappedHandle = ref _handles[handleThatGotMoved.Index];
            swappedHandle = swappedHandle with { Index = handleToBeDeleted.Index };
        }

        // Increment version to invalidate existing handles
        handleToBeDeleted = handleToBeDeleted with 
        {
            // Increment version so all the previous instances are invalid and it's ready for future recycle
            Version = handle.Version + 1,
            Index = _freeIndex
        };

        // Update free list
        if (_freeIndex == Tombstone)
        {
            _lastFreeIndex = handle.Index;
        }

        _freeIndex = handle.Index;

        return true;
    }

    
    public bool TryGetValue1(THandle handle, out TValue1 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue2((int)index);
        return true;
    }
    public bool TryGetValue2(THandle handle, out TValue2 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue3((int)index);
        return true;
    }
    public bool TryGetValue3(THandle handle, out TValue3 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue4((int)index);
        return true;
    }
    public bool TryGetValue4(THandle handle, out TValue4 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue5((int)index);
        return true;
    }
    public bool TryGetValue5(THandle handle, out TValue5 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue6((int)index);
        return true;
    }
    public bool TryGetValue6(THandle handle, out TValue6 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue7((int)index);
        return true;
    }
    public bool TryGetValue7(THandle handle, out TValue7 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue8((int)index);
        return true;
    }
    public bool TryGetValue8(THandle handle, out TValue8 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue9((int)index);
        return true;
    }
    
    
    public TValue1 GetValue1(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue2((int)index);
    }
    public TValue2 GetValue2(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue3((int)index);
    }
    public TValue3 GetValue3(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue4((int)index);
    }
    public TValue4 GetValue4(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue5((int)index);
    }
    public TValue5 GetValue5(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue6((int)index);
    }
    public TValue6 GetValue6(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue7((int)index);
    }
    public TValue7 GetValue7(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue8((int)index);
    }
    public TValue8 GetValue8(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue9((int)index);
    }

    
    public ref TValue1 UnsafeTryGetRefValue1(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue1>();
        }
        
        value = false;
        return ref _dense.GetRefValue2((int)index);
    }
    public ref TValue2 UnsafeTryGetRefValue2(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue2>();
        }
        
        value = false;
        return ref _dense.GetRefValue3((int)index);
    }
    public ref TValue3 UnsafeTryGetRefValue3(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue3>();
        }
        
        value = false;
        return ref _dense.GetRefValue4((int)index);
    }
    public ref TValue4 UnsafeTryGetRefValue4(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue4>();
        }
        
        value = false;
        return ref _dense.GetRefValue5((int)index);
    }
    public ref TValue5 UnsafeTryGetRefValue5(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue5>();
        }
        
        value = false;
        return ref _dense.GetRefValue6((int)index);
    }
    public ref TValue6 UnsafeTryGetRefValue6(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue6>();
        }
        
        value = false;
        return ref _dense.GetRefValue7((int)index);
    }
    public ref TValue7 UnsafeTryGetRefValue7(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue7>();
        }
        
        value = false;
        return ref _dense.GetRefValue8((int)index);
    }
    public ref TValue8 UnsafeTryGetRefValue8(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue8>();
        }
        
        value = false;
        return ref _dense.GetRefValue9((int)index);
    }

    public bool Contains(THandle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public bool Contains(THandle handle, out uint index)
    {
        if (handle.Index >= _handles.Length)
        {
            index = default;
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        index = storedHandle.Index; 
        return storedHandle.Version == handle.Version;
    }

    public int Length => _dense.Length;

    public Span<THandle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
    public Span<TValue6> Values6 => _dense.Values7;
    public Span<TValue7> Values7 => _dense.Values8;
    public Span<TValue8> Values8 => _dense.Values9;
}

public struct DenseSlotMapStruct<THandle, TValue1>
    where THandle: struct, IHandle<THandle>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<THandle> _handles;
    private MultiArrayStruct<THandle, TValue1> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMapStruct()
    {
        _handles = new FastListStruct<THandle>();
        _dense = new MultiArrayStruct<THandle, TValue1>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public THandle Add(TValue1 value1)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Length;
            THandle handle = new THandle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new THandle{ Index = handleIndex, Version = 0 }, value1);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref THandle handleToRecycle = ref _handles[handleIndex];

            bool isItTheOnlyFreeIndex = _freeIndex == _lastFreeIndex;
            if (isItTheOnlyFreeIndex)
            {
                _freeIndex = Tombstone;
                _lastFreeIndex = Tombstone;
            }
            else
            {
                _freeIndex = handleToRecycle.Index;
            }
            
            // Note the version has been incremented on removal too, so it's free to use already
            THandle finalHandle = new THandle { Index = handleIndex, Version = handleToRecycle.Version };
            uint denseIndex = (uint)_dense.Add(finalHandle, value1);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(THandle handle)
    {
        if (handle.IsNull())
        {
            return false;
        }

        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref THandle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out THandle handleThatGotMoved))
        {
            ref THandle swappedHandle = ref _handles[handleThatGotMoved.Index];
            swappedHandle = swappedHandle with { Index = handleToBeDeleted.Index };
        }

        // Increment version to invalidate existing handles
        handleToBeDeleted = handleToBeDeleted with 
        {
            // Increment version so all the previous instances are invalid and it's ready for future recycle
            Version = handle.Version + 1,
            Index = _freeIndex
        };

        // Update free list
        if (_freeIndex == Tombstone)
        {
            _lastFreeIndex = handle.Index;
        }

        _freeIndex = handle.Index;

        return true;
    }

    
    public bool TryGetValue1(THandle handle, out TValue1 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue2((int)index);
        return true;
    }
    
    
    public TValue1 GetValue1(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue2((int)index);
    }

    
    public ref TValue1 UnsafeTryGetRefValue1(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue1>();
        }
        
        value = false;
        return ref _dense.GetRefValue2((int)index);
    }

    public bool Contains(THandle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public bool Contains(THandle handle, out uint index)
    {
        if (handle.Index >= _handles.Length)
        {
            index = default;
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        index = storedHandle.Index; 
        return storedHandle.Version == handle.Version;
    }

    public int Length => _dense.Length;

    public Span<THandle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
}
public struct DenseSlotMapStruct<THandle, TValue1, TValue2>
    where THandle: struct, IHandle<THandle>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<THandle> _handles;
    private MultiArrayStruct<THandle, TValue1, TValue2> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMapStruct()
    {
        _handles = new FastListStruct<THandle>();
        _dense = new MultiArrayStruct<THandle, TValue1, TValue2>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public THandle Add(TValue1 value1, TValue2 value2)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Length;
            THandle handle = new THandle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new THandle{ Index = handleIndex, Version = 0 }, value1, value2);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref THandle handleToRecycle = ref _handles[handleIndex];

            bool isItTheOnlyFreeIndex = _freeIndex == _lastFreeIndex;
            if (isItTheOnlyFreeIndex)
            {
                _freeIndex = Tombstone;
                _lastFreeIndex = Tombstone;
            }
            else
            {
                _freeIndex = handleToRecycle.Index;
            }
            
            // Note the version has been incremented on removal too, so it's free to use already
            THandle finalHandle = new THandle { Index = handleIndex, Version = handleToRecycle.Version };
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(THandle handle)
    {
        if (handle.IsNull())
        {
            return false;
        }

        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref THandle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out THandle handleThatGotMoved))
        {
            ref THandle swappedHandle = ref _handles[handleThatGotMoved.Index];
            swappedHandle = swappedHandle with { Index = handleToBeDeleted.Index };
        }

        // Increment version to invalidate existing handles
        handleToBeDeleted = handleToBeDeleted with 
        {
            // Increment version so all the previous instances are invalid and it's ready for future recycle
            Version = handle.Version + 1,
            Index = _freeIndex
        };

        // Update free list
        if (_freeIndex == Tombstone)
        {
            _lastFreeIndex = handle.Index;
        }

        _freeIndex = handle.Index;

        return true;
    }

    
    public bool TryGetValue1(THandle handle, out TValue1 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue2((int)index);
        return true;
    }
    public bool TryGetValue2(THandle handle, out TValue2 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue3((int)index);
        return true;
    }
    
    
    public TValue1 GetValue1(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue2((int)index);
    }
    public TValue2 GetValue2(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue3((int)index);
    }

    
    public ref TValue1 UnsafeTryGetRefValue1(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue1>();
        }
        
        value = false;
        return ref _dense.GetRefValue2((int)index);
    }
    public ref TValue2 UnsafeTryGetRefValue2(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue2>();
        }
        
        value = false;
        return ref _dense.GetRefValue3((int)index);
    }

    public bool Contains(THandle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public bool Contains(THandle handle, out uint index)
    {
        if (handle.Index >= _handles.Length)
        {
            index = default;
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        index = storedHandle.Index; 
        return storedHandle.Version == handle.Version;
    }

    public int Length => _dense.Length;

    public Span<THandle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
}
public struct DenseSlotMapStruct<THandle, TValue1, TValue2, TValue3>
    where THandle: struct, IHandle<THandle>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<THandle> _handles;
    private MultiArrayStruct<THandle, TValue1, TValue2, TValue3> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMapStruct()
    {
        _handles = new FastListStruct<THandle>();
        _dense = new MultiArrayStruct<THandle, TValue1, TValue2, TValue3>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public THandle Add(TValue1 value1, TValue2 value2, TValue3 value3)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Length;
            THandle handle = new THandle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new THandle{ Index = handleIndex, Version = 0 }, value1, value2, value3);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref THandle handleToRecycle = ref _handles[handleIndex];

            bool isItTheOnlyFreeIndex = _freeIndex == _lastFreeIndex;
            if (isItTheOnlyFreeIndex)
            {
                _freeIndex = Tombstone;
                _lastFreeIndex = Tombstone;
            }
            else
            {
                _freeIndex = handleToRecycle.Index;
            }
            
            // Note the version has been incremented on removal too, so it's free to use already
            THandle finalHandle = new THandle { Index = handleIndex, Version = handleToRecycle.Version };
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2, value3);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(THandle handle)
    {
        if (handle.IsNull())
        {
            return false;
        }

        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref THandle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out THandle handleThatGotMoved))
        {
            ref THandle swappedHandle = ref _handles[handleThatGotMoved.Index];
            swappedHandle = swappedHandle with { Index = handleToBeDeleted.Index };
        }

        // Increment version to invalidate existing handles
        handleToBeDeleted = handleToBeDeleted with 
        {
            // Increment version so all the previous instances are invalid and it's ready for future recycle
            Version = handle.Version + 1,
            Index = _freeIndex
        };

        // Update free list
        if (_freeIndex == Tombstone)
        {
            _lastFreeIndex = handle.Index;
        }

        _freeIndex = handle.Index;

        return true;
    }

    
    public bool TryGetValue1(THandle handle, out TValue1 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue2((int)index);
        return true;
    }
    public bool TryGetValue2(THandle handle, out TValue2 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue3((int)index);
        return true;
    }
    public bool TryGetValue3(THandle handle, out TValue3 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue4((int)index);
        return true;
    }
    
    
    public TValue1 GetValue1(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue2((int)index);
    }
    public TValue2 GetValue2(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue3((int)index);
    }
    public TValue3 GetValue3(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue4((int)index);
    }

    
    public ref TValue1 UnsafeTryGetRefValue1(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue1>();
        }
        
        value = false;
        return ref _dense.GetRefValue2((int)index);
    }
    public ref TValue2 UnsafeTryGetRefValue2(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue2>();
        }
        
        value = false;
        return ref _dense.GetRefValue3((int)index);
    }
    public ref TValue3 UnsafeTryGetRefValue3(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue3>();
        }
        
        value = false;
        return ref _dense.GetRefValue4((int)index);
    }

    public bool Contains(THandle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public bool Contains(THandle handle, out uint index)
    {
        if (handle.Index >= _handles.Length)
        {
            index = default;
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        index = storedHandle.Index; 
        return storedHandle.Version == handle.Version;
    }

    public int Length => _dense.Length;

    public Span<THandle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
}
public struct DenseSlotMapStruct<THandle, TValue1, TValue2, TValue3, TValue4>
    where THandle: struct, IHandle<THandle>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<THandle> _handles;
    private MultiArrayStruct<THandle, TValue1, TValue2, TValue3, TValue4> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMapStruct()
    {
        _handles = new FastListStruct<THandle>();
        _dense = new MultiArrayStruct<THandle, TValue1, TValue2, TValue3, TValue4>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public THandle Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Length;
            THandle handle = new THandle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new THandle{ Index = handleIndex, Version = 0 }, value1, value2, value3, value4);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref THandle handleToRecycle = ref _handles[handleIndex];

            bool isItTheOnlyFreeIndex = _freeIndex == _lastFreeIndex;
            if (isItTheOnlyFreeIndex)
            {
                _freeIndex = Tombstone;
                _lastFreeIndex = Tombstone;
            }
            else
            {
                _freeIndex = handleToRecycle.Index;
            }
            
            // Note the version has been incremented on removal too, so it's free to use already
            THandle finalHandle = new THandle { Index = handleIndex, Version = handleToRecycle.Version };
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2, value3, value4);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(THandle handle)
    {
        if (handle.IsNull())
        {
            return false;
        }

        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref THandle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out THandle handleThatGotMoved))
        {
            ref THandle swappedHandle = ref _handles[handleThatGotMoved.Index];
            swappedHandle = swappedHandle with { Index = handleToBeDeleted.Index };
        }

        // Increment version to invalidate existing handles
        handleToBeDeleted = handleToBeDeleted with 
        {
            // Increment version so all the previous instances are invalid and it's ready for future recycle
            Version = handle.Version + 1,
            Index = _freeIndex
        };

        // Update free list
        if (_freeIndex == Tombstone)
        {
            _lastFreeIndex = handle.Index;
        }

        _freeIndex = handle.Index;

        return true;
    }

    
    public bool TryGetValue1(THandle handle, out TValue1 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue2((int)index);
        return true;
    }
    public bool TryGetValue2(THandle handle, out TValue2 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue3((int)index);
        return true;
    }
    public bool TryGetValue3(THandle handle, out TValue3 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue4((int)index);
        return true;
    }
    public bool TryGetValue4(THandle handle, out TValue4 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue5((int)index);
        return true;
    }
    
    
    public TValue1 GetValue1(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue2((int)index);
    }
    public TValue2 GetValue2(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue3((int)index);
    }
    public TValue3 GetValue3(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue4((int)index);
    }
    public TValue4 GetValue4(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue5((int)index);
    }

    
    public ref TValue1 UnsafeTryGetRefValue1(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue1>();
        }
        
        value = false;
        return ref _dense.GetRefValue2((int)index);
    }
    public ref TValue2 UnsafeTryGetRefValue2(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue2>();
        }
        
        value = false;
        return ref _dense.GetRefValue3((int)index);
    }
    public ref TValue3 UnsafeTryGetRefValue3(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue3>();
        }
        
        value = false;
        return ref _dense.GetRefValue4((int)index);
    }
    public ref TValue4 UnsafeTryGetRefValue4(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue4>();
        }
        
        value = false;
        return ref _dense.GetRefValue5((int)index);
    }

    public bool Contains(THandle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public bool Contains(THandle handle, out uint index)
    {
        if (handle.Index >= _handles.Length)
        {
            index = default;
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        index = storedHandle.Index; 
        return storedHandle.Version == handle.Version;
    }

    public int Length => _dense.Length;

    public Span<THandle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
}
public struct DenseSlotMapStruct<THandle, TValue1, TValue2, TValue3, TValue4, TValue5>
    where THandle: struct, IHandle<THandle>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<THandle> _handles;
    private MultiArrayStruct<THandle, TValue1, TValue2, TValue3, TValue4, TValue5> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMapStruct()
    {
        _handles = new FastListStruct<THandle>();
        _dense = new MultiArrayStruct<THandle, TValue1, TValue2, TValue3, TValue4, TValue5>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public THandle Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Length;
            THandle handle = new THandle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new THandle{ Index = handleIndex, Version = 0 }, value1, value2, value3, value4, value5);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref THandle handleToRecycle = ref _handles[handleIndex];

            bool isItTheOnlyFreeIndex = _freeIndex == _lastFreeIndex;
            if (isItTheOnlyFreeIndex)
            {
                _freeIndex = Tombstone;
                _lastFreeIndex = Tombstone;
            }
            else
            {
                _freeIndex = handleToRecycle.Index;
            }
            
            // Note the version has been incremented on removal too, so it's free to use already
            THandle finalHandle = new THandle { Index = handleIndex, Version = handleToRecycle.Version };
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2, value3, value4, value5);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(THandle handle)
    {
        if (handle.IsNull())
        {
            return false;
        }

        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref THandle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out THandle handleThatGotMoved))
        {
            ref THandle swappedHandle = ref _handles[handleThatGotMoved.Index];
            swappedHandle = swappedHandle with { Index = handleToBeDeleted.Index };
        }

        // Increment version to invalidate existing handles
        handleToBeDeleted = handleToBeDeleted with 
        {
            // Increment version so all the previous instances are invalid and it's ready for future recycle
            Version = handle.Version + 1,
            Index = _freeIndex
        };

        // Update free list
        if (_freeIndex == Tombstone)
        {
            _lastFreeIndex = handle.Index;
        }

        _freeIndex = handle.Index;

        return true;
    }

    
    public bool TryGetValue1(THandle handle, out TValue1 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue2((int)index);
        return true;
    }
    public bool TryGetValue2(THandle handle, out TValue2 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue3((int)index);
        return true;
    }
    public bool TryGetValue3(THandle handle, out TValue3 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue4((int)index);
        return true;
    }
    public bool TryGetValue4(THandle handle, out TValue4 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue5((int)index);
        return true;
    }
    public bool TryGetValue5(THandle handle, out TValue5 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue6((int)index);
        return true;
    }
    
    
    public TValue1 GetValue1(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue2((int)index);
    }
    public TValue2 GetValue2(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue3((int)index);
    }
    public TValue3 GetValue3(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue4((int)index);
    }
    public TValue4 GetValue4(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue5((int)index);
    }
    public TValue5 GetValue5(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue6((int)index);
    }

    
    public ref TValue1 UnsafeTryGetRefValue1(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue1>();
        }
        
        value = false;
        return ref _dense.GetRefValue2((int)index);
    }
    public ref TValue2 UnsafeTryGetRefValue2(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue2>();
        }
        
        value = false;
        return ref _dense.GetRefValue3((int)index);
    }
    public ref TValue3 UnsafeTryGetRefValue3(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue3>();
        }
        
        value = false;
        return ref _dense.GetRefValue4((int)index);
    }
    public ref TValue4 UnsafeTryGetRefValue4(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue4>();
        }
        
        value = false;
        return ref _dense.GetRefValue5((int)index);
    }
    public ref TValue5 UnsafeTryGetRefValue5(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue5>();
        }
        
        value = false;
        return ref _dense.GetRefValue6((int)index);
    }

    public bool Contains(THandle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public bool Contains(THandle handle, out uint index)
    {
        if (handle.Index >= _handles.Length)
        {
            index = default;
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        index = storedHandle.Index; 
        return storedHandle.Version == handle.Version;
    }

    public int Length => _dense.Length;

    public Span<THandle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
}
public struct DenseSlotMapStruct<THandle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6>
    where THandle: struct, IHandle<THandle>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<THandle> _handles;
    private MultiArrayStruct<THandle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMapStruct()
    {
        _handles = new FastListStruct<THandle>();
        _dense = new MultiArrayStruct<THandle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public THandle Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Length;
            THandle handle = new THandle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new THandle{ Index = handleIndex, Version = 0 }, value1, value2, value3, value4, value5, value6);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref THandle handleToRecycle = ref _handles[handleIndex];

            bool isItTheOnlyFreeIndex = _freeIndex == _lastFreeIndex;
            if (isItTheOnlyFreeIndex)
            {
                _freeIndex = Tombstone;
                _lastFreeIndex = Tombstone;
            }
            else
            {
                _freeIndex = handleToRecycle.Index;
            }
            
            // Note the version has been incremented on removal too, so it's free to use already
            THandle finalHandle = new THandle { Index = handleIndex, Version = handleToRecycle.Version };
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2, value3, value4, value5, value6);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(THandle handle)
    {
        if (handle.IsNull())
        {
            return false;
        }

        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref THandle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out THandle handleThatGotMoved))
        {
            ref THandle swappedHandle = ref _handles[handleThatGotMoved.Index];
            swappedHandle = swappedHandle with { Index = handleToBeDeleted.Index };
        }

        // Increment version to invalidate existing handles
        handleToBeDeleted = handleToBeDeleted with 
        {
            // Increment version so all the previous instances are invalid and it's ready for future recycle
            Version = handle.Version + 1,
            Index = _freeIndex
        };

        // Update free list
        if (_freeIndex == Tombstone)
        {
            _lastFreeIndex = handle.Index;
        }

        _freeIndex = handle.Index;

        return true;
    }

    
    public bool TryGetValue1(THandle handle, out TValue1 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue2((int)index);
        return true;
    }
    public bool TryGetValue2(THandle handle, out TValue2 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue3((int)index);
        return true;
    }
    public bool TryGetValue3(THandle handle, out TValue3 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue4((int)index);
        return true;
    }
    public bool TryGetValue4(THandle handle, out TValue4 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue5((int)index);
        return true;
    }
    public bool TryGetValue5(THandle handle, out TValue5 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue6((int)index);
        return true;
    }
    public bool TryGetValue6(THandle handle, out TValue6 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue7((int)index);
        return true;
    }
    
    
    public TValue1 GetValue1(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue2((int)index);
    }
    public TValue2 GetValue2(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue3((int)index);
    }
    public TValue3 GetValue3(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue4((int)index);
    }
    public TValue4 GetValue4(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue5((int)index);
    }
    public TValue5 GetValue5(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue6((int)index);
    }
    public TValue6 GetValue6(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue7((int)index);
    }

    
    public ref TValue1 UnsafeTryGetRefValue1(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue1>();
        }
        
        value = false;
        return ref _dense.GetRefValue2((int)index);
    }
    public ref TValue2 UnsafeTryGetRefValue2(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue2>();
        }
        
        value = false;
        return ref _dense.GetRefValue3((int)index);
    }
    public ref TValue3 UnsafeTryGetRefValue3(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue3>();
        }
        
        value = false;
        return ref _dense.GetRefValue4((int)index);
    }
    public ref TValue4 UnsafeTryGetRefValue4(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue4>();
        }
        
        value = false;
        return ref _dense.GetRefValue5((int)index);
    }
    public ref TValue5 UnsafeTryGetRefValue5(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue5>();
        }
        
        value = false;
        return ref _dense.GetRefValue6((int)index);
    }
    public ref TValue6 UnsafeTryGetRefValue6(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue6>();
        }
        
        value = false;
        return ref _dense.GetRefValue7((int)index);
    }

    public bool Contains(THandle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public bool Contains(THandle handle, out uint index)
    {
        if (handle.Index >= _handles.Length)
        {
            index = default;
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        index = storedHandle.Index; 
        return storedHandle.Version == handle.Version;
    }

    public int Length => _dense.Length;

    public Span<THandle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
    public Span<TValue6> Values6 => _dense.Values7;
}
public struct DenseSlotMapStruct<THandle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7>
    where THandle: struct, IHandle<THandle>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<THandle> _handles;
    private MultiArrayStruct<THandle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMapStruct()
    {
        _handles = new FastListStruct<THandle>();
        _dense = new MultiArrayStruct<THandle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public THandle Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Length;
            THandle handle = new THandle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new THandle{ Index = handleIndex, Version = 0 }, value1, value2, value3, value4, value5, value6, value7);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref THandle handleToRecycle = ref _handles[handleIndex];

            bool isItTheOnlyFreeIndex = _freeIndex == _lastFreeIndex;
            if (isItTheOnlyFreeIndex)
            {
                _freeIndex = Tombstone;
                _lastFreeIndex = Tombstone;
            }
            else
            {
                _freeIndex = handleToRecycle.Index;
            }
            
            // Note the version has been incremented on removal too, so it's free to use already
            THandle finalHandle = new THandle { Index = handleIndex, Version = handleToRecycle.Version };
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2, value3, value4, value5, value6, value7);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(THandle handle)
    {
        if (handle.IsNull())
        {
            return false;
        }

        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref THandle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out THandle handleThatGotMoved))
        {
            ref THandle swappedHandle = ref _handles[handleThatGotMoved.Index];
            swappedHandle = swappedHandle with { Index = handleToBeDeleted.Index };
        }

        // Increment version to invalidate existing handles
        handleToBeDeleted = handleToBeDeleted with 
        {
            // Increment version so all the previous instances are invalid and it's ready for future recycle
            Version = handle.Version + 1,
            Index = _freeIndex
        };

        // Update free list
        if (_freeIndex == Tombstone)
        {
            _lastFreeIndex = handle.Index;
        }

        _freeIndex = handle.Index;

        return true;
    }

    
    public bool TryGetValue1(THandle handle, out TValue1 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue2((int)index);
        return true;
    }
    public bool TryGetValue2(THandle handle, out TValue2 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue3((int)index);
        return true;
    }
    public bool TryGetValue3(THandle handle, out TValue3 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue4((int)index);
        return true;
    }
    public bool TryGetValue4(THandle handle, out TValue4 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue5((int)index);
        return true;
    }
    public bool TryGetValue5(THandle handle, out TValue5 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue6((int)index);
        return true;
    }
    public bool TryGetValue6(THandle handle, out TValue6 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue7((int)index);
        return true;
    }
    public bool TryGetValue7(THandle handle, out TValue7 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue8((int)index);
        return true;
    }
    
    
    public TValue1 GetValue1(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue2((int)index);
    }
    public TValue2 GetValue2(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue3((int)index);
    }
    public TValue3 GetValue3(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue4((int)index);
    }
    public TValue4 GetValue4(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue5((int)index);
    }
    public TValue5 GetValue5(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue6((int)index);
    }
    public TValue6 GetValue6(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue7((int)index);
    }
    public TValue7 GetValue7(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue8((int)index);
    }

    
    public ref TValue1 UnsafeTryGetRefValue1(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue1>();
        }
        
        value = false;
        return ref _dense.GetRefValue2((int)index);
    }
    public ref TValue2 UnsafeTryGetRefValue2(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue2>();
        }
        
        value = false;
        return ref _dense.GetRefValue3((int)index);
    }
    public ref TValue3 UnsafeTryGetRefValue3(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue3>();
        }
        
        value = false;
        return ref _dense.GetRefValue4((int)index);
    }
    public ref TValue4 UnsafeTryGetRefValue4(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue4>();
        }
        
        value = false;
        return ref _dense.GetRefValue5((int)index);
    }
    public ref TValue5 UnsafeTryGetRefValue5(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue5>();
        }
        
        value = false;
        return ref _dense.GetRefValue6((int)index);
    }
    public ref TValue6 UnsafeTryGetRefValue6(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue6>();
        }
        
        value = false;
        return ref _dense.GetRefValue7((int)index);
    }
    public ref TValue7 UnsafeTryGetRefValue7(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue7>();
        }
        
        value = false;
        return ref _dense.GetRefValue8((int)index);
    }

    public bool Contains(THandle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public bool Contains(THandle handle, out uint index)
    {
        if (handle.Index >= _handles.Length)
        {
            index = default;
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        index = storedHandle.Index; 
        return storedHandle.Version == handle.Version;
    }

    public int Length => _dense.Length;

    public Span<THandle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
    public Span<TValue6> Values6 => _dense.Values7;
    public Span<TValue7> Values7 => _dense.Values8;
}
public struct DenseSlotMapStruct<THandle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>
    where THandle: struct, IHandle<THandle>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<THandle> _handles;
    private MultiArrayStruct<THandle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMapStruct()
    {
        _handles = new FastListStruct<THandle>();
        _dense = new MultiArrayStruct<THandle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public THandle Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Length;
            THandle handle = new THandle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new THandle{ Index = handleIndex, Version = 0 }, value1, value2, value3, value4, value5, value6, value7, value8);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref THandle handleToRecycle = ref _handles[handleIndex];

            bool isItTheOnlyFreeIndex = _freeIndex == _lastFreeIndex;
            if (isItTheOnlyFreeIndex)
            {
                _freeIndex = Tombstone;
                _lastFreeIndex = Tombstone;
            }
            else
            {
                _freeIndex = handleToRecycle.Index;
            }
            
            // Note the version has been incremented on removal too, so it's free to use already
            THandle finalHandle = new THandle { Index = handleIndex, Version = handleToRecycle.Version };
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2, value3, value4, value5, value6, value7, value8);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(THandle handle)
    {
        if (handle.IsNull())
        {
            return false;
        }

        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref THandle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out THandle handleThatGotMoved))
        {
            ref THandle swappedHandle = ref _handles[handleThatGotMoved.Index];
            swappedHandle = swappedHandle with { Index = handleToBeDeleted.Index };
        }

        // Increment version to invalidate existing handles
        handleToBeDeleted = handleToBeDeleted with 
        {
            // Increment version so all the previous instances are invalid and it's ready for future recycle
            Version = handle.Version + 1,
            Index = _freeIndex
        };

        // Update free list
        if (_freeIndex == Tombstone)
        {
            _lastFreeIndex = handle.Index;
        }

        _freeIndex = handle.Index;

        return true;
    }

    
    public bool TryGetValue1(THandle handle, out TValue1 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue2((int)index);
        return true;
    }
    public bool TryGetValue2(THandle handle, out TValue2 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue3((int)index);
        return true;
    }
    public bool TryGetValue3(THandle handle, out TValue3 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue4((int)index);
        return true;
    }
    public bool TryGetValue4(THandle handle, out TValue4 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue5((int)index);
        return true;
    }
    public bool TryGetValue5(THandle handle, out TValue5 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue6((int)index);
        return true;
    }
    public bool TryGetValue6(THandle handle, out TValue6 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue7((int)index);
        return true;
    }
    public bool TryGetValue7(THandle handle, out TValue7 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue8((int)index);
        return true;
    }
    public bool TryGetValue8(THandle handle, out TValue8 value)
    {
        if (!Contains(handle, out uint index))
        {
            value = default;
            return false;
        }

        value = _dense.GetValue9((int)index);
        return true;
    }
    
    
    public TValue1 GetValue1(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue2((int)index);
    }
    public TValue2 GetValue2(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue3((int)index);
    }
    public TValue3 GetValue3(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue4((int)index);
    }
    public TValue4 GetValue4(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue5((int)index);
    }
    public TValue5 GetValue5(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue6((int)index);
    }
    public TValue6 GetValue6(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue7((int)index);
    }
    public TValue7 GetValue7(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue8((int)index);
    }
    public TValue8 GetValue8(THandle handle)
    {
        if (!Contains(handle, out uint index))
        {
            throw new HandleNotFoundException(handle.ToString());
        }

        return _dense.GetValue9((int)index);
    }

    
    public ref TValue1 UnsafeTryGetRefValue1(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue1>();
        }
        
        value = false;
        return ref _dense.GetRefValue2((int)index);
    }
    public ref TValue2 UnsafeTryGetRefValue2(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue2>();
        }
        
        value = false;
        return ref _dense.GetRefValue3((int)index);
    }
    public ref TValue3 UnsafeTryGetRefValue3(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue3>();
        }
        
        value = false;
        return ref _dense.GetRefValue4((int)index);
    }
    public ref TValue4 UnsafeTryGetRefValue4(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue4>();
        }
        
        value = false;
        return ref _dense.GetRefValue5((int)index);
    }
    public ref TValue5 UnsafeTryGetRefValue5(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue5>();
        }
        
        value = false;
        return ref _dense.GetRefValue6((int)index);
    }
    public ref TValue6 UnsafeTryGetRefValue6(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue6>();
        }
        
        value = false;
        return ref _dense.GetRefValue7((int)index);
    }
    public ref TValue7 UnsafeTryGetRefValue7(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue7>();
        }
        
        value = false;
        return ref _dense.GetRefValue8((int)index);
    }
    public ref TValue8 UnsafeTryGetRefValue8(THandle handle, out bool value)
    {
        if (!Contains(handle, out uint index))
        {
            value = false;
            return ref Unsafe.NullRef<TValue8>();
        }
        
        value = false;
        return ref _dense.GetRefValue9((int)index);
    }

    public bool Contains(THandle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public bool Contains(THandle handle, out uint index)
    {
        if (handle.Index >= _handles.Length)
        {
            index = default;
            return false;
        }
        
        ref THandle storedHandle = ref _handles[handle.Index];
        index = storedHandle.Index; 
        return storedHandle.Version == handle.Version;
    }

    public int Length => _dense.Length;

    public Span<THandle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
    public Span<TValue6> Values6 => _dense.Values7;
    public Span<TValue7> Values7 => _dense.Values8;
    public Span<TValue8> Values8 => _dense.Values9;
}

