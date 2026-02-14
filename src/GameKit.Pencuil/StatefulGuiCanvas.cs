namespace GameKit.Pencuil;

public abstract class StatefulGuiCanvas : GuiCanvas
{
    public bool IsDirty { get; private set; } = true;

    public void Invalidate() => IsDirty = true;

    internal void ClearDirty() => IsDirty = false;
}

public abstract class StatefulGuiCanvas<TState> : StatefulGuiCanvas
{
    private TState _state;

    public TState State
    {
        get => _state;
        set
        {
            if (!EqualityComparer<TState>.Default.Equals(_state, value))
            {
                _state = value;
                Invalidate();
            }
        }
    }

    protected StatefulGuiCanvas(TState initialState)
    {
        _state = initialState;
    }
}
