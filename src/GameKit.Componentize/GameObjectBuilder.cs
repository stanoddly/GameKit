namespace GameKit.Componentize;

public struct GameObjectBuilder
{
    private GameWorld? _world;
    private List<GameComponent>? _components;

    internal GameObjectBuilder(GameWorld world)
    {
        _world = world;
    }

    public GameObjectBuilder With<TComponent>() where TComponent : GameComponent, new()
    {
        return With(new TComponent());
    }

    public GameObjectBuilder With<TComponent>(TComponent component) where TComponent : GameComponent
    {
        _components ??= new();
        _components.Add(component);
        return this;
    }

    public GameObject Build()
    {
        GameWorld world = _world ?? throw new InvalidOperationException("GameObjectBuilder was not created via GameWorld.CreateGameObjectBuilder().");

        GameObject gameObject = world.CreateGameObject();

        if (_components != null)
        {
            foreach (GameComponent component in _components)
            {
                gameObject.AttachWithoutReady(component);
            }

            foreach (GameComponent component in _components)
            {
                component.OnReady();
            }

            _components.Clear();
        }

        return gameObject;
    }
}
