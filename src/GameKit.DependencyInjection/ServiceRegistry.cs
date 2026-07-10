namespace GameKit.DependencyInjection;

public sealed class ServiceRegistry<TService>
    where TService : class
{
    private readonly List<TService> _services = new();

    internal ServiceRegistry()
    {
    }

    public IReadOnlyList<TService> Services => _services;

    public List<TService>.Enumerator GetEnumerator()
    {
        return _services.GetEnumerator();
    }

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
    }

    public void Sort(Comparison<TService> comparison)
    {
        _services.Sort(comparison);
    }

    internal void Unsubscribe(TService service)
    {
        _services.Remove(service);
    }
}
