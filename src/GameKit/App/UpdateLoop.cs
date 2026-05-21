namespace GameKit.App;

internal sealed class UpdateLoop
{
    private readonly List<IUpdatable?> _updatables = new();

    public int Count { get; private set; }

    public void Register(IUpdatable updatable)
    {
        for (int i = 0; i < _updatables.Count; i++)
        {
            if (ReferenceEquals(_updatables[i], updatable))
            {
                return;
            }
        }

        _updatables.Add(updatable);
        Count++;
    }

    public void Unregister(IUpdatable updatable)
    {
        for (int i = 0; i < _updatables.Count; i++)
        {
            if (ReferenceEquals(_updatables[i], updatable))
            {
                _updatables[i] = null;
                Count--;
            }
        }
    }

    public void Update()
    {
        int count = _updatables.Count;
        bool needsCompaction = false;
        for (int i = 0; i < count; i++)
        {
            IUpdatable? updatable = _updatables[i];
            if (updatable == null)
            {
                needsCompaction = true;
                continue;
            }

            updatable.Update();
        }

        if (needsCompaction)
        {
            Compact();
        }
    }

    private void Compact()
    {
        for (int i = _updatables.Count - 1; i >= 0; i--)
        {
            if (_updatables[i] == null)
            {
                _updatables.RemoveAt(i);
            }
        }

    }
}

internal static class UpdateLoopServiceCollectionExtensions
{
    public static GameKitAppBuilder RegisterUpdatables(this GameKitAppBuilder services, UpdateLoop updateLoop)
    {
        services.OnActivated((instance, _) =>
        {
            if (instance is IUpdatable updatable)
            {
                updateLoop.Register(updatable);
            }
        });

        services.OnDisposing((instance, _) =>
        {
            if (instance is IUpdatable updatable)
            {
                updateLoop.Unregister(updatable);
            }
        });

        return services;
    }
}
