using System.Numerics;
using System.Runtime.InteropServices;

namespace GameKit.Common;

public abstract class InterningService<T, TKey>
    where T: notnull
    where TKey: unmanaged, IUnsignedNumber<TKey>
{
    private readonly T _defaultInstance;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly record struct Key
    {
        public readonly TKey Value;

        internal Key(TKey value)
        {
            Value = value;
        }
    }
    
    public static readonly Key DefaultKey = new Key(TKey.Zero);
    
    private TKey _nextId = TKey.One;
    private readonly Dictionary<T, TKey> _instanceToKeyMap = new();
    private readonly Dictionary<TKey, T> _keyToInstanceMap = new();

    protected InterningService(T defaultInstance)
    {
        if (defaultInstance == null)
        {
            throw new ArgumentNullException(nameof(defaultInstance));
        }

        _defaultInstance = defaultInstance;
        _instanceToKeyMap[defaultInstance] = TKey.Zero;
        _keyToInstanceMap[TKey.Zero] = defaultInstance;
    }

    public Key Intern(T instance)
    {
        if (_instanceToKeyMap.TryGetValue(instance, out TKey key))
        {
            return new Key(key);
        }

        TKey id = _nextId++;
        _instanceToKeyMap[instance] = id;
        _keyToInstanceMap[id] = instance;

        return new Key(id);
    }
    
    public T Resolve(Key key)
    {
        return _keyToInstanceMap.GetValueOrDefault(key.Value, _defaultInstance);
    }
}