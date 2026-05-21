namespace GameKit.App;

internal sealed class UpdateRegistry
{
    private readonly List<IUpdatable> _updatables = new();
    private readonly object _lock = new();

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _updatables.Count;
            }
        }
    }

    public void Register(IUpdatable updatable)
    {
        lock (_lock)
        {
            for (int i = 0; i < _updatables.Count; i++)
            {
                if (ReferenceEquals(_updatables[i], updatable))
                {
                    return;
                }
            }

            _updatables.Add(updatable);
        }
    }

    public void Unregister(IUpdatable updatable)
    {
        lock (_lock)
        {
            for (int i = _updatables.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(_updatables[i], updatable))
                {
                    _updatables.RemoveAt(i);
                }
            }
        }
    }

    public IUpdatable[] Snapshot()
    {
        lock (_lock)
        {
            return _updatables.ToArray();
        }
    }
}

internal static class UpdateRegistryServiceCollectionExtensions
{
    public static GameKitAppBuilder RegisterUpdatables(this GameKitAppBuilder services, UpdateRegistry updateRegistry)
    {
        services.OnActivated((instance, _) =>
        {
            if (instance is IUpdatable updatable)
            {
                updateRegistry.Register(updatable);
            }
        });

        services.OnDisposing((instance, _) =>
        {
            if (instance is IUpdatable updatable)
            {
                updateRegistry.Unregister(updatable);
            }
        });

        return services;
    }
}
