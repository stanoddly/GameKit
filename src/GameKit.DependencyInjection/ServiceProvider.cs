using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GameKit.DependencyInjection;

/// <summary>An immutable, singleton-only service container produced by <see cref="ServiceCollection.BuildServiceProvider()"/>.</summary>
public class ServiceProvider : IDisposable
{
    // Flat array indexed by service ID (sequential, domain-scoped from ServiceTypeId).
    // Chosen over FrozenDictionary<int, object>: a bounds-checked array index is ~0.4ns
    // faster than hash+compare on the GetRequiredService hot path. Null until FreezeServices().
    private object?[]? _services;
    private Dictionary<int, object>? _pending;
    private readonly ServiceProvider? _parent;
    private readonly List<Action<ServiceProvider>> _disposeCallbacks;
    private Func<int, object>? _buildTimeResolver;
    private Func<int, object?>? _buildTimeTryResolver;
    private Func<int, object[]>? _buildTimeCollectionResolver;
    private bool _disposed;
    // Values are real T[] instances built via Array.CreateInstance (see ServiceCollection).
    // GetServices<T>() recovers the typed array via Unsafe.As<T[]> and returns it directly
    // as IReadOnlyList<T> — zero allocation, zero copy. Do NOT switch to object[] storage:
    // that would force a per-call T[] allocation + element copy on every GetServices call.
    private Dictionary<int, Array>? _serviceCollections;
    // Tracks slot indices in the order services were first stored, for reverse-order disposal
    private readonly List<int> _creationOrder = new();

    public static ServiceProvider Empty { get; } = CreateEmpty();

    internal ServiceProvider(ServiceProvider? parent, List<Action<ServiceProvider>> disposeCallbacks)
    {
        _parent = parent;
        _disposeCallbacks = disposeCallbacks;
        _pending = new Dictionary<int, object>();
    }

    private static ServiceProvider CreateEmpty()
    {
        ServiceProvider provider = new ServiceProvider(null, new List<Action<ServiceProvider>>());
        provider.FreezeServices();
        return provider;
    }

    internal void SetServiceCollections(Dictionary<int, Array> collections)
    {
        _serviceCollections = collections;
    }

    internal void SetBuildTimeResolver(Func<int, object>? resolver, Func<int, object?>? tryResolver, Func<int, object[]>? collectionResolver)
    {
        _buildTimeResolver = resolver;
        _buildTimeTryResolver = tryResolver;
        _buildTimeCollectionResolver = collectionResolver;
    }

    internal void FreezeServices()
    {
        Dictionary<int, object> pending = _pending!;

        // Determine the child's own max id.
        int childMaxId = -1;
        foreach (KeyValuePair<int, object> kvp in pending)
        {
            if (kvp.Key > childMaxId)
            {
                childMaxId = kvp.Key;
            }
        }

        // Parent is already frozen; its _services array is the fully-flattened ancestor
        // state. One level up is sufficient — ancestors are already baked into it.
        object?[]? parentServices = _parent?._services;
        int parentMaxId = parentServices != null ? parentServices.Length - 1 : -1;

        int flatMaxId = childMaxId > parentMaxId ? childMaxId : parentMaxId;

        object?[] services = flatMaxId >= 0 ? new object?[flatMaxId + 1] : Array.Empty<object?>();

        // Copy parent slots first; child slots overlay them below (child wins on collision).
        // Parent slots are NOT added to _creationOrder — ownership stays with the parent.
        if (parentServices != null)
        {
            for (int i = 0; i < parentServices.Length; i++)
            {
                services[i] = parentServices[i];
            }
        }

        // Overlay child's own pending slots; these are already tracked in _creationOrder
        // (populated by SetService during build), so no _creationOrder mutation is needed here.
        foreach (KeyValuePair<int, object> kvp in pending)
        {
            services[kvp.Key] = kvp.Value;
        }

        _services = services;
        _pending = null;

        // Merge parent's service collections into the child's — child entries take precedence
        // (child's own collection for a given id fully replaces parent's, matching the
        // all-or-nothing fallback semantics on the hot path). Only fill gaps the child left.
        Dictionary<int, Array>? parentCollections = _parent?._serviceCollections;
        if (parentCollections != null)
        {
            // _serviceCollections may be null if the child had no multi-registrations.
            if (_serviceCollections == null)
            {
                _serviceCollections = new Dictionary<int, Array>(parentCollections.Count);
            }

            foreach (KeyValuePair<int, Array> kvp in parentCollections)
            {
                // TryAdd: child's entry wins; parent fills only the gaps.
                _serviceCollections.TryAdd(kvp.Key, kvp.Value);
            }
        }
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

    // Walks the parent chain when an id isn't found locally — used for alias source
    // resolution against a parent provider during BuildServiceProvider.
    internal object? GetServiceByIdInChain(int id)
    {
        object? service = GetServiceById(id);
        if (service != null)
        {
            return service;
        }
        return _parent?.GetServiceByIdInChain(id);
    }

    internal void SetService(int id, object service)
    {
        if (!_pending!.ContainsKey(id))
        {
            _creationOrder.Add(id);
        }

        _pending[id] = service;
    }

    /// <summary>Returns the singleton instance registered for <typeparamref name="T"/>, throwing if the type is not registered.</summary>
    /// <typeparam name="T">The service type to resolve.</typeparam>
    /// <returns>The registered singleton instance of <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <typeparamref name="T"/> is not registered in this provider or any ancestor provider.</exception>
    public T GetRequiredService<T>() where T : class
    {
        int id = ServiceTypeId<T>.Id;

        object?[]? services = _services;
        if (services != null)
        {
            // Frozen path: _services is the fully-flattened ancestor-to-child array built at
            // FreezeServices() time. Single bounds-check + index — no parent traversal.
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

            throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered.");
        }

        // Unfrozen (build-time) path.
        if (_pending != null && _pending.TryGetValue(id, out object? pending))
        {
            return Unsafe.As<T>(pending);
        }

        if (_buildTimeResolver != null)
        {
            return (T)_buildTimeResolver(id);
        }

        throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered.");
    }

    /// <summary>Returns all instances registered under <typeparamref name="T"/> as an <see cref="IReadOnlyList{T}"/>.</summary>
    /// <typeparam name="T">The service type to resolve.</typeparam>
    /// <returns>
    /// A <c>T[]</c> built at <see cref="ServiceCollection.BuildServiceProvider()"/> time and returned directly — no allocation or copy on each call.
    /// After freeze, <c>_serviceCollections</c> is the fully-merged ancestor-to-child map (child wins), so no parent traversal occurs here.
    /// Returns an empty list if no services of type <typeparamref name="T"/> are registered in this provider or any ancestor.
    /// </returns>
    public IReadOnlyList<T> GetServices<T>() where T : class
    {
        int id = ServiceTypeId<T>.Id;

        if (_serviceCollections != null)
        {
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

        // Unfrozen (build-time) path.
        if (_buildTimeCollectionResolver != null)
        {
            object[] resolved = _buildTimeCollectionResolver(id);
            T[] typed = new T[resolved.Length];
            for (int i = 0; i < resolved.Length; i++)
            {
                typed[i] = (T)resolved[i];
            }
            return typed;
        }

        return Array.Empty<T>();
    }

    /// <summary>Returns the singleton instance registered for <typeparamref name="T"/>, or <see langword="null"/> if the type is not registered.</summary>
    /// <typeparam name="T">The service type to resolve.</typeparam>
    /// <returns>The registered singleton instance of <typeparamref name="T"/>, or <see langword="null"/> if not registered in this provider or any ancestor.</returns>
    public T? GetService<T>() where T : class
    {
        int id = ServiceTypeId<T>.Id;

        object?[]? services = _services;
        if (services != null)
        {
            // Frozen path: _services is the fully-flattened ancestor-to-child array.
            // Single bounds-check + index — no parent traversal.
            if (id < services.Length)
            {
                object? frozen = services[id];
                if (frozen != null)
                {
                    return Unsafe.As<T>(frozen);
                }
            }

            return null;
        }

        // Unfrozen (build-time) path.
        if (_pending != null && _pending.TryGetValue(id, out object? pending))
        {
            return Unsafe.As<T>(pending);
        }

        if (_buildTimeTryResolver != null)
        {
            return (T?)_buildTimeTryResolver(id);
        }

        return null;
    }

    /// <summary>Fires <c>OnDispose</c> callbacks, then disposes all <see cref="IDisposable"/> services in reverse creation order.</summary>
    /// <remarks>Services aliased to multiple types are disposed exactly once — deduplication is done by reference, so aliases do not cause double disposal.</remarks>
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
