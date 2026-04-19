namespace GameKit.Componentize;

/// <summary>
/// Collects components via <see cref="With{TComponent}(TComponent)"/> and creates a GameObject
/// with two-phase initialization via <see cref="Build"/>.
/// Reusable — create one builder and call Build multiple times.
/// </summary>
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

    /// <summary>
    /// Creates a GameObject and runs the two-phase lifecycle:
    /// OnAttach for all components, then OnReady for all.
    /// Resets internal state so the builder can be reused.
    /// </summary>
    public GameObject Build()
    {
        List<GameComponent> components = _components ?? new();
        _components = null;

        GameObject gameObject = _world.CreateGameObject(components);

        foreach (GameComponent component in components)
        {
            component.InternalOwner = gameObject;
            component.OnAttach();
        }

        foreach (GameComponent component in components)
        {
            component.OnReady();
        }

        return gameObject;
    }
}
