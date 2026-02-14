namespace GameKit.Pencuil;

public abstract class StatefulGuiCanvas<TState> : GuiCanvas
{
    public State<TState> State { get; }

    public override bool IsDirty => State.IsDirty;

    internal override void ClearDirty() => State.IsDirty = false;

    protected StatefulGuiCanvas(State<TState> state)
    {
        State = state;
    }
}
