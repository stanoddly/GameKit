using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace GameKit.DependencyInjection;

/// <summary>Collects service registrations and builds a <see cref="ServiceProvider"/> with all singletons eagerly resolved.</summary>
public class ServiceCollection
{
    private readonly HashSet<int> _registeredTypeIds = new();
    private readonly Dictionary<int, List<ServiceDescriptor>> _serviceGroups = new();
    private readonly List<Action<ServiceProvider>> _onStartActions = new();
    private readonly List<ServiceActivatedCallback> _activatedCallbacks = new();
    private readonly List<ServiceDisposingCallback> _disposingCallbacks = new();

    /// <summary>Registers <typeparamref name="T"/> as a singleton, constructing it via its single public constructor with dependencies resolved from the provider.</summary>
    /// <typeparam name="T">The concrete service type to register. Must be a named concrete type at the call site, not a type parameter.</typeparam>
    /// <remarks>This overload is intercepted by the source generator at each call site. The type argument must be a named concrete type — passing a type parameter prevents interception and causes the method to throw at runtime.</remarks>
    /// <exception cref="InvalidOperationException">Thrown at runtime if the source generator did not intercept this call — either because the generator is not referenced or because <typeparamref name="T"/> is a type parameter at the call site.</exception>
    public void AddSingleton<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] T>() where T : class
    {
        throw new InvalidOperationException(
            $"AddSingleton<{typeof(T).Name}>() was not intercepted by the source generator. Ensure the GameKit.DependencyInjection.Generator is referenced.");
    }

    /// <summary>Registers <typeparamref name="TImplementation"/> under the service type <typeparamref name="TService"/>, constructing it via its single public constructor.</summary>
    /// <typeparam name="TService">The service type (interface or base class) under which the implementation is resolved.</typeparam>
    /// <typeparam name="TImplementation">The concrete type to construct. Must be a named concrete type at the call site, not a type parameter.</typeparam>
    /// <remarks>This overload is intercepted by the source generator at each call site. Both type arguments must be named concrete types — passing a type parameter prevents interception and causes the method to throw at runtime.</remarks>
    /// <exception cref="InvalidOperationException">Thrown at runtime if the source generator did not intercept this call — either because the generator is not referenced or because either type argument is a type parameter at the call site.</exception>
    public void AddSingleton<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        throw new InvalidOperationException(
            $"AddSingleton<{typeof(TService).Name}, {typeof(TImplementation).Name}>() was not intercepted by the source generator. Ensure the GameKit.DependencyInjection.Generator is referenced.");
    }

    /// <summary>Registers an already-constructed instance as the singleton for <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The service type under which the instance is registered.</typeparam>
    /// <param name="instance">The pre-constructed instance to register.</param>
    public void AddSingleton<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] T>(T instance) where T : class
    {
        ServiceDescriptor descriptor = ServiceDescriptor.ForInstance(instance);
        RegisterDescriptor(ServiceTypeId<T>.Id, descriptor);
    }

    /// <summary>Registers a factory delegate for <typeparamref name="T"/> whose parameters are resolved as services from the provider.</summary>
    /// <typeparam name="T">The service type to register. Must be a named concrete type at the call site, not a type parameter.</typeparam>
    /// <param name="factory">A static method group or lambda whose parameter types are all registered services.</param>
    /// <remarks>This overload is intercepted by the source generator at each call site. The type argument must be a named concrete type — passing a type parameter prevents interception and causes the method to throw at runtime. Use the <see cref="AddSingleton{T}(Func{ServiceProvider,T})"/> overload when <typeparamref name="T"/> is a type parameter.</remarks>
    /// <exception cref="InvalidOperationException">Thrown at runtime if the source generator did not intercept this call — either because the generator is not referenced or because <typeparamref name="T"/> is a type parameter at the call site.</exception>
    public void AddSingleton<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] T>(Delegate factory) where T : class
    {
        throw new InvalidOperationException(
            $"AddSingleton<{typeof(T).Name}>(Delegate) was not intercepted by the source generator. Ensure the GameKit.DependencyInjection.Generator is referenced.");
    }

    /// <summary>
    /// Registers a typed factory that produces <typeparamref name="TImpl"/> instances under the service type
    /// <typeparamref name="TService"/>. Activation and disposal callbacks receive <c>typeof(TImpl)</c>.
    /// </summary>
    /// <typeparam name="TService">The service type (interface or base class) under which the instance is resolved.</typeparam>
    /// <typeparam name="TImpl">The concrete implementation type produced by <paramref name="factory"/>.</typeparam>
    /// <param name="factory">A delegate that receives the <see cref="ServiceProvider"/> and returns the constructed instance.</param>
    public void AddSingleton<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TImpl>(
        Func<ServiceProvider, TImpl> factory)
        where TService : class
        where TImpl : class, TService
    {
        ServiceDescriptor descriptor = ServiceDescriptor.ForTypedFactoryWithConcreteType<TService, TImpl>(factory);
        RegisterDescriptor(ServiceTypeId<TService>.Id, descriptor);
    }

    /// <summary>Registers a typed factory delegate that receives the <see cref="ServiceProvider"/> directly and returns the singleton instance for <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The service type to register.</typeparam>
    /// <param name="factory">A delegate that receives the <see cref="ServiceProvider"/> and returns the constructed instance.</param>
    /// <example>
    /// <code>
    /// services.AddSingleton&lt;WorldMap&gt;(static sp =&gt;
    /// {
    ///     MapLoader loader = sp.GetRequiredService&lt;MapLoader&gt;();
    ///     return loader.LoadDefault();
    /// });
    /// </code>
    /// </example>
    public void AddSingleton<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] T>(Func<ServiceProvider, T> factory) where T : class
    {
        ServiceDescriptor descriptor = ServiceDescriptor.ForTypedFactory(factory);
        RegisterDescriptor(ServiceTypeId<T>.Id, descriptor);
    }

    /// <summary>Registers a callback that runs after all services are constructed but before the provider is frozen.</summary>
    /// <param name="action">The callback to invoke with the fully constructed <see cref="ServiceProvider"/>.</param>
    public void OnStart(Action<ServiceProvider> action)
    {
        _onStartActions.Add(action);
    }

    /// <summary>Makes <typeparamref name="TService"/> resolve to the same instance as the already-registered <typeparamref name="TImplementation"/>.</summary>
    /// <typeparam name="TService">The alias service type (interface or base class) to register.</typeparam>
    /// <typeparam name="TImplementation">The concrete type whose existing instance will be shared. Must already be registered.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown if <typeparamref name="TImplementation"/> has not been registered before calling this method.</exception>
    public void AddAlias<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        if (!_registeredTypeIds.Contains(ServiceTypeId<TImplementation>.Id))
        {
            throw new InvalidOperationException($"{typeof(TImplementation).Name} has not been registered first.");
        }

        ServiceDescriptor descriptor = ServiceDescriptor.ForAlias<TService, TImplementation>();
        RegisterDescriptor(ServiceTypeId<TService>.Id, descriptor);
    }

    private void RegisterDescriptor(int id, ServiceDescriptor descriptor)
    {
        _registeredTypeIds.Add(id);

        if (!_serviceGroups.TryGetValue(id, out List<ServiceDescriptor>? group))
        {
            group = new List<ServiceDescriptor>();
            _serviceGroups[id] = group;
        }

        group.Add(descriptor);
    }

    /// <summary>Registers a callback whose parameters are resolved as services, invoked after all services are constructed but before the provider is frozen.</summary>
    /// <param name="action">A delegate whose parameter types are all registered services.</param>
    /// <remarks>This overload is intercepted by the source generator at each call site. The delegate argument must be resolvable at compile time — otherwise the method throws at runtime.</remarks>
    /// <exception cref="InvalidOperationException">Thrown at runtime if the source generator did not intercept this call — either because the generator is not referenced or because the delegate is not resolvable at compile time.</exception>
    public void OnStart(Delegate action)
    {
        throw new InvalidOperationException(
            "OnStart() was not intercepted by the source generator. Ensure the GameKit.DependencyInjection.Generator is referenced.");
    }

    /// <summary>
    /// Registers a callback invoked immediately after each singleton is constructed (or, for pre-constructed
    /// instances, when the provider is built). The callback receives the instance and its concrete implementation type.
    /// </summary>
    /// <param name="callback">The callback to invoke for each activated singleton.</param>
    public void OnActivated(ServiceActivatedCallback callback)
    {
        _activatedCallbacks.Add(callback);
    }

    public void OnActivated(Action<object, Type> callback)
    {
        _activatedCallbacks.Add((instance, type, _) => callback(instance, type));
    }

    /// <summary>
    /// Registers a callback invoked during <see cref="ServiceProvider.Dispose"/> for each singleton,
    /// immediately before that service's own <see cref="IDisposable.Dispose"/> call. Services are visited
    /// in reverse creation order.
    /// </summary>
    /// <param name="callback">The callback to invoke for each singleton being disposed.</param>
    public void OnDisposing(ServiceDisposingCallback callback)
    {
        _disposingCallbacks.Add(callback);
    }

    public void OnDisposing(Action<object, Type> callback)
    {
        _disposingCallbacks.Add((instance, type, _) => callback(instance, type));
    }

    /// <summary>Returns <see langword="true"/> if <typeparamref name="T"/> has been registered at least once.</summary>
    /// <typeparam name="T">The service type to check.</typeparam>
    /// <returns><see langword="true"/> if <typeparamref name="T"/> is registered; otherwise <see langword="false"/>.</returns>
    public bool IsRegistered<T>()
    {
        return _registeredTypeIds.Contains(ServiceTypeId<T>.Id);
    }

    /// <summary>Resolves all services, fires <c>OnStart</c> callbacks, freezes the provider, and returns it.</summary>
    /// <returns>The fully constructed and frozen <see cref="ServiceProvider"/>.</returns>
    public ServiceProvider BuildServiceProvider()
    {
        return BuildServiceProvider(null);
    }

    /// <summary>Resolves all services, fires <c>OnStart</c> callbacks, freezes the provider, and returns it; resolution falls back to <paramref name="parent"/> when a type is not registered locally.</summary>
    /// <param name="parent">An optional parent provider used as a fallback for types not registered in this collection.</param>
    /// <returns>The fully constructed and frozen <see cref="ServiceProvider"/>.</returns>
    public ServiceProvider BuildServiceProvider(ServiceProvider? parent)
    {
        // Build descriptorMap: last-wins descriptor per type (last element in each group)
        Dictionary<int, ServiceDescriptor> descriptorMap = new();

        foreach (KeyValuePair<int, List<ServiceDescriptor>> entry in _serviceGroups)
        {
            ServiceDescriptor lastDescriptor = entry.Value[entry.Value.Count - 1];
            descriptorMap[entry.Key] = lastDescriptor;
        }

        ServiceProvider provider = new ServiceProvider(parent);

        List<ServiceActivatedCallback>? activatedCallbacks =
            _activatedCallbacks.Count > 0 ? new List<ServiceActivatedCallback>(_activatedCallbacks) : null;
        List<ServiceDisposingCallback>? disposingCallbacks =
            _disposingCallbacks.Count > 0 ? new List<ServiceDisposingCallback>(_disposingCallbacks) : null;
        provider.SetCallbacks(activatedCallbacks, disposingCallbacks);

        // Register ServiceProvider itself
        provider.SetService(ServiceTypeId<ServiceProvider>.Id, provider);

        // Cache of resolved instances keyed by descriptor, for non-last-wins descriptors
        Dictionary<ServiceDescriptor, object> resolvedInstances = new();

        HashSet<int> resolving = new();

        // Set build-time resolvers so generated factories can trigger on-demand resolution
        provider.SetBuildTimeResolver(
            id => ResolveServiceById(id, provider, parent, descriptorMap, resolving),
            id => TryResolveServiceById(id, provider, parent, descriptorMap, resolving),
            id => ResolveServiceCollectionById(id, provider, parent, descriptorMap, resolving));

        foreach (KeyValuePair<int, ServiceDescriptor> entry in descriptorMap)
        {
            Resolve(entry.Value, provider, parent, descriptorMap, resolving);
        }

        // Resolve all non-last-wins descriptors and cache their instances
        foreach (KeyValuePair<int, List<ServiceDescriptor>> entry in _serviceGroups)
        {
            List<ServiceDescriptor> group = entry.Value;
            // Skip the last element (last-wins, already resolved above)
            for (int i = 0; i < group.Count - 1; i++)
            {
                ServiceDescriptor descriptor = group[i];
                object? instance = ResolveNonLastDescriptor(descriptor, provider, parent, descriptorMap, resolving);
                if (instance != null)
                {
                    resolvedInstances[descriptor] = instance;
                }
            }
        }

        // Build service collections for GetServices<T>(), keyed by service-type id.
        Dictionary<int, Array> serviceCollections = new();
        foreach (KeyValuePair<int, List<ServiceDescriptor>> entry in _serviceGroups)
        {
            List<ServiceDescriptor> group = entry.Value;
            List<object> instances = new(group.Count);

            for (int i = 0; i < group.Count; i++)
            {
                ServiceDescriptor descriptor = group[i];
                if (i == group.Count - 1)
                {
                    object? slotInstance = provider.GetServiceById(descriptor.ServiceTypeId);
                    if (slotInstance != null)
                    {
                        instances.Add(slotInstance);
                    }
                }
                else
                {
                    // Non-last-wins: use cached resolved instance
                    if (resolvedInstances.TryGetValue(descriptor, out object? cachedInstance))
                    {
                        instances.Add(cachedInstance);
                    }
                }
            }

            // Array.CreateInstance(T, n) produces a real T[] at runtime. Populating via an
            // object[] view avoids Array.SetValue's reflection path; covariance store-checks
            // still apply but all instances are guaranteed to be T. The reason this is stored
            // as Array and not object[] is so ServiceProvider.GetServices<T>() can return it
            // directly via Unsafe.As<T[]> with zero allocation. If this is ever "simplified"
            // to object[] storage, GetServices<T> must allocate + copy on every call.
            Type serviceType = group[0].ServiceType;
            Array arr = Array.CreateInstance(serviceType, instances.Count);
            object[] arrAsObjects = Unsafe.As<object[]>(arr);
            for (int i = 0; i < instances.Count; i++)
            {
                arrAsObjects[i] = instances[i];
            }

            serviceCollections[entry.Key] = arr;
        }

        // Invariant: by this point every descriptor has been eagerly resolved into _pending
        // above, so serviceCollections is complete for every registered type. OnStart callbacks
        // below can only read through the public ServiceProvider API — there is no supported
        // path to add a new registration during OnStart, so freezing collections here is safe.
        provider.SetServiceCollections(serviceCollections);

        foreach (Action<ServiceProvider> action in _onStartActions)
        {
            action(provider);
        }

        // Clear build-time resolvers — after build, all singletons are resolved
        provider.SetBuildTimeResolver(null, null, null);

        // FreezeServices snapshots _pending into the flat _services array for O(1) lookup.
        // Ordering relative to OnStart is not load-bearing (OnStart only reads), but freezing
        // last keeps the build-time and runtime resolution paths consistent for callbacks.
        provider.FreezeServices();

        return provider;
    }

    private void Resolve(
        ServiceDescriptor descriptor,
        ServiceProvider provider,
        ServiceProvider? parent,
        Dictionary<int, ServiceDescriptor> descriptorMap,
        HashSet<int> resolving)
    {
        int id = descriptor.ServiceTypeId;

        if (provider.GetServiceById(id) != null)
        {
            return;
        }

        switch (descriptor.Kind)
        {
            case ServiceDescriptorKind.Instance:
            {
                object instance = descriptor.Instance!;
                provider.SetServiceWithType(id, instance, descriptor.ConcreteType!);
                provider.RunActivatedCallbacks(instance, descriptor.ConcreteType!);
                break;
            }

            case ServiceDescriptorKind.TypedFactory:
            {
                if (!resolving.Add(id))
                {
                    throw new InvalidOperationException(
                        $"Circular dependency detected while resolving {descriptor.ServiceType.Name}.");
                }

                object instance = descriptor.TypedFactory!(provider)
                    ?? throw new InvalidOperationException("Factory delegate returned null.");
                provider.SetServiceWithType(id, instance, descriptor.ConcreteType!);
                provider.RunActivatedCallbacks(instance, descriptor.ConcreteType!);
                resolving.Remove(id);
                break;
            }

            case ServiceDescriptorKind.Alias:
            {
                int sourceId = descriptor.AliasSourceId;

                // Ensure source is resolved first
                if (descriptorMap.TryGetValue(sourceId, out ServiceDescriptor? sourceDescriptor))
                {
                    Resolve(sourceDescriptor, provider, parent, descriptorMap, resolving);
                }

                object? source = provider.GetServiceById(sourceId);
                source ??= parent?.GetServiceByIdInChain(sourceId);

                if (source == null)
                {
                    throw new InvalidOperationException(
                        $"Cannot resolve alias {descriptor.ServiceType.Name}: source type {descriptor.AliasSourceName} is not registered.");
                }

                // Aliases share the source instance — use SetService (no type tracking needed;
                // the source descriptor already owns the typed activation/disposal records).
                provider.SetService(id, source);
                break;
            }
        }
    }

    private object? ResolveNonLastDescriptor(
        ServiceDescriptor descriptor,
        ServiceProvider provider,
        ServiceProvider? parent,
        Dictionary<int, ServiceDescriptor> descriptorMap,
        HashSet<int> resolving)
    {
        switch (descriptor.Kind)
        {
            case ServiceDescriptorKind.Instance:
            {
                object instance = descriptor.Instance!;
                provider.RunActivatedCallbacks(instance, descriptor.ConcreteType!);
                return instance;
            }

            case ServiceDescriptorKind.TypedFactory:
            {
                object instance = descriptor.TypedFactory!(provider)
                    ?? throw new InvalidOperationException("Factory delegate returned null.");
                provider.RunActivatedCallbacks(instance, descriptor.ConcreteType!);
                return instance;
            }

            case ServiceDescriptorKind.Alias:
            {
                int sourceId = descriptor.AliasSourceId;
                object? source = provider.GetServiceById(sourceId);
                return source ?? parent?.GetServiceByIdInChain(sourceId);
            }

            default:
            {
                return null;
            }
        }
    }

    private object ResolveServiceById(
        int id,
        ServiceProvider provider,
        ServiceProvider? parent,
        Dictionary<int, ServiceDescriptor> descriptorMap,
        HashSet<int> resolving)
    {
        object? service = provider.GetServiceById(id);

        if (service != null)
        {
            return service;
        }

        if (descriptorMap.TryGetValue(id, out ServiceDescriptor? descriptor))
        {
            Resolve(descriptor, provider, parent, descriptorMap, resolving);
            service = provider.GetServiceById(id);
            if (service != null)
            {
                return service;
            }
        }

        service = parent?.GetServiceByIdInChain(id);
        if (service != null)
        {
            return service;
        }

        throw new InvalidOperationException(
            $"Cannot resolve service with id {id}.");
    }

    private object? TryResolveServiceById(
        int id,
        ServiceProvider provider,
        ServiceProvider? parent,
        Dictionary<int, ServiceDescriptor> descriptorMap,
        HashSet<int> resolving)
    {
        object? service = provider.GetServiceById(id);

        if (service != null)
        {
            return service;
        }

        if (descriptorMap.TryGetValue(id, out ServiceDescriptor? descriptor))
        {
            Resolve(descriptor, provider, parent, descriptorMap, resolving);
            service = provider.GetServiceById(id);
            if (service != null)
            {
                return service;
            }
        }

        return parent?.GetServiceByIdInChain(id);
    }

    private object[] ResolveServiceCollectionById(
        int id,
        ServiceProvider provider,
        ServiceProvider? parent,
        Dictionary<int, ServiceDescriptor> descriptorMap,
        HashSet<int> resolving)
    {
        Array? parentCollection = parent?.GetMergedServiceCollectionById(id);

        if (!_serviceGroups.TryGetValue(id, out List<ServiceDescriptor>? group))
        {
            if (parentCollection == null)
            {
                return Array.Empty<object>();
            }

            object[] parentInstances = new object[parentCollection.Length];
            CopyArrayItems(parentCollection, parentInstances);

            return parentInstances;
        }

        List<object> instances = new(group.Count + (parentCollection?.Length ?? 0));

        if (parentCollection != null)
        {
            object[] parentItems = Unsafe.As<object[]>(parentCollection);
            instances.AddRange(parentItems);
        }

        for (int i = 0; i < group.Count; i++)
        {
            ServiceDescriptor descriptor = group[i];
            bool isLastWins = i == group.Count - 1;

            switch (descriptor.Kind)
            {
                case ServiceDescriptorKind.Instance:
                {
                    instances.Add(descriptor.Instance!);
                    break;
                }
                case ServiceDescriptorKind.TypedFactory:
                {
                    if (isLastWins)
                    {
                        Resolve(descriptor, provider, parent, descriptorMap, resolving);
                        object? resolved = provider.GetServiceById(descriptor.ServiceTypeId);
                        if (resolved != null)
                        {
                            instances.Add(resolved);
                        }
                    }
                    else
                    {
                        object? instance = ResolveNonLastDescriptor(descriptor, provider, parent, descriptorMap, resolving);
                        if (instance != null)
                        {
                            instances.Add(instance);
                        }
                    }
                    break;
                }
                case ServiceDescriptorKind.Alias:
                {
                    int sourceId = descriptor.AliasSourceId;

                    if (descriptorMap.TryGetValue(sourceId, out ServiceDescriptor? sourceDescriptor))
                    {
                        Resolve(sourceDescriptor, provider, parent, descriptorMap, resolving);
                    }

                    object? source = provider.GetServiceById(sourceId);
                    source ??= parent?.GetServiceByIdInChain(sourceId);
                    if (source != null)
                    {
                        instances.Add(source);
                    }
                    break;
                }
            }
        }

        return instances.ToArray();
    }

    private static void CopyArrayItems(Array source, object[] destination)
    {
        object[] sourceItems = Unsafe.As<object[]>(source);
        for (int i = 0; i < sourceItems.Length; i++)
        {
            destination[i] = sourceItems[i];
        }
    }
}
