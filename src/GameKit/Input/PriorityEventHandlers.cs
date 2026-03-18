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
