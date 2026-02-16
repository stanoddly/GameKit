namespace GameKit.Pencuil;

public class State<TValue>
{
    private TValue _value;

    public State(TValue initialValue)
    {
        _value = initialValue;
    }

    internal bool IsDirty = true;

    public bool ConsumeDirty()
    {
        if (!IsDirty)
        {
            return false;
        }

        IsDirty = false;
        return true;
    }

    public TValue Value
    {
        get => _value;
        set
        {
            if (!EqualityComparer<TValue>.Default.Equals(_value, value))
            {
                _value = value;
                IsDirty = true;
            }
        }
    }
}
