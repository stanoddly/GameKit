namespace GameKit.DI;

internal enum ServiceDescriptorKind
{
    Type,
    Instance,
    Factory,
    Alias
}

internal class ServiceDescriptor
{
    public Type ServiceType { get; }
    public ServiceDescriptorKind Kind { get; }
    public object? Instance { get; }
    public Delegate? Factory { get; }
    public Func<ServiceProvider, object>? TypedFactory { get; }
    public Type? AliasSource { get; }

    public ServiceDescriptor(Type serviceType, ServiceDescriptorKind kind, Type? aliasSource = null)
    {
        ServiceType = serviceType;
        Kind = kind;
        AliasSource = aliasSource;
    }

    public ServiceDescriptor(Type serviceType, object instance)
    {
        ServiceType = serviceType;
        Kind = ServiceDescriptorKind.Instance;
        Instance = instance;
    }

    public ServiceDescriptor(Type serviceType, Delegate factory)
    {
        ServiceType = serviceType;
        Kind = ServiceDescriptorKind.Factory;
        Factory = factory;
    }

    public ServiceDescriptor(Type serviceType, Func<ServiceProvider, object> typedFactory)
    {
        ServiceType = serviceType;
        Kind = ServiceDescriptorKind.Type;
        TypedFactory = typedFactory;
    }
}
