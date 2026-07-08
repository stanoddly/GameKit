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

        if (service is IOrderable)
        {
            _services.Sort(static (left, right) =>
            {
                int leftOrder = left is IOrderable leftOrderable ? leftOrderable.Order : 0;
                int rightOrder = right is IOrderable rightOrderable ? rightOrderable.Order : 0;
                return leftOrder.CompareTo(rightOrder);
            });
        }
    }

    internal void Unsubscribe(TService service)
    {
        _services.Remove(service);
    }
}
