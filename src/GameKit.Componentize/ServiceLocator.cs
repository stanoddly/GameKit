namespace GameKit.Componentize;

public static class ServiceLocator
{
    private static Func<Type, object>? _serviceResolver;

    public static void SetServiceResolver(Func<Type, object> resolver)
    {
        _serviceResolver = resolver;
    }

    public static TService GetService<TService>() where TService : class
    {
        if (_serviceResolver == null)
        {
            throw new InvalidOperationException("ServiceLocator has not been configured. Call SetServiceResolver first.");
        }

        object service = _serviceResolver(typeof(TService));
        return (TService)service;
    }
}

public static class Services<TService> where TService: class
{
    static Services()
    {
        Instance = ServiceLocator.GetService<TService>();
    }

    public static TService Instance { get; }
}
