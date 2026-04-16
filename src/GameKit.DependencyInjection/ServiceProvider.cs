using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GameKit.DependencyInjection;

public class ServiceProvider : IDisposable
{
    // Flat array indexed by service ID (sequential, domain-scoped from ServiceTypeId).
    // Chosen over FrozenDictionary<int, object>: a bounds-checked array index is ~0.4ns
    // faster than hash+compare on the GetRequiredService hot path. Null until FreezeServices().
    private object?[]? _services;
    private Dictionary<int, object>? _pending;
    private readonly ServiceProvider? _parent;
    private readonly List<Action<ServiceProvider>> _disposeCallbacks;
    private Func<Type, object>? _buildTimeResolver;
    private Func<Type, object?>? _buildTimeTryResolver;
    private Func<Type, object[]>? _buildTimeCollectionResolver;
    private bool _disposed;
    // Values are real T[] instances built via Array.CreateInstance (see ServiceCollection).
    // GetServices<T>() recovers the typed array via Unsafe.As<T[]> and returns it directly
    // as IReadOnlyList<T> — zero allocation, zero copy. Do NOT switch to object[] storage:
    // that would force a per-call T[] allocation + element copy on every GetServices call.
    private Dictionary<int, Array>? _serviceCollections;
    // Tracks slot indices in the order services were first stored, for reverse-order disposal
    private readonly List<int> _creationOrder = new();

    internal ServiceProvider(ServiceProvider? parent, List<Action<ServiceProvider>> disposeCallbacks)
    {
        _parent = parent;
        _disposeCallbacks = disposeCallbacks;
        _pending = new Dictionary<int, object>();
    }

    internal void SetServiceCollections(Dictionary<int, Array> collections)
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
        Dictionary<int, object> pending = _pending!;

        int maxId = -1;
        foreach (KeyValuePair<int, object> kvp in pending)
        {
            if (kvp.Key > maxId)
            {
                maxId = kvp.Key;
            }
        }

        object?[] services = maxId >= 0 ? new object?[maxId + 1] : Array.Empty<object?>();
        foreach (KeyValuePair<int, object> kvp in pending)
        {
            services[kvp.Key] = kvp.Value;
        }

        _services = services;
        _pending = null;
    }

    internal object? GetServiceById(int id)
    {
        object?[]? services = _services;
        if (services != null)
        {
            if (id < services.Length)
            {
                return services[id];
            }
            return null;
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

        object?[]? services = _services;
        if (services != null)
        {
            if (id < services.Length)
            {
                object? frozen = services[id];
                if (frozen != null)
                {
                    // Unsafe.As skips the castclass check. Safe: the source generator
                    // controls registration and always stores services under their correct type.
                    return Unsafe.As<T>(frozen);
                }
            }
        }
        else if (_pending != null && _pending.TryGetValue(id, out object? pending))
        {
            return Unsafe.As<T>(pending);
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
        if (_serviceCollections != null)
        {
            int id = ServiceTypeId<T>.Id;
            // GetValueRefOrNullRef avoids the out-param dance of TryGetValue.
            ref Array arr = ref CollectionsMarshal.GetValueRefOrNullRef(_serviceCollections, id);
            if (!Unsafe.IsNullRef(ref arr))
            {
                // arr's runtime type IS T[] (built via Array.CreateInstance(typeof(T), n)
                // in ServiceCollection). Unsafe.As<T[]> recovers the strong type; returning
                // it directly as IReadOnlyList<T> costs nothing — no alloc, no copy.
                return Unsafe.As<T[]>(arr);
            }
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

        object?[]? services = _services;
        if (services != null)
        {
            if (id < services.Length)
            {
                object? frozen = services[id];
                if (frozen != null)
                {
                    return Unsafe.As<T>(frozen);
                }
            }
        }
        else if (_pending != null && _pending.TryGetValue(id, out object? pending))
        {
            return Unsafe.As<T>(pending);
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

        object?[]? services = _services;
        if (services != null)
        {
            if (id < services.Length)
            {
                return services[id];
            }
            return null;
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
            int slot = _creationOrder[i];
            object? service = null;

            object?[]? services = _services;
            if (services != null)
            {
                if (slot < services.Length)
                {
                    service = services[slot];
                }
            }
            else
            {
                service = _pending?.GetValueOrDefault(slot);
            }

            if (service is IDisposable disposable
                && !ReferenceEquals(service, this)
                && alreadyDisposed.Add(service))
            {
                disposable.Dispose();
            }
        }
    }
}
