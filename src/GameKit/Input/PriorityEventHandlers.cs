namespace GameKit.Input;

internal class PriorityEventHandlers<TDelegate> where TDelegate : Delegate
{
    private readonly List<(int Priority, TDelegate Handler)> _handlers = new();
    private bool _dirty;

    public void Add(int priority, TDelegate handler)
    {
        _handlers.Add((priority, handler));
        _dirty = true;
    }

    public void Remove(TDelegate handler)
    {
        _handlers.RemoveAll(entry => entry.Handler == handler);
    }

    public ReadOnlySpan<(int Priority, TDelegate Handler)> GetSorted()
    {
        if (_dirty)
        {
            _handlers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            _dirty = false;
        }

        return System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_handlers);
    }
}

internal sealed class ViewScopedPriorityEventHandlers<TDelegate> where TDelegate : Delegate
{
    private readonly List<(ViewScope ViewScope, PriorityEventHandlers<TDelegate> Handlers)> _handlers = new();

    public void Add(ViewScope viewScope, int priority, TDelegate handler)
    {
        foreach ((ViewScope registeredViewScope, PriorityEventHandlers<TDelegate> handlers) in _handlers)
        {
            if (registeredViewScope == viewScope)
            {
                handlers.Add(priority, handler);
                return;
            }
        }

        PriorityEventHandlers<TDelegate> newHandlers = new();
        newHandlers.Add(priority, handler);
        _handlers.Add((viewScope, newHandlers));
    }

    public void Remove(ViewScope viewScope, TDelegate handler)
    {
        foreach ((ViewScope registeredViewScope, PriorityEventHandlers<TDelegate> handlers) in _handlers)
        {
            if (registeredViewScope == viewScope)
            {
                handlers.Remove(handler);
                return;
            }
        }
    }

    public ReadOnlySpan<(int Priority, TDelegate Handler)> GetSorted(ViewScope viewScope)
    {
        foreach ((ViewScope registeredViewScope, PriorityEventHandlers<TDelegate> handlers) in _handlers)
        {
            if (registeredViewScope == viewScope)
            {
                return handlers.GetSorted();
            }
        }

        return ReadOnlySpan<(int Priority, TDelegate Handler)>.Empty;
    }
}
