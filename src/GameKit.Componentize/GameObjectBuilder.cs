namespace GameKit.Componentize;

/// <summary>
/// Collects components via <see cref="With{TComponent}(TComponent)"/> and creates a GameObject
/// with two-phase initialization via <see cref="Build"/>.
/// Reusable — create one builder and call Build multiple times.
/// </summary>
public class GameObjectBuilder
{
    private readonly GameWorld _world;
    private List<ComponentBase>? _components;

    internal GameObjectBuilder(GameWorld world)
    {
        _world = world;
    }

    public GameObjectBuilder With<TComponent>() where TComponent : ComponentBase, new()
    {
        return With(new TComponent());
    }

    public GameObjectBuilder With<TComponent>(TComponent component) where TComponent : ComponentBase
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
        List<ComponentBase> components = _components ?? new();
        _components = null;

        GameObject gameObject = _world.CreateGameObject(components);

        gameObject.State = GameObjectState.Alive | GameObjectState.Building;
        try
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].OnAttach(gameObject, gameObject.ServiceProvider);
            }

            gameObject.State = GameObjectState.Alive;

            int readyCount = components.Count;
            for (int i = 0; i < readyCount; i++)
            {
                components[i].OnReady(gameObject, gameObject.ServiceProvider);
            }
        }
        catch
        {
            gameObject.State = GameObjectState.Alive;
            throw;
        }

        return gameObject;
    }
}
