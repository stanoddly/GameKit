using GameKit.DependencyInjection;

namespace GameKit.App;

public class SceneManager : IDisposable
{
    private readonly ServiceProvider _rootProvider;
    private readonly Dictionary<string, ServiceProvider> _scenes = new();

    public SceneManager(ServiceProvider rootProvider)
    {
        _rootProvider = rootProvider;
    }

    public void Load(string name, Action<ServiceCollection> configure)
    {
        Unload(name);
        ServiceCollection collection = new();
        configure(collection);
        _scenes[name] = collection.BuildServiceProvider(_rootProvider);
    }

    public void Unload(string name)
    {
        if (_scenes.Remove(name, out ServiceProvider? provider))
        {
            provider.Dispose();
        }
    }

    public void Load(Action<ServiceCollection> configure)
    {
        Load("default", configure);
    }

    public void Unload()
    {
        Unload("default");
    }

    public void UnloadAll()
    {
        foreach (ServiceProvider provider in _scenes.Values)
        {
            provider.Dispose();
        }

        _scenes.Clear();
    }

    public void Dispose()
    {
        UnloadAll();
    }
}
