using System.ComponentModel;

namespace GameKit.DependencyInjection;

public class ServiceCollection
{
    private readonly List<ServiceDescriptor> _descriptors = new();
    private readonly HashSet<Type> _registeredTypes = new();
    private readonly List<Action<object>> _activationCallbacks = new();
    private readonly List<Action<ServiceProvider>> _onStartGeneratedActions = new();
    private readonly List<Action<ServiceProvider>> _disposeCallbacks = new();

    public void AddSingleton<T>() where T : class
    {
        throw new InvalidOperationException(
            $"AddSingleton<{typeof(T).Name}>() was not intercepted by the source generator. Ensure the GameKit.DependencyInjection.Generator is referenced.");
    }

    public void AddSingleton<TService, TImplementation>()
        where TService : class
        where TImplementation : class
    {
        throw new InvalidOperationException(
            $"AddSingleton<{typeof(TService).Name}, {typeof(TImplementation).Name}>() was not intercepted by the source generator. Ensure the GameKit.DependencyInjection.Generator is referenced.");
    }

    public void AddSingleton<T>(T instance) where T : class
    {
        Type type = typeof(T);

        if (!_registeredTypes.Add(type))
        {
            throw new InvalidOperationException($"Type {type.Name} is already registered.");
        }

        _descriptors.Add(ServiceDescriptor.ForInstance(type, instance));
    }

    public void AddSingleton<T>(Delegate factory) where T : class
    {
        throw new InvalidOperationException(
            $"AddSingleton<{typeof(T).Name}>(Delegate) was not intercepted by the source generator. Ensure the GameKit.DependencyInjection.Generator is referenced.");
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void AddSingletonGenerated<T>(Func<ServiceProvider, object> factory) where T : class
    {
        Type type = typeof(T);

        if (!_registeredTypes.Add(type))
        {
            throw new InvalidOperationException($"Type {type.Name} is already registered.");
        }

        _descriptors.Add(ServiceDescriptor.ForTypedFactory(type, factory));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void OnStartGenerated(Action<ServiceProvider> action)
    {
        _onStartGeneratedActions.Add(action);
    }

    public void AddAlias<TService, TImplementation>()
        where TService : class
        where TImplementation : class
    {
        if (!typeof(TService).IsAssignableFrom(typeof(TImplementation)))
        {
            throw new ArgumentException($"{typeof(TImplementation).Name} is not assignable to {typeof(TService).Name}.");
        }

        if (!_registeredTypes.Contains(typeof(TImplementation)))
        {
            throw new InvalidOperationException($"{typeof(TImplementation).Name} has not been registered first.");
        }

        if (!_registeredTypes.Add(typeof(TService)))
        {
            throw new InvalidOperationException($"Type {typeof(TService).Name} is already registered.");
        }

        _descriptors.Add(ServiceDescriptor.ForAlias(typeof(TService), typeof(TImplementation)));
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
        Dictionary<Type, ServiceDescriptor> descriptorMap = new();
        int maxId = ServiceTypeId<ServiceProvider>.Id;

        foreach (ServiceDescriptor descriptor in _descriptors)
        {
            descriptorMap[descriptor.ServiceType] = descriptor;
            int id = ServiceTypeId.GetId(descriptor.ServiceType);
            if (id > maxId)
            {
                maxId = id;
            }
        }

        object?[] services = new object?[maxId + 1];
        List<Action<ServiceProvider>> disposeCallbacks = new(_disposeCallbacks);
        ServiceProvider provider = new ServiceProvider(services, parent, disposeCallbacks);

        // Register ServiceProvider itself
        services[ServiceTypeId<ServiceProvider>.Id] = provider;

        // Resolve all registered services
        HashSet<Type> resolving = new();

        // Set build-time resolvers so generated factories can trigger on-demand resolution
        provider.SetBuildTimeResolver(
            type => ResolveServiceByType(type, provider, parent, descriptorMap, resolving),
            type => TryResolveServiceByType(type, provider, parent, descriptorMap, resolving));

        foreach (ServiceDescriptor descriptor in _descriptors)
        {
            Resolve(descriptor, provider, parent, descriptorMap, resolving);
        }

        // Fire OnStart callbacks
        foreach (Action<ServiceProvider> action in _onStartGeneratedActions)
        {
            action(provider);
        }

        // Clear build-time resolvers — after build, all singletons are resolved
        provider.SetBuildTimeResolver(null, null);

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

        if (id < provider.ServicesLength && provider.GetServiceByIndex(id) != null)
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
                object? source = sourceId < provider.ServicesLength ? provider.GetServiceByIndex(sourceId) : null;
                source ??= parent?.TryGetService(descriptor.AliasSource!);

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

    private object ResolveServiceByType(
        Type type,
        ServiceProvider provider,
        ServiceProvider? parent,
        Dictionary<Type, ServiceDescriptor> descriptorMap,
        HashSet<Type> resolving)
    {
        int id = ServiceTypeId.GetId(type);
        object? service = id < provider.ServicesLength ? provider.GetServiceByIndex(id) : null;

        if (service != null)
        {
            return service;
        }

        if (descriptorMap.TryGetValue(type, out ServiceDescriptor? descriptor))
        {
            Resolve(descriptor, provider, parent, descriptorMap, resolving);
            service = id < provider.ServicesLength ? provider.GetServiceByIndex(id) : null;
            if (service != null)
            {
                return service;
            }
        }

        service = parent?.TryGetService(type);
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
        object? service = id < provider.ServicesLength ? provider.GetServiceByIndex(id) : null;

        if (service != null)
        {
            return service;
        }

        if (descriptorMap.TryGetValue(type, out ServiceDescriptor? descriptor))
        {
            Resolve(descriptor, provider, parent, descriptorMap, resolving);
            service = id < provider.ServicesLength ? provider.GetServiceByIndex(id) : null;
            if (service != null)
            {
                return service;
            }
        }

        return parent?.TryGetService(type);
    }

    private void InvokeActivationCallbacks(object instance)
    {
        foreach (Action<object> callback in _activationCallbacks)
        {
            callback(instance);
        }
    }
}
