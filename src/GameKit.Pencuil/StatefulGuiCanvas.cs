namespace GameKit.Pencuil;

public abstract class StatefulGuiCanvas<TState> : IGuiCanvas
{
    protected State<TState> State { get; }

    public bool ConsumeDirty() => State.ConsumeDirty();

    protected StatefulGuiCanvas(State<TState> state)
    {
        State = state;
    }

    public abstract void Build(Pencil pencil);
}
