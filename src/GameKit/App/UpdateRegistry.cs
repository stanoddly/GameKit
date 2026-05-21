using GameKit.DependencyInjection;

namespace GameKit.App;

public sealed class UpdateRegistry
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

public static class UpdateRegistryServiceCollectionExtensions
{
    public static ServiceCollection RegisterUpdatables(this ServiceCollection services)
    {
        services.OnActivated((instance, _, provider) =>
        {
            if (instance is IUpdatable updatable)
            {
                UpdateRegistry updateRegistry = provider.GetRequiredService<UpdateRegistry>();
                updateRegistry.Register(updatable);
            }
        });

        services.OnDisposing((instance, _, provider) =>
        {
            if (instance is IUpdatable updatable)
            {
                UpdateRegistry updateRegistry = provider.GetRequiredService<UpdateRegistry>();
                updateRegistry.Unregister(updatable);
            }
        });

        return services;
    }
}
