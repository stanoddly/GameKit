using System.Runtime.CompilerServices;
using GameKit.Collections;
using GameKit.DependencyInjection;

namespace GameKit.Componentize;

public class GameWorld
{
    private readonly ServiceProvider _serviceProvider;
    private DenseSlotMap<Handle<GameObject>, GameObject> _gameObjects = new();
    private Dictionary<Type, GameComponent>? _exposed;

    public GameWorld(ServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    internal ServiceProvider ServiceProvider => _serviceProvider;

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

    public void Expose<T>(T component) where T : GameComponent
    {
        _exposed ??= new();

        if (!_exposed.TryAdd(typeof(T), component))
        {
            throw new InvalidOperationException($"A component of type {typeof(T).Name} is already exposed.");
        }
    }

    public void Revoke<T>(T component) where T : GameComponent
    {
        if (_exposed == null || !_exposed.TryGetValue(typeof(T), out GameComponent? existing) || existing != component)
        {
            throw new InvalidOperationException($"Component is not exposed as {typeof(T).Name}.");
        }

        _exposed.Remove(typeof(T));
    }

    public T Resolve<T>() where T : GameComponent
    {
        if (_exposed != null && _exposed.TryGetValue(typeof(T), out GameComponent? component))
        {
            return Unsafe.As<GameComponent, T>(ref component);
        }

        throw new InvalidOperationException($"No component exposed as {typeof(T).Name}.");
    }

    public T? TryResolve<T>() where T : GameComponent
    {
        if (_exposed != null && _exposed.TryGetValue(typeof(T), out GameComponent? component))
        {
            return Unsafe.As<GameComponent, T>(ref component);
        }

        return null;
    }

}
