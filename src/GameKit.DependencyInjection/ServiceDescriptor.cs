namespace GameKit.DependencyInjection;

internal enum ServiceDescriptorKind
{
    Type,
    TypedFactory,
    Instance,
    Factory,
    Alias
}

internal class ServiceDescriptor
{
    public Type ServiceType { get; }
    public ServiceDescriptorKind Kind { get; }
    public object? Instance { get; private init; }
    public Delegate? Factory { get; private init; }
    public Func<ServiceProvider, object>? TypedFactory { get; private init; }
    public Type? AliasSource { get; private init; }

    private ServiceDescriptor(Type serviceType, ServiceDescriptorKind kind)
    {
        ServiceType = serviceType;
        Kind = kind;
    }

    public static ServiceDescriptor ForType(Type serviceType)
    {
        return new ServiceDescriptor(serviceType, ServiceDescriptorKind.Type);
    }

    public static ServiceDescriptor ForInstance(Type serviceType, object instance)
    {
        return new ServiceDescriptor(serviceType, ServiceDescriptorKind.Instance) { Instance = instance };
    }

    public static ServiceDescriptor ForFactory(Type serviceType, Delegate factory)
    {
        return new ServiceDescriptor(serviceType, ServiceDescriptorKind.Factory) { Factory = factory };
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
