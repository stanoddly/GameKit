using GameKit.Componentize;

namespace GameKit.Pencuil;

public abstract class GuiCanvasComponent<TState> : GameComponent, IGuiCanvas where TState : IGuiCanvasState
{
    protected TState State { get; }

    protected GuiCanvasComponent(TState state)
    {
        State = state;
    }

    public bool ConsumeDirty()
    {
        if (!State.IsDirty)
        {
            return false;
        }

        State.IsDirty = false;
        return true;
    }

    public abstract void Build(Pencil pencil);

    protected override void OnAttach()
    {
        Services<GuiCanvasRegistry>.Instance.Add(this);
    }

    protected override void OnDetach()
    {
        Services<GuiCanvasRegistry>.Instance.Remove(this);
    }
}
