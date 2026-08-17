namespace GameKit.Input;

internal sealed class PriorityEventHandlers<TEventArgs>
    where TEventArgs : ConsumableInputEventArgs
{
    private readonly List<(int Priority, InputEventHandler<TEventArgs> Handler)> _handlers = new();
    private bool _dirty;

    public void Add(int priority, InputEventHandler<TEventArgs> handler)
    {
        _handlers.Add((priority, handler));
        _dirty = true;
    }

    public void Remove(InputEventHandler<TEventArgs> handler)
    {
        _handlers.RemoveAll(entry => entry.Handler == handler);
    }

    public void Invoke(TEventArgs eventArgs)
    {
        if (_dirty)
        {
            _handlers.Sort(static (left, right) => left.Priority.CompareTo(right.Priority));
            _dirty = false;
        }

        eventArgs.Consumed = false;

        foreach ((_, InputEventHandler<TEventArgs> handler) in _handlers)
        {
            handler(eventArgs);

            if (eventArgs.Consumed)
            {
                break;
            }
        }
    }
}

internal sealed class ViewScopedPriorityEventHandlers<TEventArgs>
    where TEventArgs : ConsumableInputEventArgs
{
    private readonly List<(
        ViewScope ViewScope,
        int Priority,
        InputEventHandler<TEventArgs> Handler)> _handlers = new();
    private bool _dirty;

    public void Add(
        ViewScope viewScope,
        int priority,
        InputEventHandler<TEventArgs> handler)
    {
        _handlers.Add((viewScope, priority, handler));
        _dirty = true;
    }

    public void Remove(
        ViewScope viewScope,
        InputEventHandler<TEventArgs> handler)
    {
        _handlers.RemoveAll(entry =>
            entry.ViewScope == viewScope && entry.Handler == handler);
    }

    public void Invoke(
        ViewScope viewScope,
        TEventArgs eventArgs)
    {
        if (_dirty)
        {
            _handlers.Sort(static (left, right) => left.Priority.CompareTo(right.Priority));
            _dirty = false;
        }

        eventArgs.Consumed = false;

        foreach ((ViewScope registeredViewScope, _, InputEventHandler<TEventArgs> handler) in _handlers)
        {
            if (registeredViewScope != viewScope)
            {
                continue;
            }

            handler(eventArgs);

            if (eventArgs.Consumed)
            {
                break;
            }
        }
    }
}
