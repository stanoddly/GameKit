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
        if (_freeIndex != Tombstone)
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
        else
        {
            uint index = (uint)_dense.Count;
            Handle handle = new Handle { Index = index, Version = 0 };
            uint handleIndex = (uint)_handles.Add(handle);

            _dense.Add(new Handle(handleIndex, 0), value);

            return handle;
        }
    }

    public bool Remove(Handle handle)
    {
        // Validate the handle points to a valid slot
        if (handle.Index >= _handles.Length)
            return false;
        
        // Check if handle is current (not stale)
        ref Handle storedHandle = ref _handles[handle.Index];
        if (storedHandle.Version != handle.Version)
        {
            return false;
        }

        // Remove from dense storage, swapping with last element if needed    
        if (!_dense.SwapRemoveReturnFirst((int)storedHandle.Index, out Handle lastHandleThatGotSwapped))
        {
            return false;
        }
        
        ref Handle swappedHandle = ref _handles[lastHandleThatGotSwapped.Index];
        swappedHandle = swappedHandle with { Index = storedHandle.Index };

        // Increment version to invalidate existing handles
        storedHandle = storedHandle with 
        { 
            Version = handle.Version + 1,
            Index = _freeIndex // Store previous free index
        };

        // Update free list
        if (_freeIndex == Tombstone)
        {
            _lastFreeIndex = handle.Index;
        }
            
        _freeIndex = handle.Index;

        return true;
    }

    public Span<Handle> Handles => _dense.Values1;
    public Span<TValue> Values => _dense.Values2;
}