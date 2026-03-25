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

        List<GameComponent> components;

        if (_components != null)
        {
            components = new List<GameComponent>(_components);
            _components.Clear();
        }
        else
        {
            components = new();
        }

        GameObject gameObject = world.CreateGameObject(components);

        foreach (GameComponent component in components)
        {
            component.InternalOwner = gameObject;
            component.OnAttach();
            world.NotifyComponentAttached(gameObject, component);
        }

        foreach (GameComponent component in components)
        {
            component.OnReady();
        }

        return gameObject;
    }
}
