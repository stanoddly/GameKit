namespace GameKit.Pencuil;

public abstract class StatefulGuiCanvas : GuiCanvas
{
    public bool IsDirty { get; private set; } = true;

    public void Invalidate() => IsDirty = true;

    internal void ClearDirty() => IsDirty = false;

    protected State<TValue> CreateState<TValue>(TValue initialValue)
    {
        return new State<TValue>(initialValue, this);
    }
}
