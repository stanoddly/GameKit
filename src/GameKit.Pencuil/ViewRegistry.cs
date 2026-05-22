namespace GameKit.Pencuil;

public class ViewRegistry
{
    private readonly List<IView?> _views = new();
    private bool _dirty;

    public IReadOnlyList<IView?> Views => _views;

    public void Add(IView view)
    {
        for (int i = 0; i < _views.Count; i++)
        {
            if (ReferenceEquals(_views[i], view))
            {
                return;
            }
        }

        _views.Add(view);
        _dirty = true;
    }

    public void Remove(IView view)
    {
        for (int i = 0; i < _views.Count; i++)
        {
            if (ReferenceEquals(_views[i], view))
            {
                _views[i] = null;
                _dirty = true;
                return;
            }
        }
    }

    public bool ConsumeDirty()
    {
        if (_dirty)
        {
            _dirty = false;
            return true;
        }

        return false;
    }

    public void Compact()
    {
        for (int i = _views.Count - 1; i >= 0; i--)
        {
            if (_views[i] == null)
            {
                _views.RemoveAt(i);
            }
        }
    }
}
