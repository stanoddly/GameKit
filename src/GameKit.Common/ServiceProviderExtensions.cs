using System.Runtime.CompilerServices;

namespace GameKit.Common;

public static class ServiceProviderExtensions
{
    public static TService GetMandatoryService<TService>(this IServiceProvider serviceProvider)
    {
        object? objectService = serviceProvider.GetService(typeof(TService));

        if (objectService == null)
        {
            throw new InvalidOperationException($"Service of type '{typeof(TService).FullName}' is not registered in the service provider.");
        }
        
        return Unsafe.As<object, TService>(ref objectService);
    } 
}