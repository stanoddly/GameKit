namespace GameKit.Pencuil;

public class ViewRegistry
{
    private readonly List<IView> _views;
    private readonly Pencil _pencil;

    public ViewRegistry(Pencil pencil, IEnumerable<IView> views)
    {
        _pencil = pencil;
        _views = [..views];
    }

    public IReadOnlyList<IView> Views => _views;

    public void Add(IView view)
    {
        _views.Add(view);
        _pencil.Invalidate();
    }

    public void Remove(IView view)
    {
        _views.Remove(view);
        _pencil.Invalidate();
    }
}
