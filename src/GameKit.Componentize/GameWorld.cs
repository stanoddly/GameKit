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
        GlobalComponents = new GlobalComponentRegistry();
    }

    internal ServiceProvider ServiceProvider => _serviceProvider;

    public GlobalComponentRegistry GlobalComponents { get; }

    public GameObject CreateGameObject()
    {
        GameObject gameObject = new GameObject(this);
        Handle<GameObject> handle = _gameObjects.Add(gameObject);
        gameObject.Handle = handle;
        return gameObject;
    }

    internal GameObject CreateGameObject(List<GameComponent> components)
    {
        GameObject gameObject = new GameObject(this, components);
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
            gameObject.DetachAll();
            gameObject.NotifyRemoved();
        }
    }

    public void RemoveGameObject(GameObject gameObject)
    {
        RemoveGameObject(gameObject.Handle);
    }

}
