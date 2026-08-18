namespace Pixely.Gpu;

internal class MemoryTrackedSet<T> where T : IGpuMemorySized
{
    private readonly Lock _lock = new();
    private readonly HashSet<T> _set = new();
    private int _count;
    private long _totalBytes;

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _count;
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

    public (int Count, long TotalBytes) CountAndTotalBytes
    {
        get
        {
            lock (_lock)
            {
                return (_count, _totalBytes);
            }
        }
    }

    public void Add(T item)
    {
        lock (_lock)
        {
            if (_set.Add(item))
            {
                _count++;
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
                _count--;
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
            _count = 0;
            _totalBytes = 0;
            return copy;
        }
    }
}
