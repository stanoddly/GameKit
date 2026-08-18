namespace Pixely.Pencuil;

public interface IPencuilView
{
    ViewScope ViewScope => default;
    bool ConsumeDirty();
    void Build(Pencil pencil);
}
