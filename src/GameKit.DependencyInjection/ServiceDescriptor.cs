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

    private ServiceDescriptor(int serviceTypeId, Type serviceType, ServiceDescriptorKind kind)
    {
        ServiceTypeId = serviceTypeId;
        ServiceType = serviceType;
        Kind = kind;
    }

    public static ServiceDescriptor ForInstance<T>(T instance) where T : class
    {
        return new ServiceDescriptor(ServiceTypeId<T>.Id, typeof(T), ServiceDescriptorKind.Instance)
        {
            Instance = instance
        };
    }

    public static ServiceDescriptor ForTypedFactory<T>(Func<ServiceProvider, T> typedFactory) where T : class
    {
        return new ServiceDescriptor(ServiceTypeId<T>.Id, typeof(T), ServiceDescriptorKind.TypedFactory)
        {
            TypedFactory = typedFactory
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
