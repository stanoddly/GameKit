namespace GameKit.Collections;

public class DenseSlotMap<TValue>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<Handle> _handles;
    private MultiArrayStruct<Handle, TValue> _dense;
    private uint _freeIndex;
    private uint _lastFreeIndex;

    public DenseSlotMap()
    {
        _handles = new FastListStruct<Handle>();
        _dense = new MultiArrayStruct<Handle, TValue>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
    }

    public Handle Add(TValue value)
    {
        // If there is nothing to recycle create new
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_dense.Count;
            Handle handle = new Handle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new Handle(handleIndex, 0), value);

            return handle;
        }
        // Recycle
        else
        {
            var handleIndex = _freeIndex;
            ref Handle handle = ref _handles[handleIndex];

            bool isItTheOnlyFreeIndex = _freeIndex == _lastFreeIndex;
            if (isItTheOnlyFreeIndex)
            {
                _freeIndex = Tombstone;
                _lastFreeIndex = Tombstone;
            }
            else
            {
                _freeIndex = handle.Index;
            }
            
            // Note the version has been incremented on removal too, so it's free to use already
            uint denseIndex = (uint)_dense.Add(new Handle(handleIndex, handle.Version), value);

            handle = handle with { Index = denseIndex};

            return handle;
        }
    }

    public bool Remove(Handle handle)
    {
        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
            return false;
        
        // Check if handle is current (not stale)
        ref Handle handleToBeDeleted = ref _handles[handle.Index];
        if (handleToBeDeleted.Version != handle.Version)
        {
            return false;
        }
        
        // Remove from dense storage, swapping with last element if needed
        if (_dense.SwapRemove((int)handleToBeDeleted.Index))
        {
            int swappedIndex = _dense.Count;
            ref Handle swappedHandle = ref _handles[lastHandleThatGotSwapped.Index];
            swappedHandle = swappedHandle with { Index = handleToBeDeleted.Index };
        }
        if (_dense.SwapRemoveReturnFirst((int)handleToBeDeleted.Index, out Handle lastHandleThatGotSwapped))
        {
            ref Handle swappedHandle = ref _handles[lastHandleThatGotSwapped.Index];
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
    
    public bool TryGetValue(Handle handle, out TValue value)
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
    public Span<TValue> Values => _dense.Values2;
}
