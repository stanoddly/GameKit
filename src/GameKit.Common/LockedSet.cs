namespace GameKit.Common;

internal struct LockedSet<T>
{
    private readonly Lock _lock = new();
    private readonly HashSet<T> _set = new();

    public LockedSet() { }

    public void Add(T item)
    {
        lock (_lock)
        {
            _set.Add(item);
        }
    }

    public void Remove(T item)
    {
        lock (_lock)
        {
            _set.Remove(item);
        }
    }

    public T[] ClearAndCopy()
    {
        lock (_lock)
        {
            T[] copy = _set.ToArray();
            _set.Clear();
            return copy;
        }
    }

    public T[] Copy()
    {
        lock (_lock)
        {
            return _set.ToArray();
        }
    }
}
