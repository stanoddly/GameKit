namespace GameKit.Pencuil;

public abstract class GuiCanvas
{
    public virtual bool IsDirty => false;

    internal virtual void ClearDirty() { }

    public abstract void Build(Pencil pencil);
}
