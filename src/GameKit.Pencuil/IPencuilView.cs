namespace GameKit.Pencuil;

public interface IPencuilView : IViewScoped
{
    bool ConsumeDirty();
    void Build(Pencil pencil);
}
