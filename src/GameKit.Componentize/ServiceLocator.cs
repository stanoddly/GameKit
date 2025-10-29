using GameKit.Common;
using Microsoft.Extensions.DependencyInjection;

namespace GameKit.Componentize;

public static class ServiceLocator
{
    private static IServiceProvider _serviceProvider;
    
    static ServiceLocator()
    {
        _serviceProvider = new ServiceCollection().BuildServiceProvider();
    }
    
    public static void SetServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public static TService GetService<TService>() where TService : class
    {
        return _serviceProvider.GetMandatoryService<TService>();
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
