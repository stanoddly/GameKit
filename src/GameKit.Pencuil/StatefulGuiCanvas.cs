namespace GameKit.Pencuil;

public abstract class StatefulGuiCanvas : IGuiCanvas
{
    private readonly IGuiCanvasState _state;

    public bool ConsumeDirty()
    {
        if (!_state.IsDirty)
        {
            return false;
        }

        _state.IsDirty = false;
        return true;
    }

    protected StatefulGuiCanvas(IGuiCanvasState state)
    {
        _state = state;
    }

    public abstract void Build(Pencil pencil);
}
