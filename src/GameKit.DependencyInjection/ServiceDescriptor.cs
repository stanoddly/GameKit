namespace GameKit.DependencyInjection;

internal enum ServiceDescriptorKind
{
    TypedFactory,
    Instance,
    Alias
}

internal class ServiceDescriptor
{
    public Type ServiceType { get; }
    public ServiceDescriptorKind Kind { get; }
    public object? Instance { get; private init; }
    public Func<ServiceProvider, object>? TypedFactory { get; private init; }
    public Type? AliasSource { get; private init; }

    private ServiceDescriptor(Type serviceType, ServiceDescriptorKind kind)
    {
        ServiceType = serviceType;
        Kind = kind;
    }

    public static ServiceDescriptor ForInstance(Type serviceType, object instance)
    {
        return new ServiceDescriptor(serviceType, ServiceDescriptorKind.Instance) { Instance = instance };
    }

    public static ServiceDescriptor ForTypedFactory(Type serviceType, Func<ServiceProvider, object> typedFactory)
    {
        return new ServiceDescriptor(serviceType, ServiceDescriptorKind.TypedFactory) { TypedFactory = typedFactory };
    }

    public static ServiceDescriptor ForAlias(Type serviceType, Type aliasSource)
    {
        return new ServiceDescriptor(serviceType, ServiceDescriptorKind.Alias) { AliasSource = aliasSource };
    }
}
