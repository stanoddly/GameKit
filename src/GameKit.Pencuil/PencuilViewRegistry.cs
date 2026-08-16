using System.Runtime.InteropServices;

namespace GameKit.Pencuil;

public sealed class PencuilViewRegistry : IViewScoped
{
    private readonly List<IPencuilView> _views = new();
    private bool _dirty;

    public ViewScope ViewScope { get; }
    public ReadOnlySpan<IPencuilView> Views => CollectionsMarshal.AsSpan(_views);

    public PencuilViewRegistry(ViewScope viewScope)
    {
        ViewScope = viewScope;
    }

    public void Add(IPencuilView view)
    {
        if (view.ViewScope != ViewScope)
        {
            throw new InvalidOperationException(
                $"A Pencuil view for ViewScope {view.ViewScope.Value} cannot be registered " +
                $"with ViewScope {ViewScope.Value}.");
        }

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

    public void Remove(IPencuilView view)
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
