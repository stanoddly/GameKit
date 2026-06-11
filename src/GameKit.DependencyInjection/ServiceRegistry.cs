namespace GameKit.DependencyInjection;

public sealed class ServiceRegistry<TService>
    where TService : class
{
    private readonly List<TService> _services = new();

    public IReadOnlyList<TService> Services => _services;

    internal void Subscribe(TService service)
    {
        _services.Add(service);
    }

    internal void Unsubscribe(TService service)
    {
        _services.Remove(service);
    }
}
