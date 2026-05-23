using GameKit.DependencyInjection;

namespace GameKit.App;

public class SceneManager : IDisposable
{
    private readonly ServiceProvider _rootProvider;
    private ServiceProvider? _sceneProvider;
    private Action<ServiceCollection>? _pendingLoad;
    private bool _pendingUnload;

    public SceneManager(ServiceProvider rootProvider)
    {
        _rootProvider = rootProvider;
    }

    public void Load(Action<ServiceCollection> configure)
    {
        _pendingLoad = configure;
        _pendingUnload = false;
    }

    public void Unload()
    {
        _pendingLoad = null;
        _pendingUnload = true;
    }

    public void LoadImmediately(Action<ServiceCollection> configure)
    {
        UnloadImmediately();
        ServiceCollection collection = new();
        configure(collection);
        _sceneProvider = collection.BuildServiceProvider(_rootProvider);
    }

    public void UnloadImmediately()
    {
        if (_sceneProvider != null)
        {
            _sceneProvider.Dispose();
            _sceneProvider = null;
        }
    }

    internal void ApplyPendingTransition()
    {
        if (_pendingLoad != null)
        {
            Action<ServiceCollection> configure = _pendingLoad;
            _pendingLoad = null;
            LoadImmediately(configure);
        }
        else if (_pendingUnload)
        {
            _pendingUnload = false;
            UnloadImmediately();
        }
    }

    public void Dispose()
    {
        _pendingLoad = null;
        _pendingUnload = false;
        UnloadImmediately();
    }
}
