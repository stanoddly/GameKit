namespace GameKit.Pencuil;

public interface IGuiCanvas
{
    bool ConsumeDirty();
    void Build(Pencil pencil);
}
