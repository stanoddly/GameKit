namespace GameKit.EntityComponent;

public class GameWorld
{
    private readonly Dictionary<string, GameObject> _gameObjects = new();

    public void Add(string name, GameObject gameObject)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentException($"'{nameof(name)}' cannot be null or empty");

        if (!_gameObjects.TryAdd(name, gameObject))
        {
            throw new ArgumentException($"GameObject with '{name}' already exists");
        }
            
    }

    public void Remove(string name)
    {
        if (_gameObjects.Remove(name, out GameObject? gameObject))
        {
            gameObject.DetachAll();
        }
    }
}
