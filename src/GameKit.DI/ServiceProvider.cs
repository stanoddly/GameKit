namespace GameKit.DI;

public class ServiceProvider : IDisposable
{
    private object?[] _services;
    private readonly ServiceProvider? _parent;
    private readonly List<Action<ServiceProvider>> _disposeCallbacks;
    private Func<Type, object>? _buildTimeResolver;
    private bool _disposed;

    internal ServiceProvider(object?[] services, ServiceProvider? parent, List<Action<ServiceProvider>> disposeCallbacks)
    {
        _services = services;
        _parent = parent;
        _disposeCallbacks = disposeCallbacks;
    }

    internal void SetBuildTimeResolver(Func<Type, object>? resolver)
    {
        _buildTimeResolver = resolver;
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
            object?[] resized = new object?[id + 1];
            Array.Copy(_services, resized, _services.Length);
            _services = resized;
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

        if (_buildTimeResolver != null)
        {
            return (T)_buildTimeResolver(typeof(T));
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
    }
}
