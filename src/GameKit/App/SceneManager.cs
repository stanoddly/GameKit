using GameKit.DependencyInjection;

namespace GameKit.App;

public class SceneManager : IDisposable
{
    private readonly ServiceProvider _rootProvider;
    private ServiceProvider? _sceneProvider;

    public SceneManager(ServiceProvider rootProvider)
    {
        _rootProvider = rootProvider;
    }

    public void Load(Action<ServiceCollection> configure)
    {
        Unload();
        ServiceCollection collection = new();
        configure(collection);
        _sceneProvider = collection.BuildServiceProvider(_rootProvider);
    }

    public void Unload()
    {
        if (_sceneProvider != null)
        {
            _sceneProvider.Dispose();
            _sceneProvider = null;
        }
    }

    public void Dispose()
    {
        Unload();
    }
}
