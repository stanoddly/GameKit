namespace GameKit.Componentize;

public class GameObjectBuilder
{
    private readonly GameWorld _world;
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
        List<GameComponent> components = _components ?? new();
        _components = null;

        GameObject gameObject = _world.CreateGameObject(components);

        foreach (GameComponent component in components)
        {
            component.InternalOwner = gameObject;
            component.OnAttach();
            _world.NotifyComponentAttached(gameObject, component);
        }

        foreach (GameComponent component in components)
        {
            component.OnReady();
        }

        return gameObject;
    }
}
