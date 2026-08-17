using System.Runtime.InteropServices;
using GameKit.DependencyInjection;

namespace GameKit.Pencuil;

internal sealed class Pencuil : IViewScoped
{
    private readonly List<IPencuilView> _views = new();
    private readonly List<IPencuilView> _serviceViews = new();
    private readonly List<IPencuilView> _componentViews = new();
    private ulong _serviceViewsVersion = ulong.MaxValue;
    private bool _componentViewsChanged;

    internal Pencil Pencil { get; }
    internal ReadOnlySpan<IPencuilView> Views => CollectionsMarshal.AsSpan(_views);

    public ViewScope ViewScope { get; }

    internal Pencuil(ViewScope viewScope, Pencil pencil)
    {
        ViewScope = viewScope;
        Pencil = pencil;
    }

    internal static Pencuil GetRequired(ServiceProvider provider, ViewScope viewScope)
    {
        _ = provider.GetServices<Pencuil>();
        ServiceRegistry<Pencuil> registry = provider.GetRequiredService<ServiceRegistry<Pencuil>>();
        return GetRequired(registry, viewScope);
    }

    internal static Pencuil GetRequired(
        ServiceRegistry<Pencuil> registry,
        ViewScope viewScope)
    {
        Pencuil? result = null;

        foreach (Pencuil pencuil in registry)
        {
            if (pencuil.ViewScope != viewScope)
            {
                continue;
            }

            if (result != null)
            {
                throw new InvalidOperationException(
                    $"Pencuil is configured more than once for ViewScope {viewScope.Value}.");
            }

            result = pencuil;
        }

        return result ?? throw new InvalidOperationException(
            $"Pencuil is not configured for ViewScope {viewScope.Value}.");
    }

    internal void AddComponentView(IPencuilView view)
    {
        if (view.ViewScope != ViewScope)
        {
            throw new InvalidOperationException(
                $"A Pencuil view for ViewScope {view.ViewScope.Value} cannot be registered " +
                $"with ViewScope {ViewScope.Value}.");
        }

        if (ContainsReference(_componentViews, view))
        {
            return;
        }

        _componentViews.Add(view);
        _componentViewsChanged = true;
    }

    internal void RemoveComponentView(IPencuilView view)
    {
        for (int i = 0; i < _componentViews.Count; i++)
        {
            if (!ReferenceEquals(_componentViews[i], view))
            {
                continue;
            }

            _componentViews.RemoveAt(i);
            _componentViewsChanged = true;
            return;
        }
    }

    internal bool SynchronizeViews(ServiceRegistry<IPencuilView> serviceViews)
    {
        ServiceRegistry<IPencuilView>.Enumerator enumerator = serviceViews.GetEnumerator();
        ulong serviceViewsVersion = serviceViews.Version;
        if (_serviceViewsVersion == serviceViewsVersion && !_componentViewsChanged)
        {
            enumerator.Dispose();
            return false;
        }

        if (_serviceViewsVersion != serviceViewsVersion)
        {
            _serviceViews.Clear();
            try
            {
                while (enumerator.MoveNext())
                {
                    IPencuilView view = enumerator.Current;
                    if (view.ViewScope == ViewScope)
                    {
                        _serviceViews.Add(view);
                    }
                }
            }
            finally
            {
                enumerator.Dispose();
            }
        }
        else
        {
            enumerator.Dispose();
        }

        _serviceViewsVersion = serviceViewsVersion;
        _componentViewsChanged = false;
        bool changed = false;

        for (int i = _views.Count - 1; i >= 0; i--)
        {
            IPencuilView view = _views[i];
            if (ContainsReference(_componentViews, view) || ContainsReference(_serviceViews, view))
            {
                continue;
            }

            _views.RemoveAt(i);
            changed = true;
        }

        foreach (IPencuilView view in _serviceViews)
        {
            if (!ContainsReference(_views, view))
            {
                _views.Add(view);
                changed = true;
            }
        }

        foreach (IPencuilView view in _componentViews)
        {
            if (!ContainsReference(_views, view))
            {
                _views.Add(view);
                changed = true;
            }
        }

        return changed;
    }

    private static bool ContainsReference(List<IPencuilView> views, IPencuilView target)
    {
        for (int i = 0; i < views.Count; i++)
        {
            if (ReferenceEquals(views[i], target))
            {
                return true;
            }
        }

        return false;
    }
}
