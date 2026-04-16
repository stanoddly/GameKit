using System.Collections.Frozen;

namespace GameKit.DependencyInjection;

public class ServiceProvider : IDisposable
{
    private FrozenDictionary<int, object> _services = FrozenDictionary<int, object>.Empty;
    private Dictionary<int, object>? _pending;
    private readonly ServiceProvider? _parent;
    private readonly List<Action<ServiceProvider>> _disposeCallbacks;
    private Func<Type, object>? _buildTimeResolver;
    private Func<Type, object?>? _buildTimeTryResolver;
    private Func<Type, object[]>? _buildTimeCollectionResolver;
    private bool _disposed;
    private Dictionary<Type, object[]>? _serviceCollections;
    // Tracks slot indices in the order services were first stored, for reverse-order disposal
    private readonly List<int> _creationOrder = new();

    internal ServiceProvider(ServiceProvider? parent, List<Action<ServiceProvider>> disposeCallbacks)
    {
        _parent = parent;
        _disposeCallbacks = disposeCallbacks;
        _pending = new Dictionary<int, object>();
    }

    internal void SetServiceCollections(Dictionary<Type, object[]> collections)
    {
        _serviceCollections = collections;
    }

    internal void SetBuildTimeResolver(Func<Type, object>? resolver, Func<Type, object?>? tryResolver, Func<Type, object[]>? collectionResolver)
    {
        _buildTimeResolver = resolver;
        _buildTimeTryResolver = tryResolver;
        _buildTimeCollectionResolver = collectionResolver;
    }

    internal void FreezeServices()
    {
        _services = _pending!.ToFrozenDictionary();
        _pending = null;
    }

    internal object? GetServiceByIndex(int id)
    {
        if (_services.TryGetValue(id, out object? frozen))
        {
            return frozen;
        }

        if (_pending != null && _pending.TryGetValue(id, out object? pending))
        {
            return pending;
        }

        return null;
    }

    internal void SetService(int id, object service)
    {
        if (!_pending!.ContainsKey(id))
        {
            _creationOrder.Add(id);
        }

        _pending[id] = service;
    }

    public T GetRequiredService<T>() where T : class
    {
        int id = ServiceTypeId<T>.Id;

        if (_services.TryGetValue(id, out object? frozen))
        {
            return (T)frozen;
        }

        if (_pending != null && _pending.TryGetValue(id, out object? pending))
        {
            return (T)pending;
        }

        if (_buildTimeResolver != null)
        {
            return (T)_buildTimeResolver(typeof(T));
        }

        if (_parent != null)
        {
            return _parent.GetRequiredService<T>();
        }

        throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered.");
    }

    public IReadOnlyList<T> GetServices<T>() where T : class
    {
        if (_serviceCollections != null && _serviceCollections.TryGetValue(typeof(T), out object[]? items))
        {
            T[] typed = new T[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                typed[i] = (T)items[i];
            }
            return typed;
        }

        if (_buildTimeCollectionResolver != null)
        {
            object[] resolved = _buildTimeCollectionResolver(typeof(T));
            T[] typed = new T[resolved.Length];
            for (int i = 0; i < resolved.Length; i++)
            {
                typed[i] = (T)resolved[i];
            }
            return typed;
        }

        if (_parent != null)
        {
            return _parent.GetServices<T>();
        }

        return Array.Empty<T>();
    }

    public T? GetService<T>() where T : class
    {
        int id = ServiceTypeId<T>.Id;

        if (_services.TryGetValue(id, out object? frozen))
        {
            return (T)frozen;
        }

        if (_pending != null && _pending.TryGetValue(id, out object? pending))
        {
            return (T)pending;
        }

        if (_buildTimeTryResolver != null)
        {
            return (T?)_buildTimeTryResolver(typeof(T));
        }

        if (_parent != null)
        {
            return _parent.GetService<T>();
        }

        return null;
    }

    internal object? GetService(Type type)
    {
        int id = ServiceTypeId.GetId(type);

        if (_services.TryGetValue(id, out object? frozen))
        {
            return frozen;
        }

        if (_pending != null && _pending.TryGetValue(id, out object? pending))
        {
            return pending;
        }

        if (_parent != null)
        {
            return _parent.GetService(type);
        }

        return null;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        foreach (Action<ServiceProvider> callback in _disposeCallbacks)
        {
            callback(this);
        }

        // Dispose in reverse creation order; deduplicate to avoid double-disposing aliased instances
        HashSet<object> alreadyDisposed = new(ReferenceEqualityComparer.Instance);
        for (int i = _creationOrder.Count - 1; i >= 0; i--)
        {
            object? service = _services.GetValueOrDefault(_creationOrder[i])
                ?? _pending?.GetValueOrDefault(_creationOrder[i]);
            if (service is IDisposable disposable
                && !ReferenceEquals(service, this)
                && alreadyDisposed.Add(service))
            {
                disposable.Dispose();
            }
        }
    }
}
