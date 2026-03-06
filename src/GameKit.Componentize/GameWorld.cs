using System.Runtime.CompilerServices;

namespace GameKit.Componentize;

public class GameWorld
{
    private readonly Dictionary<string, GameObject> _gameObjects = new();
    private List<(Type Type, Action<GameObject, GameComponent> Callback)>? _attachedCallbacks;
    private List<(Type Type, Action<GameObject, GameComponent> Callback)>? _detachedCallbacks;

    public GameObject CreateGameObject(string name)
    {
        GameObject gameObject = new GameObject(this);

        _gameObjects.Add(name, gameObject);
        gameObject.Name = name;

        return gameObject;
    }

    public GameObject? GetGameObject(string name)
    {
        _gameObjects.TryGetValue(name, out GameObject? gameObject);
        return gameObject;
    }

    public void RemoveGameObject(string name)
    {
        if (_gameObjects.Remove(name, out GameObject? gameObject))
        {
            // TODO: make an internal method to delete self for performance reasons
            gameObject.DetachAll();
        }
    }

    public void OnComponentAttached<T>(Action<GameObject, T> callback) where T : GameComponent
    {
        _attachedCallbacks ??= new();
        // Safe: IsAssignableFrom guarantees the type match before invocation
        _attachedCallbacks.Add((typeof(T), (gameObject, component) => callback(gameObject, Unsafe.As<GameComponent, T>(ref component))));
    }

    public void OnComponentDetached<T>(Action<GameObject, T> callback) where T : GameComponent
    {
        _detachedCallbacks ??= new();
        // Safe: IsAssignableFrom guarantees the type match before invocation
        _detachedCallbacks.Add((typeof(T), (gameObject, component) => callback(gameObject, Unsafe.As<GameComponent, T>(ref component))));
    }

    internal void NotifyComponentAttached(GameObject gameObject, GameComponent component)
    {
        if (_attachedCallbacks == null) return;

        Type componentType = component.GetType();
        foreach (var (type, callback) in _attachedCallbacks)
        {
            if (type.IsAssignableFrom(componentType))
            {
                callback(gameObject, component);
            }
        }
    }

    internal void NotifyComponentDetached(GameObject gameObject, GameComponent component)
    {
        if (_detachedCallbacks == null) return;

        Type componentType = component.GetType();
        foreach (var (type, callback) in _detachedCallbacks)
        {
            if (type.IsAssignableFrom(componentType))
            {
                callback(gameObject, component);
            }
        }
    }
}
