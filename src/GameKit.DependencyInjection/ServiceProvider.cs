using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GameKit.DependencyInjection;

/// <summary>An immutable service container produced by <see cref="ServiceCollection.BuildServiceProvider()"/>.</summary>
public class ServiceProvider : IDisposable
{
    // Flat array indexed by service ID (sequential, domain-scoped from ServiceTypeId).
    // Chosen over FrozenDictionary<int, object>: a bounds-checked array index is ~0.4ns
    // faster than hash+compare on the GetRequiredService hot path. Null until FreezeServices().
    private object?[]? _services;
    private Dictionary<int, object>? _pending;
    // Nulled on dispose after detaching, so unloaded child providers do not retain parent graphs.
    private ServiceProvider? _parent;
    private List<ServiceProvider>? _children;
    private List<ServiceActivatedCallback>? _activatedCallbacks;
    private List<ServiceDisposingCallback>? _disposingCallbacks;
    private Func<int, object>? _buildTimeResolver;
    private Func<int, object?>? _buildTimeTryResolver;
    private Func<int, object[]>? _buildTimeCollectionResolver;
    private bool _disposed;
    [ThreadStatic]
    private static HashSet<int>? s_resolvingTransientIds;
    // Values are real T[] instances built via Array.CreateInstance (see ServiceCollection).
    // GetServices<T>() recovers the typed array via Unsafe.As<T[]> and returns it directly
    // as IReadOnlyList<T> — zero allocation, zero copy. Do NOT switch to object[] storage:
    // that would force a per-call T[] allocation + element copy on every GetServices call.
    private Dictionary<int, Array>? _serviceCollections;
    private ServiceDescriptor?[]? _transientDescriptors;
    private Dictionary<int, ServiceCollectionRegistration[]>? _serviceCollectionRegistrations;
    private readonly List<object> _transientDisposables = new();
    private readonly List<Type> _transientDisposableTypes = new();
    // Tracks slot indices in the order services were first stored, for reverse-order disposal.
    // Parallel list _creationTypes records the concrete type for each slot, enabling typed disposal callbacks.
    private readonly List<int> _creationOrder = new();
    private readonly List<Type> _creationTypes = new();

    internal ServiceProvider(ServiceProvider? parent)
    {
        if (parent != null)
        {
            parent.AddChild(this);
        }

        _parent = parent;
        _pending = new Dictionary<int, object>();
    }

    private void AddChild(ServiceProvider child)
    {
        ThrowIfDisposed();

        _children ??= new List<ServiceProvider>();
        _children.Add(child);
    }

    private void RemoveChild(ServiceProvider child)
    {
        // Parent cascade clears _children before disposing children; child detach then becomes a no-op.
        _children?.Remove(child);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ServiceProvider));
        }
    }

    internal void SetServiceCollections(Dictionary<int, Array> collections)
    {
        _serviceCollections = collections;
    }

    internal void SetTransientDescriptors(ServiceDescriptor?[]? descriptors)
    {
        _transientDescriptors = descriptors;
    }

    internal void SetServiceCollectionRegistrations(Dictionary<int, ServiceCollectionRegistration[]> registrations)
    {
        _serviceCollectionRegistrations = registrations;
    }

    internal void SetBuildTimeResolver(Func<int, object>? resolver, Func<int, object?>? tryResolver, Func<int, object[]>? collectionResolver)
    {
        _buildTimeResolver = resolver;
        _buildTimeTryResolver = tryResolver;
        _buildTimeCollectionResolver = collectionResolver;
    }

    internal void SetCallbacks(List<ServiceActivatedCallback>? activatedCallbacks, List<ServiceDisposingCallback>? disposingCallbacks)
    {
        _activatedCallbacks = activatedCallbacks;
        _disposingCallbacks = disposingCallbacks;
    }

    internal List<ServiceActivatedCallback>? ActivatedCallbacks => _activatedCallbacks;

    internal List<ServiceDisposingCallback>? DisposingCallbacks => _disposingCallbacks;

    // The [DynamicallyAccessedMembers] annotation on type preserves interface metadata
    // when called from generator-emitted code via typeof(T) where T carries the annotation.
    internal void RunActivatedCallbacks(
        object instance,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type)
    {
        if (_activatedCallbacks == null)
        {
            return;
        }

        foreach (ServiceActivatedCallback callback in _activatedCallbacks)
        {
            callback(instance, type);
        }
    }

    internal void RunDisposingCallbacks(
        object instance,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type)
    {
        if (_disposingCallbacks != null)
        {
            foreach (ServiceDisposingCallback callback in _disposingCallbacks)
            {
                callback(instance, type);
            }
        }
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

        // Merge parent's service collections into the child's. Collections compose from
        // ancestor to child, while single-service resolution remains child-wins.
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
                if (_serviceCollectionRegistrations != null &&
                    _serviceCollectionRegistrations.TryGetValue(kvp.Key, out ServiceCollectionRegistration[]? childRegistrations))
                {
                    _serviceCollectionRegistrations[kvp.Key] =
                        Concat(ToSingletonRegistrations(kvp.Value), childRegistrations);
                    continue;
                }

                if (_serviceCollections.TryGetValue(kvp.Key, out Array? childCollection))
                {
                    Array parentCollection = kvp.Value;
                    Type elementType = parentCollection.GetType().GetElementType()!;
                    Array merged = Array.CreateInstance(elementType, parentCollection.Length + childCollection.Length);
                    Array.Copy(parentCollection, 0, merged, 0, parentCollection.Length);
                    Array.Copy(childCollection, 0, merged, parentCollection.Length, childCollection.Length);
                    _serviceCollections[kvp.Key] = merged;
                }
                else
                {
                    _serviceCollections.Add(kvp.Key, kvp.Value);
                }
            }
        }

        Dictionary<int, ServiceCollectionRegistration[]>? parentRegistrations = _parent?._serviceCollectionRegistrations;
        if (parentRegistrations != null)
        {
            _serviceCollectionRegistrations ??= new Dictionary<int, ServiceCollectionRegistration[]>(parentRegistrations.Count);

            foreach (KeyValuePair<int, ServiceCollectionRegistration[]> kvp in parentRegistrations)
            {
                if (_serviceCollectionRegistrations.TryGetValue(kvp.Key, out ServiceCollectionRegistration[]? childRegistrations))
                {
                    _serviceCollectionRegistrations[kvp.Key] = Concat(kvp.Value, childRegistrations);
                    continue;
                }

                if (_serviceCollections != null && _serviceCollections.TryGetValue(kvp.Key, out Array? childCollection))
                {
                    _serviceCollectionRegistrations[kvp.Key] =
                        Concat(kvp.Value, ToSingletonRegistrations(childCollection));
                    continue;
                }

                _serviceCollectionRegistrations.Add(kvp.Key, kvp.Value);
            }
        }
    }

    private static ServiceCollectionRegistration[] ToSingletonRegistrations(Array collection)
    {
        object[] items = Unsafe.As<object[]>(collection);
        ServiceCollectionRegistration[] registrations = new ServiceCollectionRegistration[items.Length];
        for (int i = 0; i < items.Length; i++)
        {
            registrations[i] = ServiceCollectionRegistration.ForSingleton(items[i]);
        }

        return registrations;
    }

    private static ServiceCollectionRegistration[] Concat(
        ServiceCollectionRegistration[] first,
        ServiceCollectionRegistration[] second)
    {
        ServiceCollectionRegistration[] result = new ServiceCollectionRegistration[first.Length + second.Length];
        Array.Copy(first, 0, result, 0, first.Length);
        Array.Copy(second, 0, result, first.Length, second.Length);
        return result;
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

    // Parent providers are already frozen with ancestor collections merged in, so one lookup is enough.
    internal Array? GetMergedServiceCollectionById(int id)
    {
        if (_serviceCollections != null && _serviceCollections.TryGetValue(id, out Array? collection))
        {
            return collection;
        }

        return null;
    }

    internal ServiceDescriptor? GetTransientDescriptorById(int id)
    {
        ServiceDescriptor?[]? descriptors = _transientDescriptors;
        if (descriptors != null && id < descriptors.Length)
        {
            return descriptors[id];
        }

        return null;
    }

    internal ServiceDescriptor?[]? GetTransientDescriptors()
    {
        return _transientDescriptors;
    }

    internal ServiceCollectionRegistration[]? GetMergedServiceCollectionRegistrationsById(int id)
    {
        if (_serviceCollectionRegistrations != null &&
            _serviceCollectionRegistrations.TryGetValue(id, out ServiceCollectionRegistration[]? registrations))
        {
            return registrations;
        }

        return null;
    }

    internal void SetService(int id, object service)
    {
        if (!_pending!.ContainsKey(id))
        {
            _creationOrder.Add(id);
            // Placeholder type; overwritten by SetServiceWithType when type is known.
            _creationTypes.Add(service.GetType());
        }

        _pending[id] = service;
    }

    internal void SetServiceWithType(
        int id,
        object service,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type concreteType)
    {
        if (!_pending!.ContainsKey(id))
        {
            _creationOrder.Add(id);
            _creationTypes.Add(concreteType);
        }

        _pending[id] = service;
    }

    internal object CreateTransient(ServiceDescriptor descriptor)
    {
        HashSet<int> resolving = s_resolvingTransientIds ??= new HashSet<int>();
        if (!resolving.Add(descriptor.ServiceTypeId))
        {
            throw new InvalidOperationException(
                $"Circular dependency detected while resolving {descriptor.ServiceType.Name}.");
        }

        try
        {
            object instance = descriptor.TypedFactory!(this)
                ?? throw new InvalidOperationException("Factory delegate returned null.");

            if (instance is IDisposable)
            {
                _transientDisposables.Add(instance);
                _transientDisposableTypes.Add(descriptor.ConcreteType!);
            }

            RunActivatedCallbacks(instance, descriptor.ConcreteType!);

            return instance;
        }
        finally
        {
            resolving.Remove(descriptor.ServiceTypeId);
        }
    }

    /// <summary>Returns the service registered for <typeparamref name="T"/>, throwing if the type is not registered.</summary>
    /// <typeparam name="T">The service type to resolve.</typeparam>
    /// <returns>The registered service instance of <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <typeparamref name="T"/> is not registered in this provider or any ancestor provider.</exception>
    public T GetRequiredService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] T>() where T : class
    {
        ThrowIfDisposed();

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

            ServiceDescriptor? transientDescriptor = GetTransientDescriptorById(id);
            if (transientDescriptor != null)
            {
                return Unsafe.As<T>(CreateTransient(transientDescriptor));
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
    public IReadOnlyList<T> GetServices<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] T>() where T : class
    {
        ThrowIfDisposed();

        int id = ServiceTypeId<T>.Id;

        if (_serviceCollectionRegistrations != null &&
            _serviceCollectionRegistrations.TryGetValue(id, out ServiceCollectionRegistration[]? registrations))
        {
            T[] resolved = new T[registrations.Length];
            for (int i = 0; i < registrations.Length; i++)
            {
                ServiceCollectionRegistration registration = registrations[i];
                resolved[i] = registration.TransientDescriptor != null
                    ? Unsafe.As<T>(CreateTransient(registration.TransientDescriptor))
                    : Unsafe.As<T>(registration.SingletonInstance!);
            }

            return resolved;
        }

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

    /// <summary>Returns the service registered for <typeparamref name="T"/>, or <see langword="null"/> if the type is not registered.</summary>
    /// <typeparam name="T">The service type to resolve.</typeparam>
    /// <returns>The registered service instance of <typeparamref name="T"/>, or <see langword="null"/> if not registered in this provider or any ancestor.</returns>
    public T? GetService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] T>() where T : class
    {
        ThrowIfDisposed();

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

            ServiceDescriptor? transientDescriptor = GetTransientDescriptorById(id);
            if (transientDescriptor != null)
            {
                return Unsafe.As<T>(CreateTransient(transientDescriptor));
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

    /// <summary>Fires <c>OnDisposing</c> callbacks per instance, then disposes all <see cref="IDisposable"/> services in reverse creation order.</summary>
    /// <remarks>Services aliased to multiple types are disposed exactly once — deduplication is done by reference, so aliases do not cause double disposal.</remarks>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        ServiceProvider? parent = _parent;
        _parent = null;
        parent?.RemoveChild(this);

        List<ServiceProvider>? children = _children;
        _children = null;
        if (children != null)
        {
            for (int i = children.Count - 1; i >= 0; i--)
            {
                children[i].Dispose();
            }
        }

        // Dispose in reverse creation order; deduplicate to avoid double-disposing aliased instances
        HashSet<object> alreadyDisposed = new(ReferenceEqualityComparer.Instance);

        for (int i = _transientDisposables.Count - 1; i >= 0; i--)
        {
            object service = _transientDisposables[i];
            if (!alreadyDisposed.Add(service))
            {
                continue;
            }

            Type serviceType = _transientDisposableTypes[i];
            RunDisposingCallbacks(service, serviceType);

            ((IDisposable)service).Dispose();
        }

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

            if (service == null || ReferenceEquals(service, this))
            {
                continue;
            }

            if (!alreadyDisposed.Add(service))
            {
                continue;
            }

            // Disposing callbacks fire before IDisposable.Dispose so callers can still use the
            // service (e.g. unsubscribe from event buses) while it is operational.
            Type serviceType = _creationTypes[i];
            RunDisposingCallbacks(service, serviceType);

            if (service is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        _services = null;
        _pending = null;
        _serviceCollections = null;
        _transientDescriptors = null;
        _serviceCollectionRegistrations = null;
        _activatedCallbacks = null;
        _disposingCallbacks = null;
        _buildTimeResolver = null;
        _buildTimeTryResolver = null;
        _buildTimeCollectionResolver = null;
        _transientDisposables.Clear();
        _transientDisposableTypes.Clear();
    }
}

internal sealed class ServiceCollectionRegistration
{
    public object? SingletonInstance { get; }
    public ServiceDescriptor? TransientDescriptor { get; }

    private ServiceCollectionRegistration(object? singletonInstance, ServiceDescriptor? transientDescriptor)
    {
        SingletonInstance = singletonInstance;
        TransientDescriptor = transientDescriptor;
    }

    public static ServiceCollectionRegistration ForSingleton(object instance)
    {
        return new ServiceCollectionRegistration(instance, null);
    }

    public static ServiceCollectionRegistration ForTransient(ServiceDescriptor descriptor)
    {
        return new ServiceCollectionRegistration(null, descriptor);
    }
}
