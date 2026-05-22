using System.Runtime.InteropServices;

namespace GameKit.Pencuil;

public class ViewRegistry
{
    private readonly List<IView> _views = new();
    private bool _dirty;

    public ReadOnlySpan<IView> Views => CollectionsMarshal.AsSpan(_views);

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
                _views.RemoveAt(i);
                _dirty = true;
                return;
            }
        }
    }

    public bool ConsumeDirty()
    {
        bool dirty = _dirty;
        _dirty = false;
        return dirty;
    }

}
