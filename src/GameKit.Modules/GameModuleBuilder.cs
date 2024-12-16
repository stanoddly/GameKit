using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Lifetime;
using AutofacContainerBuilder = Autofac.ContainerBuilder;

namespace GameKit.Modules;

public enum Lifetime
{
    Scoped,
    Transient,
    Singleton
}

internal static class ScopeExtension
{
    public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> SetLifetimeScope<TLimit, TActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder, Lifetime lifetime)
    {
        switch (lifetime)
        {
            case Lifetime.Singleton:
                return builder.SingleInstance();
            case Lifetime.Scoped:
                return builder.InstancePerLifetimeScope();
            case Lifetime.Transient:
                return builder.InstancePerDependency();
            default:
                throw new NotImplementedException(lifetime.ToString());
        }
    }
}

public class GameModuleBuilder
{
    private readonly AutofacContainerBuilder _builder = new();
    private HashSet<Type> _registeredTypes = new();

    private void RegisterComponentContextFactory<TSourceType, TTargetType>(Func<IComponentContext, TSourceType> factory, Lifetime lifetime = default)
        where TSourceType : notnull
        where TTargetType : notnull
    {
        var registration = _builder
            .Register(factory)
            .SetLifetimeScope(lifetime)
            .As<TTargetType>()
            .OnActivating(OnActivatingHandler)
            .OnRelease(OnReleaseHandler);

        if (typeof(TSourceType) != typeof(TTargetType))
        {
            registration.As<TSourceType>();
        }

        _registeredTypes.Add(typeof(TSourceType));
        _registeredTypes.Add(typeof(TTargetType));
    }

    private static void OnActivatingHandler<TSourceType>(IActivatingEventArgs<TSourceType> handler)
    {
        TSourceType instance = handler.Instance;

        if (instance == null)
        {
            throw new Exception();
        }

        if (instance is IPreparable preparable)
        {
            preparable.Prepare();
        }
    }

    private static void OnReleaseHandler<TSourceType>(TSourceType instance)
    {
        if (instance == null)
        {
            return;
        }

        if (instance is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public GameModuleBuilder RegisterInstance<TType>(TType instance)
        where TType : class
    {
        _builder.RegisterInstance(instance).As<TType>();
        _registeredTypes.Add(typeof(TType));
        return this;
    }

    public GameModuleBuilder Register<TType>(Lifetime lifetime = default)
        where TType : notnull
    {
        Register<TType, TType>(lifetime);
        return this;
    }

    public GameModuleBuilder Register<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.Interfaces)] TSourceType, TTargetType>(Lifetime lifetime = default)
        where TSourceType : TTargetType
        where TTargetType : notnull
    {
        var registration = _builder
            .RegisterType<TSourceType>()
            .SetLifetimeScope(lifetime)
            .As<TTargetType>()
            .OnActivating(OnActivatingHandler)
            .OnRelease(OnReleaseHandler);
        if (typeof(TSourceType) != typeof(TTargetType))
        {
            registration.As<TSourceType>();
        }
        
        _registeredTypes.Add(typeof(TSourceType));
        _registeredTypes.Add(typeof(TTargetType));
        return this;
    }
    
    public GameModuleBuilder Register<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TSourceType, TTargetType1, TTargetType2>(Lifetime lifetime = default)
        where TSourceType : TTargetType1, TTargetType2
        where TTargetType1 : notnull
        where TTargetType2 : notnull
    {
        _builder
            .RegisterType<TSourceType>()
            .SetLifetimeScope(lifetime)
            .As<TTargetType1>()
            .As<TTargetType2>()
            .OnActivating(OnActivatingHandler)
            .OnRelease(OnReleaseHandler);
        
        _registeredTypes.Add(typeof(TSourceType));
        _registeredTypes.Add(typeof(TTargetType1));
        _registeredTypes.Add(typeof(TTargetType2));
        return this;
    }

    public GameModuleBuilder RegisterFactory<TSourceType, TTargetType>(Func<TSourceType> factory, Lifetime lifetime = default)
        where TSourceType : notnull
        where TTargetType : notnull
    {
        RegisterComponentContextFactory<TSourceType, TTargetType>(c => factory(), lifetime);
        return this;
    }

    public GameModuleBuilder RegisterFactory<TSourceType, TTargetType, TDependency1>(Func<TDependency1, TSourceType> factory, Lifetime lifetime = default)
        where TSourceType : notnull
        where TTargetType : notnull
        where TDependency1 : notnull
    {
        RegisterComponentContextFactory<TSourceType, TTargetType>(c => factory(c.Resolve<TDependency1>()), lifetime);
        return this;
    }

    public GameModuleBuilder RegisterFactory<TSourceType, TTargetType, TDependency1, TDependency2>(Func<TDependency1, TDependency2, TSourceType> factory, Lifetime lifetime = default)
        where TSourceType : notnull
        where TTargetType : notnull
        where TDependency1 : notnull
        where TDependency2 : notnull
    {
        RegisterComponentContextFactory<TSourceType, TTargetType>(c => factory(c.Resolve<TDependency1>(), c.Resolve<TDependency2>()), lifetime);
        return this;
    }

    public GameModuleBuilder RegisterFactory<TSourceType, TTargetType, TDependency1, TDependency2, TDependency3>(Func<TDependency1, TDependency2, TDependency3, TSourceType> factory, Lifetime lifetime = default)
        where TSourceType : notnull
        where TTargetType : notnull
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
    {
        RegisterComponentContextFactory<TSourceType, TTargetType>(c => factory(c.Resolve<TDependency1>(), c.Resolve<TDependency2>(), c.Resolve<TDependency3>()), lifetime);
        return this;
    }

    public GameModuleBuilder RegisterFactory<TSourceType>(Func<TSourceType> factory, Lifetime lifetime = default)
        where TSourceType : notnull
    {
        RegisterFactory<TSourceType, TSourceType>(factory, lifetime);
        return this;
    }

    public GameModuleBuilder RegisterFactory<TSourceType, TDependency1>(Func<TDependency1, TSourceType> factory, Lifetime lifetime = default)
        where TSourceType : notnull
        where TDependency1 : notnull
    {
        RegisterFactory<TSourceType, TSourceType, TDependency1>(factory, lifetime);
        return this;
    }
    
    public GameModuleBuilder RegisterFactory<TSourceType, TDependency1, TDependency2>(Func<TDependency1, TDependency2, TSourceType> factory, Lifetime lifetime = default)
        where TSourceType : notnull
        where TDependency1 : notnull
        where TDependency2 : notnull
    {
        RegisterFactory<TSourceType, TSourceType, TDependency1, TDependency2>(factory, lifetime);
        return this;
    }

    public bool IsRegistered<TType>()
    {
        return _registeredTypes.Contains(typeof(TType));
    }

    public GameModule Build()
    {
        List<IUpdatable> updatables = new();
        List<IDrawable> drawables = new();
        Dictionary<Type, object> services = new();
        //FrameContext frameContext = new();

        //RegisterInstance(frameContext);

        var container = _builder.Build();
        var componentRegistry = container.ComponentRegistry;

        // components are registered in reverse order
        // actually I don't think that order is guaranteed, but we have at least a test for that 
        foreach (var registration in componentRegistry.Registrations.Reverse())
        {
            // TransientLifetime is ignored
            if (registration.Lifetime is not (MatchingScopeLifetime or RootScopeLifetime or CurrentScopeLifetime))
            {
                break;
            }

            object? instanceForLater = null; 
            foreach (var registeredService in registration.Services)
            {
                var registeredServiceWithType = registeredService as TypedService;
                if (registeredServiceWithType == null)
                {
                    continue;
                }
                
                Type registeredServiceType = registeredServiceWithType.ServiceType;

                bool isInternalAutofacType = registeredServiceType == typeof(ILifetimeScope);
                if (isInternalAutofacType)
                {
                    break;
                }

                object instance = default!;
                try
                {
                    instance = container.Resolve(registeredServiceType);
                }
                catch (Autofac.Core.DependencyResolutionException ex)
                {
                    if (ex.InnerException != null)
                    {
                        ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                    }

                    throw;
                }

                if (instanceForLater == null)
                {
                    instanceForLater = instance;
                }

                services.Add(registeredServiceType, instance);
            }

            if (instanceForLater != null)
            {
                if (instanceForLater is IUpdatable updatable)
                {
                    updatables.Add(updatable);
                }

                if (instanceForLater is IDrawable drawable)
                {
                    drawables.Add(drawable);
                }
            }
        }

        return new GameModule(updatables, drawables, services);
    }
}