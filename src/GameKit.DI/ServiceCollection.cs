using System.ComponentModel;
using System.Reflection;

namespace GameKit.DI;

public class ServiceCollection
{
    private readonly List<ServiceDescriptor> _descriptors = new();
    private readonly HashSet<Type> _registeredTypes = new();
    private readonly List<Action<object>> _activationCallbacks = new();
    private readonly List<Delegate> _onStartActions = new();
    private readonly List<Action<ServiceProvider>> _onStartGeneratedActions = new();
    private readonly List<Action<ServiceProvider>> _disposeCallbacks = new();

    public ServiceRegistrar<T> RegisterType<T>() where T : class
    {
        Type type = typeof(T);

        if (!_registeredTypes.Add(type))
        {
            throw new InvalidOperationException($"Type {type.Name} is already registered.");
        }

        _descriptors.Add(ServiceDescriptor.ForType(type));

        return new ServiceRegistrar<T>(this);
    }

    public ServiceRegistrar<T> RegisterInstance<T>(T instance) where T : class
    {
        Type type = typeof(T);

        if (!_registeredTypes.Add(type))
        {
            throw new InvalidOperationException($"Type {type.Name} is already registered.");
        }

        _descriptors.Add(ServiceDescriptor.ForInstance(type, instance));

        return new ServiceRegistrar<T>(this);
    }

    public ServiceRegistrar<T> RegisterFactory<T>(Delegate factory) where T : class
    {
        Type type = typeof(T);
        Type returnType = factory.Method.ReturnType;

        if (!type.IsAssignableFrom(returnType))
        {
            throw new ArgumentException(
                $"Factory delegate returns {returnType.Name}, which is not assignable to {type.Name}.");
        }

        if (!_registeredTypes.Add(type))
        {
            throw new InvalidOperationException($"Type {type.Name} is already registered.");
        }

        _descriptors.Add(ServiceDescriptor.ForFactory(type, factory));

        return new ServiceRegistrar<T>(this);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public ServiceRegistrar<T> RegisterTypeGenerated<T>(Func<ServiceProvider, object> factory) where T : class
    {
        Type type = typeof(T);

        if (!_registeredTypes.Add(type))
        {
            throw new InvalidOperationException($"Type {type.Name} is already registered.");
        }

        _descriptors.Add(ServiceDescriptor.ForTypedFactory(type, factory));

        return new ServiceRegistrar<T>(this);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void OnStartGenerated(Action<ServiceProvider> action)
    {
        _onStartGeneratedActions.Add(action);
    }

    internal void RegisterAlias<TSource, TTarget>() where TSource : class where TTarget : class
    {
        if (!typeof(TTarget).IsAssignableFrom(typeof(TSource)))
        {
            throw new ArgumentException($"{typeof(TSource).Name} is not assignable to {typeof(TTarget).Name}.");
        }

        if (!_registeredTypes.Contains(typeof(TSource)))
        {
            throw new InvalidOperationException($"{typeof(TSource).Name} has not been registered first.");
        }

        if (!_registeredTypes.Add(typeof(TTarget)))
        {
            throw new InvalidOperationException($"Type {typeof(TTarget).Name} is already registered.");
        }

        _descriptors.Add(ServiceDescriptor.ForAlias(typeof(TTarget), typeof(TSource)));
    }

    public void OnActivation(Action<object> callback)
    {
        _activationCallbacks.Add(callback);
    }

    public void OnStart(Delegate action)
    {
        _onStartActions.Add(action);
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
        foreach (Delegate action in _onStartActions)
        {
            InvokeDelegateVoid(action, provider, parent, descriptorMap, resolving);
        }

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

            case ServiceDescriptorKind.Type:
            {
                if (!resolving.Add(descriptor.ServiceType))
                {
                    throw new InvalidOperationException(
                        $"Circular dependency detected while resolving {descriptor.ServiceType.Name}.");
                }

                object instance;
                if (descriptor.TypedFactory != null)
                {
                    instance = descriptor.TypedFactory(provider);
                }
                else
                {
                    instance = CreateInstance(descriptor.ServiceType, provider, parent, descriptorMap, resolving);
                }

                provider.SetService(id, instance);
                InvokeActivationCallbacks(instance);
                resolving.Remove(descriptor.ServiceType);
                break;
            }

            case ServiceDescriptorKind.Factory:
            {
                if (!resolving.Add(descriptor.ServiceType))
                {
                    throw new InvalidOperationException(
                        $"Circular dependency detected while resolving {descriptor.ServiceType.Name}.");
                }

                object instance = InvokeDelegate(descriptor.Factory!, provider, parent, descriptorMap, resolving);
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

        // Try to resolve from descriptors
        if (descriptorMap.TryGetValue(type, out ServiceDescriptor? descriptor))
        {
            Resolve(descriptor, provider, parent, descriptorMap, resolving);
            service = id < provider.ServicesLength ? provider.GetServiceByIndex(id) : null;
            if (service != null)
            {
                return service;
            }
        }

        // Try parent
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

    private object CreateInstance(
        Type type,
        ServiceProvider provider,
        ServiceProvider? parent,
        Dictionary<Type, ServiceDescriptor> descriptorMap,
        HashSet<Type> resolving)
    {
        ConstructorInfo[] constructors = type.GetConstructors();

        if (constructors.Length != 1)
        {
            throw new InvalidOperationException(
                $"Type {type.Name} must have exactly one public constructor.");
        }

        ConstructorInfo constructor = constructors[0];
        ParameterInfo[] parameters = constructor.GetParameters();
        object[] args = new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            try
            {
                args[i] = ResolveServiceByType(parameters[i].ParameterType, provider, parent, descriptorMap, resolving);
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException(
                    $"Cannot resolve parameter '{parameters[i].Name}' of type {parameters[i].ParameterType.Name} when constructing {type.Name}.");
            }
        }

        return constructor.Invoke(args);
    }

    private object[] ResolveDelegateArgs(
        Delegate action,
        ServiceProvider provider,
        ServiceProvider? parent,
        Dictionary<Type, ServiceDescriptor> descriptorMap,
        HashSet<Type> resolving)
    {
        MethodInfo method = action.Method;
        ParameterInfo[] parameters = method.GetParameters();
        object[] args = new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            try
            {
                args[i] = ResolveServiceByType(parameters[i].ParameterType, provider, parent, descriptorMap, resolving);
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException(
                    $"Cannot resolve parameter '{parameters[i].Name}' of type {parameters[i].ParameterType.Name}.");
            }
        }

        return args;
    }

    private object InvokeDelegate(
        Delegate action,
        ServiceProvider provider,
        ServiceProvider? parent,
        Dictionary<Type, ServiceDescriptor> descriptorMap,
        HashSet<Type> resolving)
    {
        object[] args = ResolveDelegateArgs(action, provider, parent, descriptorMap, resolving);
        object? result = action.Method.Invoke(action.Target, args);

        if (result == null)
        {
            throw new InvalidOperationException(
                $"Factory delegate returned null.");
        }

        return result;
    }

    private void InvokeDelegateVoid(
        Delegate action,
        ServiceProvider provider,
        ServiceProvider? parent,
        Dictionary<Type, ServiceDescriptor> descriptorMap,
        HashSet<Type> resolving)
    {
        object[] args = ResolveDelegateArgs(action, provider, parent, descriptorMap, resolving);
        action.Method.Invoke(action.Target, args);
    }

    private void InvokeActivationCallbacks(object instance)
    {
        foreach (Action<object> callback in _activationCallbacks)
        {
            callback(instance);
        }
    }
}
