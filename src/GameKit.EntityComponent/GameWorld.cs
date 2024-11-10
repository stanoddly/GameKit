namespace GameKit.EntityComponent;

public class GameWorld
{
    private readonly Dictionary<string, GameObject> _gameObjects = new();

    public void Add(string name, GameObject gameObject)
    {
        _gameObjects.Add(name, gameObject);
    }

    public void Remove(string name)
    {
        if (_gameObjects.Remove(name, out GameObject? gameObject))
        {
            gameObject.DetachAll();
        }
    }
}
