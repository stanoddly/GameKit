namespace GameKit.Collections;

#nullable disable
public struct SlotMapStruct<TType>
{
    private FastListStruct<TType> _data = new();

    public SlotMapStruct()
    {
    }

    public void Set(Handle handle, TType value)
    {
        int index = handle;
        if (_data.LastIndex < index)
        {
            _data.ResizeFill(index + 1, default(TType));
        }
        _data[index] = value;
    }
    
    public TType SetNewGetPrevious(Handle handle, TType value)
    {
        int index = handle;
        if (_data.LastIndex < index)
        {
            _data.ResizeFill(index + 1, default(TType));
        }

        ref TType currentValue = ref _data[index];
        TType previousValue = currentValue;
        currentValue = value;
        return previousValue;
    }

    public ref TType GetRef(Handle handle)
    {
        int index = handle;
        if (_data.LastIndex < index)
        {
            _data.ResizeFill(index + 1, default(TType));
        }
        return ref _data[index];
    }
    
    public TType Get(Handle handle)
    {
        int index = handle;
        if (index < _data.Length)
        {
            return _data[index];
        }

        return default;
    }
}