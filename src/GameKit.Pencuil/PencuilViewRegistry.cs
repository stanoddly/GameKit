using GameKit.DependencyInjection;

namespace GameKit.Pencuil;

internal sealed class PencuilViewRegistry
{
    private readonly List<IPencuilView> _views = new();
    private readonly HashSet<ViewScope> _changedViewScopes = new();

    internal static void AddPencuilViewRegistry(ServiceCollection services)
    {
        PencuilViewRegistry registry = new();
        services.AddSingleton(registry);
        services.OnActivated((instance, _) =>
        {
            if (instance is IPencuilView view)
            {
                registry.Add(view);
            }
        });
        services.OnDisposing((instance, _) =>
        {
            if (instance is IPencuilView view)
            {
                registry.Remove(view);
            }
        });
    }

    internal void Add(IPencuilView view)
    {
        for (int i = 0; i < _views.Count; i++)
        {
            if (ReferenceEquals(_views[i], view))
            {
                return;
            }
        }

        _views.Add(view);
        _changedViewScopes.Add(view.ViewScope);
    }

    internal void Remove(IPencuilView view)
    {
        for (int i = 0; i < _views.Count; i++)
        {
            if (!ReferenceEquals(_views[i], view))
            {
                continue;
            }

            _views.RemoveAt(i);
            _changedViewScopes.Add(view.ViewScope);
            return;
        }
    }

    internal bool ConsumeChanged(ViewScope viewScope)
    {
        return _changedViewScopes.Remove(viewScope);
    }

    internal void CopyViews(ViewScope viewScope, List<IPencuilView> destination)
    {
        destination.Clear();
        foreach (IPencuilView view in _views)
        {
            if (view.ViewScope == viewScope)
            {
                destination.Add(view);
            }
        }
    }
}
