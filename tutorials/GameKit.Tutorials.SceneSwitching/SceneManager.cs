using GameKit.DependencyInjection;
using GameKit.Pencuil;

namespace GameKit.Tutorials.SceneSwitching;

public class SceneManager
{
    private readonly ServiceProvider _rootProvider;
    private ServiceProvider? _sceneProvider;

    public SceneManager(ServiceProvider rootProvider)
    {
        _rootProvider = rootProvider;
    }

    public void LoadScene(Action<ServiceCollection> configure)
    {
        if (_sceneProvider != null)
        {
            Console.WriteLine("Disposing previous scene provider");
            _sceneProvider.Dispose();
        }

        ServiceCollection sceneCollection = new();
        configure(sceneCollection);
        _sceneProvider = sceneCollection.BuildServiceProvider(_rootProvider);
        Console.WriteLine("Built new scene provider");
    }

    public void UnloadScene()
    {
        if (_sceneProvider != null)
        {
            Console.WriteLine("Disposing scene provider");
            _sceneProvider.Dispose();
            _sceneProvider = null;
        }
    }
}
