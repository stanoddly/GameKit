namespace GameKit.Pencuil;

public interface IView
{
    bool ConsumeDirty();
    void Build(Pencil pencil);
}
