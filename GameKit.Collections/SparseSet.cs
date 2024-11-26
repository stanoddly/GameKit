namespace GameKit.Collections;



public enum SwapRemoveResult
{
    Swap, NoSwap, Absent, DifferentVersion
}

public class SparseSet<TKey> where TKey: IKey<TKey>
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
        return Contains(key, out _);
    }
    
    public bool Contains(TKey key, out int keyIndex) 
    {
        int index = key.Index;
        if (index > _sparse.LastIndex)
        {
            keyIndex = default;
            return false;
        }
        
        var denseKey = _sparse[index];

        if (denseKey.IsTombStone())
        {
            keyIndex = default;
            return false;
        }
        
        if (key.Version != denseKey.Version)
        {
            keyIndex = default;
            return false;
        }

        keyIndex = default;
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


public class BaseSparseSet<TKey>
    where TKey: IKey<TKey>
{
    private FastList<TKey> _sparse;
    private FastList<TKey> _dense;

    public BaseSparseSet()
    {
        _sparse = new FastList<TKey>();
        _dense = new FastList<TKey>();
    }

    protected int SetKey(TKey key)
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
    
    protected SwapRemoveResult SwapRemove(TKey key)
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
            return SwapRemoveResult.Absent;
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
    
    protected SwapRemoveResult SwapRemove(TKey key, out int keysIndex)
    {
        int index = key.Index;
        if (index > (_sparse.Length - 1))
        {
            keysIndex = default;
            return SwapRemoveResult.Absent;
        }

        ref TKey denseKey = ref _sparse[index];

        if (denseKey.IsTombStone())
        {
            keysIndex = default;
            return SwapRemoveResult.Absent;
        }

        // The key isn't here actually, incompatible versions 
        if (key.Version != denseKey.Version)
        {
            keysIndex = default;
            return SwapRemoveResult.Absent;
        }

        keysIndex = denseKey.Index;
        if (_dense.SwapRemove(keysIndex, out TKey? swappedKey))
        {
            ref var swappedSparseKey = ref _sparse[swappedKey.Index];
            swappedSparseKey = denseKey.WithVersion(swappedSparseKey.Version);
            denseKey = TKey.TombStone;
            
            return SwapRemoveResult.Swap;
        }

        denseKey = TKey.TombStone;
        return SwapRemoveResult.NoSwap;
    }
    
    public bool ContainsKey(TKey key)
    {
        return ContainsKey(key, out _);
    }
    
    public bool ContainsKey(TKey key, out int keyIndex) 
    {
        int index = key.Index;
        if (index > _sparse.LastIndex)
        {
            keyIndex = default;
            return false;
        }
        
        var denseKey = _sparse[index];

        if (denseKey.IsTombStone())
        {
            keyIndex = default;
            return false;
        }
        
        if (key.Version != denseKey.Version)
        {
            keyIndex = default;
            return false;
        }

        keyIndex = default;
        return true;
    }
    
    public int GetKeyIndex(TKey key)
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

public class SparseMap<TKey, TValue> : BaseSparseSet<TKey> where TKey: IKey<TKey>
{
    protected FastList<TValue> _values;

    public SparseMap()
    {
        _values = new();
    }

    public int Set(TKey key, in TValue value)
    {
        int index = SetKey(key);

        if (index == _values.Length)
        {
            _values.Add(value);
        }
        else
        {
            _values[index] = value;
        }

        return index;
    }

    public void Remove(TKey key)
    {
        var result = SwapRemove(key, out int index);

        if (result is SwapRemoveResult.NoSwap or SwapRemoveResult.Swap)
        {
            _values.SwapRemove(index, out _);
        }
    }
    
    public Span<TValue> Values => _values.AsSpan();
}