namespace GameKit.DependencyInjection;

public class ServiceProvider : IDisposable
{
    private object?[] _services;
    private readonly ServiceProvider? _parent;
    private readonly List<Action<ServiceProvider>> _disposeCallbacks;
    private Func<Type, object>? _buildTimeResolver;
    private Func<Type, object?>? _buildTimeTryResolver;
    private Func<Type, object[]>? _buildTimeCollectionResolver;
    private bool _disposed;
    private Dictionary<Type, object[]>? _serviceCollections;
    // Tracks slot indices in the order services were first stored, for reverse-order disposal
    private readonly List<int> _creationOrder = new();

    internal ServiceProvider(object?[] services, ServiceProvider? parent, List<Action<ServiceProvider>> disposeCallbacks)
    {
        _services = services;
        _parent = parent;
        _disposeCallbacks = disposeCallbacks;
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

    internal int ServicesLength => _services.Length;

    internal object? GetServiceByIndex(int id)
    {
        return _services[id];
    }

    internal void SetService(int id, object service)
    {
        if (id >= _services.Length)
        {
            object?[] resized = new object?[Math.Max(id + 1, _services.Length * 2)];
            Array.Copy(_services, resized, _services.Length);
            _services = resized;
        }

        // Record each slot index the first time it is populated, to support reverse-order disposal
        if (_services[id] == null)
        {
            _creationOrder.Add(id);
        }

        _services[id] = service;
    }

    public T GetService<T>() where T : class
    {
        int id = ServiceTypeId<T>.Id;

        if (id < _services.Length)
        {
            object? service = _services[id];
            if (service != null)
            {
                return (T)service;
            }
        }

        if (_buildTimeResolver != null)
        {
            return (T)_buildTimeResolver(typeof(T));
        }

        if (_parent != null)
        {
            return _parent.GetService<T>();
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

    public T? TryGetService<T>() where T : class
    {
        int id = ServiceTypeId<T>.Id;

        if (id < _services.Length)
        {
            object? service = _services[id];
            if (service != null)
            {
                return (T)service;
            }
        }

        if (_buildTimeTryResolver != null)
        {
            return (T?)_buildTimeTryResolver(typeof(T));
        }

        if (_parent != null)
        {
            return _parent.TryGetService<T>();
        }

        return null;
    }

    internal object? TryGetService(Type type)
    {
        int id = ServiceTypeId.GetId(type);

        if (id < _services.Length)
        {
            object? service = _services[id];
            if (service != null)
            {
                return service;
            }
        }

        if (_parent != null)
        {
            return _parent.TryGetService(type);
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
            object? service = _services[_creationOrder[i]];
            if (service is IDisposable disposable
                && !ReferenceEquals(service, this)
                && alreadyDisposed.Add(service))
            {
                disposable.Dispose();
            }
        }
    }
}
