namespace GameKit.Collections;

#nullable disable
public struct SlotMapStruct<THandle>
    where THandle : struct, IHandle<THandle>
{
    private const uint Tombstone = uint.MaxValue;
    private FastListStruct<THandle> _slots;
    private uint _freeIndex;
    private uint _lastFreeIndex;
    private int _count;

    public SlotMapStruct()
    {
        _slots = new FastListStruct<THandle>();
        _freeIndex = Tombstone;
        _lastFreeIndex = Tombstone;
        _count = 0;
    }

    public int Count => _count;

    public THandle CreateHandle()
    {
        if (_freeIndex == Tombstone)
        {
            uint index = (uint)_slots.Length;
            THandle handle = new THandle { Index = index, Version = 0 };
            _slots.Add(handle);
            _count++;
            return handle;
        }
        else
        {
            var slotIndex = _freeIndex;
            ref THandle slotToRecycle = ref _slots[slotIndex];

            if (_freeIndex == _lastFreeIndex)
            {
                _freeIndex = Tombstone;
                _lastFreeIndex = Tombstone;
            }
            else
            {
                _freeIndex = slotToRecycle.Index;
            }

            THandle handle = new THandle { Index = slotIndex, Version = slotToRecycle.Version };
            slotToRecycle = handle;
            _count++;
            return handle;
        }
    }

    public bool Contains(THandle handle)
    {
        if (handle.IsNull())
        {
            return false;
        }

        if (handle.Index >= _slots.Length)
        {
            return false;
        }

        ref THandle stored = ref _slots[handle.Index];
        return stored.Version == handle.Version;
    }

    public bool Remove(THandle handle)
    {
        if (handle.IsNull())
        {
            return false;
        }

        if (handle.Index >= _slots.Length)
        {
            return false;
        }

        ref THandle slot = ref _slots[handle.Index];
        if (slot.Version != handle.Version)
        {
            return false;
        }

        slot = slot with
        {
            Version = handle.Version + 1,
            Index = _freeIndex
        };

        if (_freeIndex == Tombstone)
        {
            _lastFreeIndex = handle.Index;
        }

        _freeIndex = handle.Index;
        _count--;
        return true;
    }
}
