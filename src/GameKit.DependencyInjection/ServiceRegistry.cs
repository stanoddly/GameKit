namespace GameKit.DependencyInjection;

public sealed class ServiceRegistry<TService>
    where TService : class
{
    private readonly List<TService> _services = new();

    public IReadOnlyList<TService> Services => _services;

    public int Version { get; private set; }

    internal void Subscribe(TService service)
    {
        for (int i = 0; i < _services.Count; i++)
        {
            if (ReferenceEquals(_services[i], service))
            {
                return;
            }
        }

        _services.Add(service);
        Version++;
    }

    internal void Unsubscribe(TService service)
    {
        if (_services.Remove(service))
        {
            Version++;
        }
    }
}
