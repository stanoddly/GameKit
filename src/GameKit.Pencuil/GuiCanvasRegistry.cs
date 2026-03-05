namespace GameKit.Pencuil;

public class GuiCanvasRegistry
{
    private readonly List<IGuiCanvas> _canvases;
    private readonly Pencil _pencil;

    public GuiCanvasRegistry(Pencil pencil, IEnumerable<IGuiCanvas> canvases)
    {
        _pencil = pencil;
        _canvases = [..canvases];
    }

    public IReadOnlyList<IGuiCanvas> Canvases => _canvases;

    public void Add(IGuiCanvas canvas)
    {
        _canvases.Add(canvas);
        _pencil.Invalidate();
    }

    public void Remove(IGuiCanvas canvas)
    {
        _canvases.Remove(canvas);
        _pencil.Invalidate();
    }
}
