namespace GameKit.Gpu;

internal class MemoryTrackedSet<T> where T : IGpuMemorySized
{
    private readonly Lock _lock = new();
    private readonly HashSet<T> _set = new();
    private long _totalBytes;

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _set.Count;
            }
        }
    }

    public long TotalBytes
    {
        get
        {
            lock (_lock)
            {
                return _totalBytes;
            }
        }
    }

    public void Add(T item)
    {
        lock (_lock)
        {
            if (_set.Add(item))
            {
                _totalBytes += item.SizeInBytes;
            }
        }
    }

    public void Remove(T item)
    {
        lock (_lock)
        {
            if (_set.Remove(item))
            {
                _totalBytes -= item.SizeInBytes;
            }
        }
    }

    public T[] ClearAndCopy()
    {
        lock (_lock)
        {
            T[] copy = _set.ToArray();
            _set.Clear();
            _totalBytes = 0;
            return copy;
        }
    }
}
