using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace GameKit.Ioc;

public class GameModuleBuilder
{
    private readonly ServiceCollection _services = new();
    private readonly List<Action<IServiceProvider>> _serviceProviderActions = new();
    private readonly HashSet<Type> _registeredTypes = new();
    private readonly List<Action<object>> _activationCallbacks = new();

    public GameModuleBuilder OnActivated(Action<object> callback)
    {
        _activationCallbacks.Add(callback);
        return this;
    }

    private void InvokeActivationCallbacks(object instance)
    {
        foreach (var cb in _activationCallbacks)
            cb(instance);
    }
    
    public GameModuleRegistrar<TService> RegisterType<TService>() 
        where TService : class
    {
        if (_registeredTypes.Contains(typeof(TService)))
        {
            throw new InvalidOperationException($"Type {typeof(TService)} is already registered.");
        }
        _services.AddSingleton<TService>();
        _registeredTypes.Add(typeof(TService));
        _serviceProviderActions.Add(provider => {
            var instance = provider.GetRequiredService<TService>();
            InvokeActivationCallbacks(instance);
        });
        return new GameModuleRegistrar<TService>(this);
    }

    private void ValidateRegistration(bool hasImpl, bool hasService, Type implType, Type serviceType)
    {
        if (hasImpl && hasService)
        {
            throw new InvalidOperationException($"Both {implType} and {serviceType} are already registered. This is likely a mistake.");
        }
    }

    internal GameModuleBuilder RegisterAs<TSource, TTarget>() 
        where TSource : class 
        where TTarget : class
    {
        if (!typeof(TTarget).IsAssignableFrom(typeof(TSource)))
        {
            // TODO:
            throw new ArgumentException("");
        }
        
        if (!_registeredTypes.Contains(typeof(TSource)))
        {
            throw new InvalidOperationException($"{typeof(TSource).Name} has not been registered first.");
        }

        _registeredTypes.Add(typeof(TTarget));
        _services.AddSingleton<TTarget>(provider => {
            TSource impl = provider.GetRequiredService<TSource>();
            return Unsafe.As<TTarget>(impl);
        });

        return this;
    }

    public GameModuleRegistrar<TService> RegisterInstance<TService>(TService instance)
        where TService : class
    {
        if (_registeredTypes.Contains(typeof(TService)))
        {
            throw new InvalidOperationException($"Type {typeof(TService)} is already registered.");
        }
        InvokeActivationCallbacks(instance);
        _services.AddSingleton(instance);
        _registeredTypes.Add(typeof(TService));

        return new GameModuleRegistrar<TService>(this);
    }
    
    public GameModuleRegistrar<TService> RegisterFunc<TService>(Func<IServiceProvider, TService> factory)
        where TService : class
    {
        if (_registeredTypes.Contains(typeof(TService)))
        {
            throw new InvalidOperationException($"Type {typeof(TService)} is already registered.");
        }
        _services.AddSingleton<TService>(provider => {
            var instance = factory(provider);
            InvokeActivationCallbacks(instance);
            return instance;
        });

        _registeredTypes.Add(typeof(TService));
        _serviceProviderActions.Add(provider => provider.GetRequiredService<TService>());
        return new GameModuleRegistrar<TService>(this);
    }
    
    public GameModuleRegistrar<TService> RegisterFunc<TService>(Delegate factory) where TService : class
    {
        MethodInfo method = factory.Method;
        ParameterInfo[] parameters = method.GetParameters();
        Type serviceType = method.ReturnType;

        if (method.ReturnType != serviceType)
        {
            throw new InvalidOperationException($"factory's return type {typeof(TService)} and typeof(TService) {serviceType} don't match."); 
        }

        if (_registeredTypes.Contains(serviceType))
        {
            throw new InvalidOperationException($"Type {serviceType} is already registered.");
        }

        _services.AddSingleton(serviceType, provider => {
            object[] args = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                args[i] = provider.GetRequiredService(parameters[i].ParameterType);
            }
            
            object? instance = method.Invoke(factory.Target, args);

            if (instance == null)
            {
                throw new InvalidOperationException($"Factory method returned null for service type {serviceType.Name}.");
            }
            
            InvokeActivationCallbacks(instance);
            return instance;
        });

        _registeredTypes.Add(serviceType);
        _serviceProviderActions.Add(provider => provider.GetRequiredService(serviceType));

        return new GameModuleRegistrar<TService>(this);
    }

    internal IServiceProvider BuildServiceProvider()
    {
        var serviceProvider = _services.BuildServiceProvider();
        foreach (var resolution in _serviceProviderActions)
        {
            resolution(serviceProvider);
        }
        return serviceProvider;
    }

    public bool IsRegistered(Type type)
    {
        return _registeredTypes.Contains(type);
    }

    public IServiceProvider Build()
    {
        return BuildServiceProvider();
    }
}
