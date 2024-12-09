namespace GameKit.Collections;
public class DenseSlotMap<TValue1>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<Handle> _handles;
    private MultiArrayStruct<Handle, TValue1> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMap()
    {
        _handles = new FastListStruct<Handle>();
        _dense = new MultiArrayStruct<Handle, TValue1>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public Handle Add(TValue1 value1)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Count;
            Handle handle = new Handle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new Handle(handleIndex, 0), value1);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref Handle handleToRecycle = ref _handles[handleIndex];

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
            Handle finalHandle = new Handle(handleIndex, handleToRecycle.Version);
            uint denseIndex = (uint)_dense.Add(finalHandle, value1);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(Handle handle)
    {
        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref Handle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out Handle handleThatGotMoved))
        {
            ref Handle swappedHandle = ref _handles[handleThatGotMoved.Index];
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

    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue2((int)_handles[handle.Index].Index);
        return true;
    }

    public bool Contains(Handle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref Handle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public Span<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
}
public class DenseSlotMap<TValue1, TValue2>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<Handle> _handles;
    private MultiArrayStruct<Handle, TValue1, TValue2> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMap()
    {
        _handles = new FastListStruct<Handle>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public Handle Add(TValue1 value1, TValue2 value2)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Count;
            Handle handle = new Handle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new Handle(handleIndex, 0), value1, value2);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref Handle handleToRecycle = ref _handles[handleIndex];

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
            Handle finalHandle = new Handle(handleIndex, handleToRecycle.Version);
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(Handle handle)
    {
        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref Handle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out Handle handleThatGotMoved))
        {
            ref Handle swappedHandle = ref _handles[handleThatGotMoved.Index];
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

    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue2((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue3((int)_handles[handle.Index].Index);
        return true;
    }

    public bool Contains(Handle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref Handle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public Span<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
}
public class DenseSlotMap<TValue1, TValue2, TValue3>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<Handle> _handles;
    private MultiArrayStruct<Handle, TValue1, TValue2, TValue3> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMap()
    {
        _handles = new FastListStruct<Handle>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2, TValue3>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public Handle Add(TValue1 value1, TValue2 value2, TValue3 value3)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Count;
            Handle handle = new Handle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new Handle(handleIndex, 0), value1, value2, value3);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref Handle handleToRecycle = ref _handles[handleIndex];

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
            Handle finalHandle = new Handle(handleIndex, handleToRecycle.Version);
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2, value3);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(Handle handle)
    {
        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref Handle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out Handle handleThatGotMoved))
        {
            ref Handle swappedHandle = ref _handles[handleThatGotMoved.Index];
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

    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue2((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue3((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue3(Handle handle, out TValue3 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue4((int)_handles[handle.Index].Index);
        return true;
    }

    public bool Contains(Handle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref Handle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public Span<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
}
public class DenseSlotMap<TValue1, TValue2, TValue3, TValue4>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<Handle> _handles;
    private MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMap()
    {
        _handles = new FastListStruct<Handle>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public Handle Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Count;
            Handle handle = new Handle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new Handle(handleIndex, 0), value1, value2, value3, value4);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref Handle handleToRecycle = ref _handles[handleIndex];

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
            Handle finalHandle = new Handle(handleIndex, handleToRecycle.Version);
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2, value3, value4);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(Handle handle)
    {
        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref Handle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out Handle handleThatGotMoved))
        {
            ref Handle swappedHandle = ref _handles[handleThatGotMoved.Index];
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

    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue2((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue3((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue3(Handle handle, out TValue3 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue4((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue4(Handle handle, out TValue4 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue5((int)_handles[handle.Index].Index);
        return true;
    }

    public bool Contains(Handle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref Handle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public Span<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
}
public class DenseSlotMap<TValue1, TValue2, TValue3, TValue4, TValue5>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<Handle> _handles;
    private MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMap()
    {
        _handles = new FastListStruct<Handle>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public Handle Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Count;
            Handle handle = new Handle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new Handle(handleIndex, 0), value1, value2, value3, value4, value5);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref Handle handleToRecycle = ref _handles[handleIndex];

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
            Handle finalHandle = new Handle(handleIndex, handleToRecycle.Version);
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2, value3, value4, value5);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(Handle handle)
    {
        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref Handle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out Handle handleThatGotMoved))
        {
            ref Handle swappedHandle = ref _handles[handleThatGotMoved.Index];
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

    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue2((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue3((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue3(Handle handle, out TValue3 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue4((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue4(Handle handle, out TValue4 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue5((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue5(Handle handle, out TValue5 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue6((int)_handles[handle.Index].Index);
        return true;
    }

    public bool Contains(Handle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref Handle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public Span<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
}
public class DenseSlotMap<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<Handle> _handles;
    private MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMap()
    {
        _handles = new FastListStruct<Handle>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public Handle Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Count;
            Handle handle = new Handle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new Handle(handleIndex, 0), value1, value2, value3, value4, value5, value6);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref Handle handleToRecycle = ref _handles[handleIndex];

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
            Handle finalHandle = new Handle(handleIndex, handleToRecycle.Version);
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2, value3, value4, value5, value6);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(Handle handle)
    {
        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref Handle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out Handle handleThatGotMoved))
        {
            ref Handle swappedHandle = ref _handles[handleThatGotMoved.Index];
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

    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue2((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue3((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue3(Handle handle, out TValue3 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue4((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue4(Handle handle, out TValue4 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue5((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue5(Handle handle, out TValue5 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue6((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue6(Handle handle, out TValue6 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue7((int)_handles[handle.Index].Index);
        return true;
    }

    public bool Contains(Handle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref Handle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public Span<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
    public Span<TValue6> Values6 => _dense.Values7;
}
public class DenseSlotMap<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<Handle> _handles;
    private MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMap()
    {
        _handles = new FastListStruct<Handle>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public Handle Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Count;
            Handle handle = new Handle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new Handle(handleIndex, 0), value1, value2, value3, value4, value5, value6, value7);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref Handle handleToRecycle = ref _handles[handleIndex];

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
            Handle finalHandle = new Handle(handleIndex, handleToRecycle.Version);
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2, value3, value4, value5, value6, value7);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(Handle handle)
    {
        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref Handle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out Handle handleThatGotMoved))
        {
            ref Handle swappedHandle = ref _handles[handleThatGotMoved.Index];
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

    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue2((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue3((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue3(Handle handle, out TValue3 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue4((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue4(Handle handle, out TValue4 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue5((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue5(Handle handle, out TValue5 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue6((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue6(Handle handle, out TValue6 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue7((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue7(Handle handle, out TValue7 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue8((int)_handles[handle.Index].Index);
        return true;
    }

    public bool Contains(Handle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref Handle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public Span<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
    public Span<TValue6> Values6 => _dense.Values7;
    public Span<TValue7> Values7 => _dense.Values8;
}
public class DenseSlotMap<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<Handle> _handles;
    private MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMap()
    {
        _handles = new FastListStruct<Handle>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public Handle Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Count;
            Handle handle = new Handle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new Handle(handleIndex, 0), value1, value2, value3, value4, value5, value6, value7, value8);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref Handle handleToRecycle = ref _handles[handleIndex];

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
            Handle finalHandle = new Handle(handleIndex, handleToRecycle.Version);
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2, value3, value4, value5, value6, value7, value8);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(Handle handle)
    {
        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref Handle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out Handle handleThatGotMoved))
        {
            ref Handle swappedHandle = ref _handles[handleThatGotMoved.Index];
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

    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue2((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue3((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue3(Handle handle, out TValue3 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue4((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue4(Handle handle, out TValue4 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue5((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue5(Handle handle, out TValue5 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue6((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue6(Handle handle, out TValue6 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue7((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue7(Handle handle, out TValue7 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue8((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue8(Handle handle, out TValue8 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue9((int)_handles[handle.Index].Index);
        return true;
    }

    public bool Contains(Handle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref Handle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public Span<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
    public Span<TValue6> Values6 => _dense.Values7;
    public Span<TValue7> Values7 => _dense.Values8;
    public Span<TValue8> Values8 => _dense.Values9;
}

public struct DenseSlotMapStruct<TValue1>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<Handle> _handles;
    private MultiArrayStruct<Handle, TValue1> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMapStruct()
    {
        _handles = new FastListStruct<Handle>();
        _dense = new MultiArrayStruct<Handle, TValue1>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public Handle Add(TValue1 value1)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Count;
            Handle handle = new Handle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new Handle(handleIndex, 0), value1);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref Handle handleToRecycle = ref _handles[handleIndex];

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
            Handle finalHandle = new Handle(handleIndex, handleToRecycle.Version);
            uint denseIndex = (uint)_dense.Add(finalHandle, value1);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(Handle handle)
    {
        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref Handle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out Handle handleThatGotMoved))
        {
            ref Handle swappedHandle = ref _handles[handleThatGotMoved.Index];
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

    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue2((int)_handles[handle.Index].Index);
        return true;
    }

    public bool Contains(Handle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref Handle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public Span<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
}
public struct DenseSlotMapStruct<TValue1, TValue2>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<Handle> _handles;
    private MultiArrayStruct<Handle, TValue1, TValue2> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMapStruct()
    {
        _handles = new FastListStruct<Handle>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public Handle Add(TValue1 value1, TValue2 value2)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Count;
            Handle handle = new Handle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new Handle(handleIndex, 0), value1, value2);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref Handle handleToRecycle = ref _handles[handleIndex];

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
            Handle finalHandle = new Handle(handleIndex, handleToRecycle.Version);
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(Handle handle)
    {
        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref Handle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out Handle handleThatGotMoved))
        {
            ref Handle swappedHandle = ref _handles[handleThatGotMoved.Index];
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

    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue2((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue3((int)_handles[handle.Index].Index);
        return true;
    }

    public bool Contains(Handle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref Handle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public Span<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
}
public struct DenseSlotMapStruct<TValue1, TValue2, TValue3>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<Handle> _handles;
    private MultiArrayStruct<Handle, TValue1, TValue2, TValue3> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMapStruct()
    {
        _handles = new FastListStruct<Handle>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2, TValue3>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public Handle Add(TValue1 value1, TValue2 value2, TValue3 value3)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Count;
            Handle handle = new Handle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new Handle(handleIndex, 0), value1, value2, value3);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref Handle handleToRecycle = ref _handles[handleIndex];

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
            Handle finalHandle = new Handle(handleIndex, handleToRecycle.Version);
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2, value3);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(Handle handle)
    {
        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref Handle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out Handle handleThatGotMoved))
        {
            ref Handle swappedHandle = ref _handles[handleThatGotMoved.Index];
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

    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue2((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue3((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue3(Handle handle, out TValue3 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue4((int)_handles[handle.Index].Index);
        return true;
    }

    public bool Contains(Handle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref Handle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public Span<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
}
public struct DenseSlotMapStruct<TValue1, TValue2, TValue3, TValue4>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<Handle> _handles;
    private MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMapStruct()
    {
        _handles = new FastListStruct<Handle>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public Handle Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Count;
            Handle handle = new Handle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new Handle(handleIndex, 0), value1, value2, value3, value4);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref Handle handleToRecycle = ref _handles[handleIndex];

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
            Handle finalHandle = new Handle(handleIndex, handleToRecycle.Version);
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2, value3, value4);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(Handle handle)
    {
        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref Handle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out Handle handleThatGotMoved))
        {
            ref Handle swappedHandle = ref _handles[handleThatGotMoved.Index];
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

    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue2((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue3((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue3(Handle handle, out TValue3 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue4((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue4(Handle handle, out TValue4 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue5((int)_handles[handle.Index].Index);
        return true;
    }

    public bool Contains(Handle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref Handle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public Span<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
}
public struct DenseSlotMapStruct<TValue1, TValue2, TValue3, TValue4, TValue5>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<Handle> _handles;
    private MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMapStruct()
    {
        _handles = new FastListStruct<Handle>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public Handle Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Count;
            Handle handle = new Handle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new Handle(handleIndex, 0), value1, value2, value3, value4, value5);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref Handle handleToRecycle = ref _handles[handleIndex];

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
            Handle finalHandle = new Handle(handleIndex, handleToRecycle.Version);
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2, value3, value4, value5);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(Handle handle)
    {
        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref Handle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out Handle handleThatGotMoved))
        {
            ref Handle swappedHandle = ref _handles[handleThatGotMoved.Index];
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

    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue2((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue3((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue3(Handle handle, out TValue3 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue4((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue4(Handle handle, out TValue4 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue5((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue5(Handle handle, out TValue5 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue6((int)_handles[handle.Index].Index);
        return true;
    }

    public bool Contains(Handle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref Handle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public Span<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
}
public struct DenseSlotMapStruct<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<Handle> _handles;
    private MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMapStruct()
    {
        _handles = new FastListStruct<Handle>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public Handle Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Count;
            Handle handle = new Handle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new Handle(handleIndex, 0), value1, value2, value3, value4, value5, value6);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref Handle handleToRecycle = ref _handles[handleIndex];

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
            Handle finalHandle = new Handle(handleIndex, handleToRecycle.Version);
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2, value3, value4, value5, value6);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(Handle handle)
    {
        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref Handle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out Handle handleThatGotMoved))
        {
            ref Handle swappedHandle = ref _handles[handleThatGotMoved.Index];
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

    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue2((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue3((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue3(Handle handle, out TValue3 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue4((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue4(Handle handle, out TValue4 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue5((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue5(Handle handle, out TValue5 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue6((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue6(Handle handle, out TValue6 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue7((int)_handles[handle.Index].Index);
        return true;
    }

    public bool Contains(Handle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref Handle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public Span<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
    public Span<TValue6> Values6 => _dense.Values7;
}
public struct DenseSlotMapStruct<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<Handle> _handles;
    private MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMapStruct()
    {
        _handles = new FastListStruct<Handle>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public Handle Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Count;
            Handle handle = new Handle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new Handle(handleIndex, 0), value1, value2, value3, value4, value5, value6, value7);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref Handle handleToRecycle = ref _handles[handleIndex];

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
            Handle finalHandle = new Handle(handleIndex, handleToRecycle.Version);
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2, value3, value4, value5, value6, value7);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(Handle handle)
    {
        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref Handle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out Handle handleThatGotMoved))
        {
            ref Handle swappedHandle = ref _handles[handleThatGotMoved.Index];
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

    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue2((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue3((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue3(Handle handle, out TValue3 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue4((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue4(Handle handle, out TValue4 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue5((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue5(Handle handle, out TValue5 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue6((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue6(Handle handle, out TValue6 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue7((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue7(Handle handle, out TValue7 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue8((int)_handles[handle.Index].Index);
        return true;
    }

    public bool Contains(Handle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref Handle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public Span<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
    public Span<TValue6> Values6 => _dense.Values7;
    public Span<TValue7> Values7 => _dense.Values8;
}
public struct DenseSlotMapStruct<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<Handle> _handles;
    private MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMapStruct()
    {
        _handles = new FastListStruct<Handle>();
        _dense = new MultiArrayStruct<Handle, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public Handle Add(TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Count;
            Handle handle = new Handle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new Handle(handleIndex, 0), value1, value2, value3, value4, value5, value6, value7, value8);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref Handle handleToRecycle = ref _handles[handleIndex];

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
            Handle finalHandle = new Handle(handleIndex, handleToRecycle.Version);
            uint denseIndex = (uint)_dense.Add(finalHandle, value1, value2, value3, value4, value5, value6, value7, value8);

            handleToRecycle = handleToRecycle with { Index = denseIndex};

            return finalHandle;
        }
    }

    public bool Remove(Handle handle)
    {
        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
        {
            return false;
        }

        // Check if handle is current (not stale)
        ref Handle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out Handle handleThatGotMoved))
        {
            ref Handle swappedHandle = ref _handles[handleThatGotMoved.Index];
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

    
    public bool TryGetValue1(Handle handle, out TValue1 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue2((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue2(Handle handle, out TValue2 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue3((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue3(Handle handle, out TValue3 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue4((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue4(Handle handle, out TValue4 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue5((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue5(Handle handle, out TValue5 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue6((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue6(Handle handle, out TValue6 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue7((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue7(Handle handle, out TValue7 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue8((int)_handles[handle.Index].Index);
        return true;
    }
    public bool TryGetValue8(Handle handle, out TValue8 value)
    {
        if (!Contains(handle))
        {
            value = default;
            return false;
        }
        
        value = _dense.GetValue9((int)_handles[handle.Index].Index);
        return true;
    }

    public bool Contains(Handle handle)
    {
        if (handle.Index >= _handles.Length)
        {
            return false;
        }
        
        ref Handle storedHandle = ref _handles[handle.Index];
        return storedHandle.Version == handle.Version;
    }

    public Span<Handle> Handles => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
    public Span<TValue6> Values6 => _dense.Values7;
    public Span<TValue7> Values7 => _dense.Values8;
    public Span<TValue8> Values8 => _dense.Values9;
}

