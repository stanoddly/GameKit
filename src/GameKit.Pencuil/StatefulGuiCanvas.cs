namespace GameKit.Pencuil;

public abstract class StatefulGuiCanvas<TState> : IGuiCanvas where TState : IGuiCanvasState
{
    protected TState State { get; }

    public bool ConsumeDirty()
    {
        if (!State.IsDirty)
        {
            return false;
        }

        State.IsDirty = false;
        return true;
    }

    protected StatefulGuiCanvas(TState state)
    {
        State = state;
    }

    public abstract void Build(Pencil pencil);
}
