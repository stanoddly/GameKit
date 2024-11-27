// Generated using jinja2-cli: jinja2 MultiMap.cs.jinja > MultiMap.cs
namespace GameKit.Collections;
public class MultiMap<TKey, TValue1, TValue2, TValue3> where TKey: IKey<TKey>
{
    private FastListStruct<TKey> _sparse;
    private MultiArrayStruct<TKey, TValue1, TValue2, TValue3> _dense;

    public MultiMap()
    {
        _sparse = new FastListStruct<TKey>();
        _dense = new MultiArrayStruct<TKey, TValue1, TValue2, TValue3>();
    }

    public int Set(TKey key, TValue1 value1, TValue2 value2, TValue3 value3)
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
            denseIndex = _dense.Add(key, value1, value2, value3);
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

        _dense.Set(denseIndex, key, value1, value2, value3);

        return denseIndex;
    }
    
    public bool Remove(TKey key)
    {
        int index = key.Index;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref TKey denseKey = ref _sparse[index];

        if (denseKey.IsTombStone())
        {
            return false;
        }

        // The key isn't here actually, incompatible versions 
        if (key.Version != denseKey.Version)
        {
            return false;
        }
        
        if (_dense.InternalSwapRemove(denseKey.Index, out TKey? swappedKey))
        {
            ref var swappedSparseKey = ref _sparse[swappedKey.Index];
            swappedSparseKey = denseKey.WithVersion(swappedSparseKey.Version);

            return true;
        }

        denseKey = TKey.TombStone;
        return true;
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

    public Span<TKey> Keys => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
}

public class MultiMap<TKey, TValue1, TValue2, TValue3, TValue4> where TKey: IKey<TKey>
{
    private FastListStruct<TKey> _sparse;
    private MultiArrayStruct<TKey, TValue1, TValue2, TValue3, TValue4> _dense;

    public MultiMap()
    {
        _sparse = new FastListStruct<TKey>();
        _dense = new MultiArrayStruct<TKey, TValue1, TValue2, TValue3, TValue4>();
    }

    public int Set(TKey key, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4)
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
            denseIndex = _dense.Add(key, value1, value2, value3, value4);
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

        _dense.Set(denseIndex, key, value1, value2, value3, value4);

        return denseIndex;
    }
    
    public bool Remove(TKey key)
    {
        int index = key.Index;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref TKey denseKey = ref _sparse[index];

        if (denseKey.IsTombStone())
        {
            return false;
        }

        // The key isn't here actually, incompatible versions 
        if (key.Version != denseKey.Version)
        {
            return false;
        }
        
        if (_dense.InternalSwapRemove(denseKey.Index, out TKey? swappedKey))
        {
            ref var swappedSparseKey = ref _sparse[swappedKey.Index];
            swappedSparseKey = denseKey.WithVersion(swappedSparseKey.Version);

            return true;
        }

        denseKey = TKey.TombStone;
        return true;
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

    public Span<TKey> Keys => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
}

public class MultiMap<TKey, TValue1, TValue2, TValue3, TValue4, TValue5> where TKey: IKey<TKey>
{
    private FastListStruct<TKey> _sparse;
    private MultiArrayStruct<TKey, TValue1, TValue2, TValue3, TValue4, TValue5> _dense;

    public MultiMap()
    {
        _sparse = new FastListStruct<TKey>();
        _dense = new MultiArrayStruct<TKey, TValue1, TValue2, TValue3, TValue4, TValue5>();
    }

    public int Set(TKey key, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5)
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
            denseIndex = _dense.Add(key, value1, value2, value3, value4, value5);
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

        _dense.Set(denseIndex, key, value1, value2, value3, value4, value5);

        return denseIndex;
    }
    
    public bool Remove(TKey key)
    {
        int index = key.Index;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref TKey denseKey = ref _sparse[index];

        if (denseKey.IsTombStone())
        {
            return false;
        }

        // The key isn't here actually, incompatible versions 
        if (key.Version != denseKey.Version)
        {
            return false;
        }
        
        if (_dense.InternalSwapRemove(denseKey.Index, out TKey? swappedKey))
        {
            ref var swappedSparseKey = ref _sparse[swappedKey.Index];
            swappedSparseKey = denseKey.WithVersion(swappedSparseKey.Version);

            return true;
        }

        denseKey = TKey.TombStone;
        return true;
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

    public Span<TKey> Keys => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
}

public class MultiMap<TKey, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6> where TKey: IKey<TKey>
{
    private FastListStruct<TKey> _sparse;
    private MultiArrayStruct<TKey, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6> _dense;

    public MultiMap()
    {
        _sparse = new FastListStruct<TKey>();
        _dense = new MultiArrayStruct<TKey, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6>();
    }

    public int Set(TKey key, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6)
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
            denseIndex = _dense.Add(key, value1, value2, value3, value4, value5, value6);
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

        _dense.Set(denseIndex, key, value1, value2, value3, value4, value5, value6);

        return denseIndex;
    }
    
    public bool Remove(TKey key)
    {
        int index = key.Index;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref TKey denseKey = ref _sparse[index];

        if (denseKey.IsTombStone())
        {
            return false;
        }

        // The key isn't here actually, incompatible versions 
        if (key.Version != denseKey.Version)
        {
            return false;
        }
        
        if (_dense.InternalSwapRemove(denseKey.Index, out TKey? swappedKey))
        {
            ref var swappedSparseKey = ref _sparse[swappedKey.Index];
            swappedSparseKey = denseKey.WithVersion(swappedSparseKey.Version);

            return true;
        }

        denseKey = TKey.TombStone;
        return true;
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

    public Span<TKey> Keys => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
}

public class MultiMap<TKey, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7> where TKey: IKey<TKey>
{
    private FastListStruct<TKey> _sparse;
    private MultiArrayStruct<TKey, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7> _dense;

    public MultiMap()
    {
        _sparse = new FastListStruct<TKey>();
        _dense = new MultiArrayStruct<TKey, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7>();
    }

    public int Set(TKey key, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7)
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
            denseIndex = _dense.Add(key, value1, value2, value3, value4, value5, value6, value7);
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

        _dense.Set(denseIndex, key, value1, value2, value3, value4, value5, value6, value7);

        return denseIndex;
    }
    
    public bool Remove(TKey key)
    {
        int index = key.Index;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref TKey denseKey = ref _sparse[index];

        if (denseKey.IsTombStone())
        {
            return false;
        }

        // The key isn't here actually, incompatible versions 
        if (key.Version != denseKey.Version)
        {
            return false;
        }
        
        if (_dense.InternalSwapRemove(denseKey.Index, out TKey? swappedKey))
        {
            ref var swappedSparseKey = ref _sparse[swappedKey.Index];
            swappedSparseKey = denseKey.WithVersion(swappedSparseKey.Version);

            return true;
        }

        denseKey = TKey.TombStone;
        return true;
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

    public Span<TKey> Keys => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
    public Span<TValue6> Values6 => _dense.Values7;
}

public class MultiMap<TKey, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8> where TKey: IKey<TKey>
{
    private FastListStruct<TKey> _sparse;
    private MultiArrayStruct<TKey, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8> _dense;

    public MultiMap()
    {
        _sparse = new FastListStruct<TKey>();
        _dense = new MultiArrayStruct<TKey, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>();
    }

    public int Set(TKey key, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8)
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
            denseIndex = _dense.Add(key, value1, value2, value3, value4, value5, value6, value7, value8);
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

        _dense.Set(denseIndex, key, value1, value2, value3, value4, value5, value6, value7, value8);

        return denseIndex;
    }
    
    public bool Remove(TKey key)
    {
        int index = key.Index;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref TKey denseKey = ref _sparse[index];

        if (denseKey.IsTombStone())
        {
            return false;
        }

        // The key isn't here actually, incompatible versions 
        if (key.Version != denseKey.Version)
        {
            return false;
        }
        
        if (_dense.InternalSwapRemove(denseKey.Index, out TKey? swappedKey))
        {
            ref var swappedSparseKey = ref _sparse[swappedKey.Index];
            swappedSparseKey = denseKey.WithVersion(swappedSparseKey.Version);

            return true;
        }

        denseKey = TKey.TombStone;
        return true;
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

    public Span<TKey> Keys => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
    public Span<TValue6> Values6 => _dense.Values7;
    public Span<TValue7> Values7 => _dense.Values8;
}


public struct MultiMapStruct<TKey, TValue1, TValue2, TValue3> where TKey: IKey<TKey>
{
    private FastListStruct<TKey> _sparse;
    private MultiArrayStruct<TKey, TValue1, TValue2, TValue3> _dense;

    public MultiMapStruct()
    {
        _sparse = new FastListStruct<TKey>();
        _dense = new MultiArrayStruct<TKey, TValue1, TValue2, TValue3>();
    }

    public int Set(TKey key, TValue1 value1, TValue2 value2, TValue3 value3)
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
            denseIndex = _dense.Add(key, value1, value2, value3);
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

        _dense.Set(denseIndex, key, value1, value2, value3);

        return denseIndex;
    }
    
    public bool Remove(TKey key)
    {
        int index = key.Index;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref TKey denseKey = ref _sparse[index];

        if (denseKey.IsTombStone())
        {
            return false;
        }

        // The key isn't here actually, incompatible versions 
        if (key.Version != denseKey.Version)
        {
            return false;
        }
        
        if (_dense.InternalSwapRemove(denseKey.Index, out TKey? swappedKey))
        {
            ref var swappedSparseKey = ref _sparse[swappedKey.Index];
            swappedSparseKey = denseKey.WithVersion(swappedSparseKey.Version);

            return true;
        }

        denseKey = TKey.TombStone;
        return true;
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

    public Span<TKey> Keys => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
}

public struct MultiMapStruct<TKey, TValue1, TValue2, TValue3, TValue4> where TKey: IKey<TKey>
{
    private FastListStruct<TKey> _sparse;
    private MultiArrayStruct<TKey, TValue1, TValue2, TValue3, TValue4> _dense;

    public MultiMapStruct()
    {
        _sparse = new FastListStruct<TKey>();
        _dense = new MultiArrayStruct<TKey, TValue1, TValue2, TValue3, TValue4>();
    }

    public int Set(TKey key, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4)
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
            denseIndex = _dense.Add(key, value1, value2, value3, value4);
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

        _dense.Set(denseIndex, key, value1, value2, value3, value4);

        return denseIndex;
    }
    
    public bool Remove(TKey key)
    {
        int index = key.Index;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref TKey denseKey = ref _sparse[index];

        if (denseKey.IsTombStone())
        {
            return false;
        }

        // The key isn't here actually, incompatible versions 
        if (key.Version != denseKey.Version)
        {
            return false;
        }
        
        if (_dense.InternalSwapRemove(denseKey.Index, out TKey? swappedKey))
        {
            ref var swappedSparseKey = ref _sparse[swappedKey.Index];
            swappedSparseKey = denseKey.WithVersion(swappedSparseKey.Version);

            return true;
        }

        denseKey = TKey.TombStone;
        return true;
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

    public Span<TKey> Keys => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
}

public struct MultiMapStruct<TKey, TValue1, TValue2, TValue3, TValue4, TValue5> where TKey: IKey<TKey>
{
    private FastListStruct<TKey> _sparse;
    private MultiArrayStruct<TKey, TValue1, TValue2, TValue3, TValue4, TValue5> _dense;

    public MultiMapStruct()
    {
        _sparse = new FastListStruct<TKey>();
        _dense = new MultiArrayStruct<TKey, TValue1, TValue2, TValue3, TValue4, TValue5>();
    }

    public int Set(TKey key, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5)
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
            denseIndex = _dense.Add(key, value1, value2, value3, value4, value5);
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

        _dense.Set(denseIndex, key, value1, value2, value3, value4, value5);

        return denseIndex;
    }
    
    public bool Remove(TKey key)
    {
        int index = key.Index;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref TKey denseKey = ref _sparse[index];

        if (denseKey.IsTombStone())
        {
            return false;
        }

        // The key isn't here actually, incompatible versions 
        if (key.Version != denseKey.Version)
        {
            return false;
        }
        
        if (_dense.InternalSwapRemove(denseKey.Index, out TKey? swappedKey))
        {
            ref var swappedSparseKey = ref _sparse[swappedKey.Index];
            swappedSparseKey = denseKey.WithVersion(swappedSparseKey.Version);

            return true;
        }

        denseKey = TKey.TombStone;
        return true;
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

    public Span<TKey> Keys => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
}

public struct MultiMapStruct<TKey, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6> where TKey: IKey<TKey>
{
    private FastListStruct<TKey> _sparse;
    private MultiArrayStruct<TKey, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6> _dense;

    public MultiMapStruct()
    {
        _sparse = new FastListStruct<TKey>();
        _dense = new MultiArrayStruct<TKey, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6>();
    }

    public int Set(TKey key, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6)
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
            denseIndex = _dense.Add(key, value1, value2, value3, value4, value5, value6);
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

        _dense.Set(denseIndex, key, value1, value2, value3, value4, value5, value6);

        return denseIndex;
    }
    
    public bool Remove(TKey key)
    {
        int index = key.Index;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref TKey denseKey = ref _sparse[index];

        if (denseKey.IsTombStone())
        {
            return false;
        }

        // The key isn't here actually, incompatible versions 
        if (key.Version != denseKey.Version)
        {
            return false;
        }
        
        if (_dense.InternalSwapRemove(denseKey.Index, out TKey? swappedKey))
        {
            ref var swappedSparseKey = ref _sparse[swappedKey.Index];
            swappedSparseKey = denseKey.WithVersion(swappedSparseKey.Version);

            return true;
        }

        denseKey = TKey.TombStone;
        return true;
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

    public Span<TKey> Keys => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
}

public struct MultiMapStruct<TKey, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7> where TKey: IKey<TKey>
{
    private FastListStruct<TKey> _sparse;
    private MultiArrayStruct<TKey, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7> _dense;

    public MultiMapStruct()
    {
        _sparse = new FastListStruct<TKey>();
        _dense = new MultiArrayStruct<TKey, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7>();
    }

    public int Set(TKey key, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7)
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
            denseIndex = _dense.Add(key, value1, value2, value3, value4, value5, value6, value7);
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

        _dense.Set(denseIndex, key, value1, value2, value3, value4, value5, value6, value7);

        return denseIndex;
    }
    
    public bool Remove(TKey key)
    {
        int index = key.Index;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref TKey denseKey = ref _sparse[index];

        if (denseKey.IsTombStone())
        {
            return false;
        }

        // The key isn't here actually, incompatible versions 
        if (key.Version != denseKey.Version)
        {
            return false;
        }
        
        if (_dense.InternalSwapRemove(denseKey.Index, out TKey? swappedKey))
        {
            ref var swappedSparseKey = ref _sparse[swappedKey.Index];
            swappedSparseKey = denseKey.WithVersion(swappedSparseKey.Version);

            return true;
        }

        denseKey = TKey.TombStone;
        return true;
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

    public Span<TKey> Keys => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
    public Span<TValue6> Values6 => _dense.Values7;
}

public struct MultiMapStruct<TKey, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8> where TKey: IKey<TKey>
{
    private FastListStruct<TKey> _sparse;
    private MultiArrayStruct<TKey, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8> _dense;

    public MultiMapStruct()
    {
        _sparse = new FastListStruct<TKey>();
        _dense = new MultiArrayStruct<TKey, TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>();
    }

    public int Set(TKey key, TValue1 value1, TValue2 value2, TValue3 value3, TValue4 value4, TValue5 value5, TValue6 value6, TValue7 value7, TValue8 value8)
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
            denseIndex = _dense.Add(key, value1, value2, value3, value4, value5, value6, value7, value8);
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

        _dense.Set(denseIndex, key, value1, value2, value3, value4, value5, value6, value7, value8);

        return denseIndex;
    }
    
    public bool Remove(TKey key)
    {
        int index = key.Index;
        if (index > (_sparse.Length - 1))
        {
            return false;
        }

        ref TKey denseKey = ref _sparse[index];

        if (denseKey.IsTombStone())
        {
            return false;
        }

        // The key isn't here actually, incompatible versions 
        if (key.Version != denseKey.Version)
        {
            return false;
        }
        
        if (_dense.InternalSwapRemove(denseKey.Index, out TKey? swappedKey))
        {
            ref var swappedSparseKey = ref _sparse[swappedKey.Index];
            swappedSparseKey = denseKey.WithVersion(swappedSparseKey.Version);

            return true;
        }

        denseKey = TKey.TombStone;
        return true;
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

    public Span<TKey> Keys => _dense.Values1;
    
    public Span<TValue1> Values1 => _dense.Values2;
    public Span<TValue2> Values2 => _dense.Values3;
    public Span<TValue3> Values3 => _dense.Values4;
    public Span<TValue4> Values4 => _dense.Values5;
    public Span<TValue5> Values5 => _dense.Values6;
    public Span<TValue6> Values6 => _dense.Values7;
    public Span<TValue7> Values7 => _dense.Values8;
}



