namespace GameKit.Componentize;

public class GameWorld
{
    private readonly Dictionary<string, GameObject> _gameObjects = new();

    public GameObject CreateGameObject(string name)
    {
        GameObject gameObject = new GameObject();
        
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
}
