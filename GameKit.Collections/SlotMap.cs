namespace GameKit.Collections;

public class SlotMap<THandle, TValue>
    where THandle : unmanaged, IHandle<THandle>
    where TValue: struct
{
    private readonly FastListStruct<TValue> _values = new();
    private readonly Dictionary<uint, int> _handleToIndexLookup = new();
    private readonly Dictionary<int, uint> _indexToHandleLookup = new();
    private uint _handleCounter = 0;

    public THandle Add(in TValue value)
    {
        int index = _values.Add(value);

        uint handle = ++_handleCounter;
        _handleToIndexLookup[handle] = index;
        _indexToHandleLookup[index] = handle;
        return (THandle)handle;
    }

    public void RemoveValue(THandle handle)
    {
        uint rawHandle = (uint)handle;
        if (!_handleToIndexLookup.Remove(rawHandle, out int index)) return;

        if (_values.SwapRemove(index))
        {
            int movedIndex = _values.LastIndex;
            if (_indexToHandleLookup.Remove(movedIndex, out uint movedHandle))
            {
                _indexToHandleLookup[index] = movedHandle;
                _handleToIndexLookup[movedHandle] = index;
            }
        }
        else
        {
            _indexToHandleLookup.Remove(index);
        }
    }

    public bool TryGetValue(THandle handle, out TValue value)
    {
        if (!_handleToIndexLookup.TryGetValue(handle, out int index))
        {
            value = default;
            return false;
        }
        
        value = _values[index];
        return true;
    }

    public Span<TValue> Values => _values.AsSpan();
    public ReadOnlySpan<TValue> ReadOnlyValues => _values.AsReadOnlySpan();

    public ref TValue this[THandle handle]
    {
        get
        {
            if (_handleToIndexLookup.TryGetValue((uint)handle, out int index))
            {
                return ref _values[index];
            }

            throw new ArgumentException("handle was not found", nameof(handle));
        }
    }
}