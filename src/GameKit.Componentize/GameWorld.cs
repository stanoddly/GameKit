using GameKit.Collections;
using GameKit.DependencyInjection;

namespace GameKit.Componentize;

public class GameWorld
{
    private readonly ServiceProvider _serviceProvider;
    private DenseSlotMap<Handle<GameObject>, GameObject> _gameObjects = new();

    public GameWorld(ServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    internal ServiceProvider ServiceProvider => _serviceProvider;

    public GameObject CreateGameObject()
    {
        GameObject gameObject = new GameObject(_serviceProvider);
        Handle<GameObject> handle = _gameObjects.Add(gameObject);
        gameObject.Handle = handle;
        return gameObject;
    }

    internal GameObject CreateGameObject(List<ComponentBase> components)
    {
        GameObject gameObject = new GameObject(_serviceProvider, components);
        Handle<GameObject> handle = _gameObjects.Add(gameObject);
        gameObject.Handle = handle;
        return gameObject;
    }

    /// <summary>
    /// Creates a reusable builder for constructing GameObjects with two-phase initialization.
    /// </summary>
    public GameObjectBuilder CreateGameObjectBuilder()
    {
        return new GameObjectBuilder(this);
    }

    public GameObject? GetGameObject(Handle<GameObject> handle)
    {
        if (_gameObjects.TryGetValue1(handle, out GameObject gameObject))
        {
            return gameObject;
        }
        return null;
    }

    public void RemoveGameObject(Handle<GameObject> handle)
    {
        if (_gameObjects.TryGetValue1(handle, out GameObject gameObject))
        {
            _gameObjects.Remove(handle);
            gameObject.NotifyRemoving();
            gameObject.DetachAllForRemoval();
            gameObject.NotifyRemoved();
        }
    }

    public void RemoveGameObject(GameObject gameObject)
    {
        RemoveGameObject(gameObject.Handle);
    }

}
