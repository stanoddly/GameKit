namespace GameKit.Pencuil;

public class State<TValue>
{
    private TValue _value;
    private readonly StatefulGuiCanvas _canvas;

    internal State(TValue value, StatefulGuiCanvas canvas)
    {
        _value = value;
        _canvas = canvas;
    }

    public TValue Value
    {
        get => _value;
        set
        {
            if (!EqualityComparer<TValue>.Default.Equals(_value, value))
            {
                _value = value;
                _canvas.Invalidate();
            }
        }
    }
}
