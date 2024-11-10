using System.Runtime.CompilerServices;

namespace GameKit.EntityComponent;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> Dictionary = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TService GetService<TService>() where TService : class
    {
        Type type = typeof(TService);
        return Unsafe.As<TService>(Dictionary[type]);
    }
    
    public static void ClearServices()
    {
        Dictionary.Clear();
    }
    
    public static void Provide<TService>(TService service) where TService: class
    {
        Type type = typeof(TService);
        Dictionary[type] = service;
    }

    public static void Populate(IReadOnlyDictionary<Type, object> services)
    {
        foreach (var (type, service) in services)
        {
            if (!type.IsInstanceOfType(service))
            {
                throw new Exception();
            }
            
            Dictionary.Add(type, service);
        }
    }
}
