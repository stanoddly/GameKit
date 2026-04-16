namespace GameKit.DependencyInjection;

public class ServiceCollection
{
    private readonly HashSet<Type> _registeredTypes = new();
    private readonly Dictionary<Type, List<ServiceDescriptor>> _serviceGroups = new();
    private readonly List<Action<object>> _activationCallbacks = new();
    private readonly List<Action<ServiceProvider>> _onStartActions = new();
    private readonly List<Action<ServiceProvider>> _disposeCallbacks = new();

    public void AddSingleton<T>() where T : class
    {
        throw new InvalidOperationException(
            $"AddSingleton<{typeof(T).Name}>() was not intercepted by the source generator. Ensure the GameKit.DependencyInjection.Generator is referenced.");
    }

    public void AddSingleton<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        throw new InvalidOperationException(
            $"AddSingleton<{typeof(TService).Name}, {typeof(TImplementation).Name}>() was not intercepted by the source generator. Ensure the GameKit.DependencyInjection.Generator is referenced.");
    }

    public void AddSingleton<T>(T instance) where T : class
    {
        Type type = typeof(T);
        ServiceDescriptor descriptor = ServiceDescriptor.ForInstance(type, instance);
        RegisterDescriptor(type, descriptor);
    }

    public void AddSingleton<T>(Delegate factory) where T : class
    {
        throw new InvalidOperationException(
            $"AddSingleton<{typeof(T).Name}>(Delegate) was not intercepted by the source generator. Ensure the GameKit.DependencyInjection.Generator is referenced.");
    }

    public void AddSingleton<T>(Func<ServiceProvider, T> factory) where T : class
    {
        Type type = typeof(T);
        ServiceDescriptor descriptor = ServiceDescriptor.ForTypedFactory(type, factory);
        RegisterDescriptor(type, descriptor);
    }

    public void OnStart(Action<ServiceProvider> action)
    {
        _onStartActions.Add(action);
    }

    public void AddAlias<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        if (!_registeredTypes.Contains(typeof(TImplementation)))
        {
            throw new InvalidOperationException($"{typeof(TImplementation).Name} has not been registered first.");
        }

        ServiceDescriptor descriptor = ServiceDescriptor.ForAlias(typeof(TService), typeof(TImplementation));
        RegisterDescriptor(typeof(TService), descriptor);
    }

    private void RegisterDescriptor(Type type, ServiceDescriptor descriptor)
    {
        _registeredTypes.Add(type);

        if (!_serviceGroups.TryGetValue(type, out List<ServiceDescriptor>? group))
        {
            group = new List<ServiceDescriptor>();
            _serviceGroups[type] = group;
        }

        group.Add(descriptor);
    }

    public void OnActivation(Action<object> callback)
    {
        _activationCallbacks.Add(callback);
    }

    public void OnStart(Delegate action)
    {
        throw new InvalidOperationException(
            "OnStart() was not intercepted by the source generator. Ensure the GameKit.DependencyInjection.Generator is referenced.");
    }

    public void OnDispose(Action<ServiceProvider> callback)
    {
        _disposeCallbacks.Add(callback);
    }

    public bool IsRegistered(Type type)
    {
        return _registeredTypes.Contains(type);
    }

    public bool IsRegistered<T>()
    {
        return _registeredTypes.Contains(typeof(T));
    }

    public ServiceProvider BuildServiceProvider()
    {
        return BuildServiceProvider(null);
    }

    public ServiceProvider BuildServiceProvider(ServiceProvider? parent)
    {
        // Build descriptorMap: last-wins descriptor per type (last element in each group)
        Dictionary<Type, ServiceDescriptor> descriptorMap = new();

        foreach (KeyValuePair<Type, List<ServiceDescriptor>> entry in _serviceGroups)
        {
            ServiceDescriptor lastDescriptor = entry.Value[entry.Value.Count - 1];
            descriptorMap[entry.Key] = lastDescriptor;
        }

        List<Action<ServiceProvider>> disposeCallbacks = new(_disposeCallbacks);
        ServiceProvider provider = new ServiceProvider(parent, disposeCallbacks);

        // Register ServiceProvider itself
        provider.SetService(ServiceTypeId<ServiceProvider>.Id, provider);

        // Cache of resolved instances keyed by descriptor, for non-last-wins descriptors
        Dictionary<ServiceDescriptor, object> resolvedInstances = new();

        HashSet<Type> resolving = new();

        // Set build-time resolvers so generated factories can trigger on-demand resolution
        provider.SetBuildTimeResolver(
            type => ResolveServiceByType(type, provider, parent, descriptorMap, resolving),
            type => TryResolveServiceByType(type, provider, parent, descriptorMap, resolving),
            type => ResolveServiceCollectionByType(type, provider, parent, descriptorMap, resolving));

        foreach (KeyValuePair<Type, ServiceDescriptor> entry in descriptorMap)
        {
            Resolve(entry.Value, provider, parent, descriptorMap, resolving);
        }

        // Resolve all non-last-wins descriptors and cache their instances
        foreach (KeyValuePair<Type, List<ServiceDescriptor>> entry in _serviceGroups)
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

        // Build service collections for GetServices<T>() from pre-resolved instances
        Dictionary<Type, object[]> serviceCollections = new();
        foreach (KeyValuePair<Type, List<ServiceDescriptor>> entry in _serviceGroups)
        {
            List<ServiceDescriptor> group = entry.Value;
            List<object> instances = new(group.Count);

            for (int i = 0; i < group.Count; i++)
            {
                ServiceDescriptor descriptor = group[i];
                if (i == group.Count - 1)
                {
                    int slotId = ServiceTypeId.GetId(descriptor.ServiceType);
                    object? slotInstance = provider.GetServiceById(slotId);
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

            serviceCollections[entry.Key] = instances.ToArray();
        }

        provider.SetServiceCollections(serviceCollections);

        // Fire OnStart callbacks
        foreach (Action<ServiceProvider> action in _onStartActions)
        {
            action(provider);
        }

        // Clear build-time resolvers — after build, all singletons are resolved
        provider.SetBuildTimeResolver(null, null, null);

        // Freeze after OnStart so callbacks can still trigger lazy resolution via SetService.
        provider.FreezeServices();

        return provider;
    }

    private void Resolve(
        ServiceDescriptor descriptor,
        ServiceProvider provider,
        ServiceProvider? parent,
        Dictionary<Type, ServiceDescriptor> descriptorMap,
        HashSet<Type> resolving)
    {
        int id = ServiceTypeId.GetId(descriptor.ServiceType);

        if (provider.GetServiceById(id) != null)
        {
            return;
        }

        switch (descriptor.Kind)
        {
            case ServiceDescriptorKind.Instance:
            {
                object instance = descriptor.Instance!;
                provider.SetService(id, instance);
                InvokeActivationCallbacks(instance);
                break;
            }

            case ServiceDescriptorKind.TypedFactory:
            {
                if (!resolving.Add(descriptor.ServiceType))
                {
                    throw new InvalidOperationException(
                        $"Circular dependency detected while resolving {descriptor.ServiceType.Name}.");
                }

                object instance = descriptor.TypedFactory!(provider)
                    ?? throw new InvalidOperationException("Factory delegate returned null.");
                provider.SetService(id, instance);
                InvokeActivationCallbacks(instance);
                resolving.Remove(descriptor.ServiceType);
                break;
            }

            case ServiceDescriptorKind.Alias:
            {
                // Ensure source is resolved first
                if (descriptorMap.TryGetValue(descriptor.AliasSource!, out ServiceDescriptor? sourceDescriptor))
                {
                    Resolve(sourceDescriptor, provider, parent, descriptorMap, resolving);
                }

                int sourceId = ServiceTypeId.GetId(descriptor.AliasSource!);
                object? source = provider.GetServiceById(sourceId);
                source ??= parent?.GetService(descriptor.AliasSource!);

                if (source == null)
                {
                    throw new InvalidOperationException(
                        $"Cannot resolve alias {descriptor.ServiceType.Name}: source type {descriptor.AliasSource!.Name} is not registered.");
                }

                provider.SetService(id, source);
                break;
            }
        }
    }

    private object? ResolveNonLastDescriptor(
        ServiceDescriptor descriptor,
        ServiceProvider provider,
        ServiceProvider? parent,
        Dictionary<Type, ServiceDescriptor> descriptorMap,
        HashSet<Type> resolving)
    {
        switch (descriptor.Kind)
        {
            case ServiceDescriptorKind.Instance:
            {
                object instance = descriptor.Instance!;
                InvokeActivationCallbacks(instance);
                return instance;
            }

            case ServiceDescriptorKind.TypedFactory:
            {
                object instance = descriptor.TypedFactory!(provider)
                    ?? throw new InvalidOperationException("Factory delegate returned null.");
                InvokeActivationCallbacks(instance);
                return instance;
            }

            case ServiceDescriptorKind.Alias:
            {
                int sourceId = ServiceTypeId.GetId(descriptor.AliasSource!);
                object? source = provider.GetServiceById(sourceId);
                return source ?? parent?.GetService(descriptor.AliasSource!);
            }

            default:
            {
                return null;
            }
        }
    }

    private object ResolveServiceByType(
        Type type,
        ServiceProvider provider,
        ServiceProvider? parent,
        Dictionary<Type, ServiceDescriptor> descriptorMap,
        HashSet<Type> resolving)
    {
        int id = ServiceTypeId.GetId(type);
        object? service = provider.GetServiceById(id);

        if (service != null)
        {
            return service;
        }

        if (descriptorMap.TryGetValue(type, out ServiceDescriptor? descriptor))
        {
            Resolve(descriptor, provider, parent, descriptorMap, resolving);
            service = provider.GetServiceById(id);
            if (service != null)
            {
                return service;
            }
        }

        service = parent?.GetService(type);
        if (service != null)
        {
            return service;
        }

        throw new InvalidOperationException(
            $"Cannot resolve service of type {type.Name}.");
    }

    private object? TryResolveServiceByType(
        Type type,
        ServiceProvider provider,
        ServiceProvider? parent,
        Dictionary<Type, ServiceDescriptor> descriptorMap,
        HashSet<Type> resolving)
    {
        int id = ServiceTypeId.GetId(type);
        object? service = provider.GetServiceById(id);

        if (service != null)
        {
            return service;
        }

        if (descriptorMap.TryGetValue(type, out ServiceDescriptor? descriptor))
        {
            Resolve(descriptor, provider, parent, descriptorMap, resolving);
            service = provider.GetServiceById(id);
            if (service != null)
            {
                return service;
            }
        }

        return parent?.GetService(type);
    }

    private object[] ResolveServiceCollectionByType(
        Type type,
        ServiceProvider provider,
        ServiceProvider? parent,
        Dictionary<Type, ServiceDescriptor> descriptorMap,
        HashSet<Type> resolving)
    {
        if (!_serviceGroups.TryGetValue(type, out List<ServiceDescriptor>? group))
        {
            return Array.Empty<object>();
        }

        List<object> instances = new(group.Count);

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
                        int id = ServiceTypeId.GetId(descriptor.ServiceType);
                        object? resolved = provider.GetServiceById(id);
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
                    if (descriptorMap.TryGetValue(descriptor.AliasSource!, out ServiceDescriptor? sourceDescriptor))
                    {
                        Resolve(sourceDescriptor, provider, parent, descriptorMap, resolving);
                    }

                    int sourceId = ServiceTypeId.GetId(descriptor.AliasSource!);
                    object? source = provider.GetServiceById(sourceId);
                    source ??= parent?.GetService(descriptor.AliasSource!);
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

    private void InvokeActivationCallbacks(object instance)
    {
        foreach (Action<object> callback in _activationCallbacks)
        {
            callback(instance);
        }
    }
}
