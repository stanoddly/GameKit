namespace GameKit.Utilities;

public interface IKey<TSelf> where TSelf : IKey<TSelf>
{
    int Index { get; }
    int Version { get; }
    static abstract TSelf TombStone { get; }
    bool IsTombStone();
    TSelf WithIndex(int index);
    TSelf WithVersion(int version);
}

public enum SwapRemoveResult
{
    Swap, NoSwap, Absent, DifferentVersion
}

public class SparseSet<TKey>
    where TKey: IKey<TKey>, new()
{
    private FastList<TKey> _sparse;
    private FastList<TKey> _dense;

    public SparseSet()
    {
        _sparse = new FastList<TKey>();
        _dense = new FastList<TKey>();
    }

    public int Set(TKey key)
    {
        int sparseIndex = key.Index;
        int keyVersion = key.Version;
        int denseIndex;

        if (sparseIndex > _sparse.LastIndex)
        {
            _sparse.ResizeFill(sparseIndex + 1, TKey.TombStone);
        }

        ref TKey denseKey = ref _sparse[sparseIndex];
        
        // nonexistent
        if (denseKey.IsTombStone())
        {
            denseIndex = _dense.Add(key);
            denseKey = key.WithIndex(denseIndex);
            return denseIndex;
        }
        
        // already exists with correct version
        if (keyVersion == denseKey.Version)
        {
            return denseKey.Index;
        }
        
        // exists, but needs to change the version
        denseKey = denseKey.WithVersion(keyVersion);
        denseIndex = denseKey.Index;
        _dense[denseIndex] = key;

        return denseIndex;
    }
    
    public SwapRemoveResult SwapRemove(TKey key)
    {
        int index = key.Index;
        if (index > (_sparse.Length - 1))
        {
            return SwapRemoveResult.Absent;
        }

        ref TKey denseKey = ref _sparse[index];

        if (denseKey.IsTombStone())
        {
            return SwapRemoveResult.Absent;
        }

        // The key isn't here actually, incompatible versions 
        if (key.Version != denseKey.Version)
        {
            return SwapRemoveResult.DifferentVersion;
        }
        
        if (_dense.SwapRemove(denseKey.Index, out TKey? swappedKey))
        {
            ref var swappedSparseKey = ref _sparse[swappedKey.Index];
            swappedSparseKey = denseKey.WithVersion(swappedSparseKey.Version);
            denseKey = TKey.TombStone;

            return SwapRemoveResult.Swap;
        }

        denseKey = TKey.TombStone;
        return SwapRemoveResult.NoSwap;
    }
    
    public bool Contains(TKey key) 
    {
        int index = key.Index;
        if (index > _sparse.LastIndex)
        {
            return false;
        }
        
        var denseKey = _sparse[index];

        if (denseKey.IsTombStone())
        {
            return false;
        }
        
        if (key.Version != denseKey.Version)
        {
            return false;
        }

        return true;
    }
    
    public int GetKeysIndex(TKey key)
    {
        int index = key.Index;
        if (index > _sparse.LastIndex)
        {
            return -1;
        }
        
        TKey denseKey = _sparse[index];

        if (denseKey.IsTombStone())
        {
            return -1;
        }
        
        if (denseKey.Version != key.Version)
        {
            return -1;
        }

        return denseKey.Index;
    }

    public Span<TKey> Keys => _dense.AsSpan();
}
