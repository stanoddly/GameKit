using System.Diagnostics.CodeAnalysis;

namespace GameKit.DependencyInjection;

internal enum ServiceDescriptorKind
{
    TypedFactory,
    Instance,
    Alias
}

internal class ServiceDescriptor
{
    public int ServiceTypeId { get; }
    public Type ServiceType { get; }
    public ServiceDescriptorKind Kind { get; }
    public object? Instance { get; private init; }
    public Func<ServiceProvider, object>? TypedFactory { get; private init; }
    public int AliasSourceId { get; private init; }
    public string? AliasSourceName { get; private init; }

    // The concrete implementation type — used for typed activation/disposal callbacks.
    // For Instance and TypedFactory descriptors this equals the T in ForInstance<T>/ForTypedFactory<T>.
    // Null for Alias descriptors (the source descriptor owns the type).
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
    public Type? ConcreteType { get; private init; }

    private ServiceDescriptor(int serviceTypeId, Type serviceType, ServiceDescriptorKind kind)
    {
        ServiceTypeId = serviceTypeId;
        ServiceType = serviceType;
        Kind = kind;
    }

    public static ServiceDescriptor ForInstance<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] T>(T instance) where T : class
    {
        return new ServiceDescriptor(ServiceTypeId<T>.Id, typeof(T), ServiceDescriptorKind.Instance)
        {
            Instance = instance,
            ConcreteType = typeof(T)
        };
    }

    public static ServiceDescriptor ForTypedFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] T>(Func<ServiceProvider, T> typedFactory) where T : class
    {
        return new ServiceDescriptor(ServiceTypeId<T>.Id, typeof(T), ServiceDescriptorKind.TypedFactory)
        {
            TypedFactory = typedFactory,
            ConcreteType = typeof(T)
        };
    }

    // Used by the source generator when the service type and implementation type differ
    // (AddSingleton<TService, TImpl>()) so that activation/disposal callbacks receive
    // typeof(TImpl) rather than typeof(TService).
    public static ServiceDescriptor ForTypedFactoryWithConcreteType<TService>(
        Func<ServiceProvider, TService> typedFactory,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type concreteType) where TService : class
    {
        return new ServiceDescriptor(ServiceTypeId<TService>.Id, typeof(TService), ServiceDescriptorKind.TypedFactory)
        {
            TypedFactory = typedFactory,
            ConcreteType = concreteType
        };
    }

    public static ServiceDescriptor ForAlias<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        return new ServiceDescriptor(ServiceTypeId<TService>.Id, typeof(TService), ServiceDescriptorKind.Alias)
        {
            AliasSourceId = ServiceTypeId<TImplementation>.Id,
            AliasSourceName = ServiceTypeId<TImplementation>.Name
        };
    }
}
