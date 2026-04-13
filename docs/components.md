# Components

GameKit provides a lightweight component system for game logic through `GameKit.Componentize`.

## Core Types

### GameWorld

Container for all game objects. Register it as a singleton and create objects via `[OnActivate]`:

```csharp
[Singleton]
public partial GameWorld GameWorld { get; }

[OnActivate]
void SetupWorld(GameWorld gameWorld)
{
    GameObjectBuilder builder = gameWorld.CreateGameObjectBuilder();
    builder
        .With<MovementComponent>()
        .With<HealthComponent>()
        .Build();
}
```

### GameObject

Entity that holds components, identified by `Handle<GameObject>`:

```csharp
GameObject player = gameWorld.CreateGameObject();
player.Attach<MovementComponent>();
player.Attach(new HealthComponent(100)); // Instance attachment

// Lookup
MovementComponent movement = player.Get<MovementComponent>();
HealthComponent? health = player.TryGet<HealthComponent>();

// Removal
player.Detach<HealthComponent>();
gameWorld.RemoveGameObject(player); // Detaches all components
```

### GameComponent

Base class for all components. Override lifecycle hooks:

- **`OnAttach`** — component is placed on the GameObject. Set up self-contained state. When using `GameObjectBuilder`, sibling `OnAttach` may not have run yet.
- **`OnReady`** — all siblings are attached and their `OnAttach` has completed. Safe to resolve sibling references and subscribe to sibling events.
- **`OnDetach`** — component is being removed. Clean up subscriptions and resources.

When attaching to a live GameObject via `Attach`, both `OnAttach` and `OnReady` are called immediately in sequence.

```csharp
public class MovementComponent : GameComponent
{
    private Handle<UpdateTag> _updateHandle;

    protected override void OnAttach()
    {
        _updateHandle = Services<UpdateSystem>.Instance.Add(Update);
    }

    protected override void OnReady()
    {
        // Safe to access siblings here
    }

    protected override void OnDetach()
    {
        Services<UpdateSystem>.Instance.Remove(_updateHandle);
    }

    private void Update()
    {
        // Called each frame
    }
}
```

## Services Access

Components access registered services via `Services<T>.Instance`:

```csharp
protected override void OnAttach()
{
    KeyboardService keyboard = Services<KeyboardService>.Instance;
    keyboard.KeyDown += OnKeyDown;
}
```

**Note:** `Services<T>` caches the instance on first access. The `ServiceLocator` must be configured with `SetServiceResolver` before any component accesses it.

## Update Registration

Updates are **not automatic**. Components must explicitly register with `UpdateSystem`:

```csharp
private Handle<UpdateTag> _updateHandle;

protected override void OnAttach()
{
    _updateHandle = Services<UpdateSystem>.Instance.Add(Update);
}

protected override void OnDetach()
{
    Services<UpdateSystem>.Instance.Remove(_updateHandle);
}
```

The handle must be stored to unregister later.

## Sibling Components

Access other components on the same GameObject:

```csharp
// Get sibling (throws if not found)
HealthComponent health = GetSibling<HealthComponent>();

// Try get sibling (returns null if not found)
HealthComponent? health = TryGetSibling<HealthComponent>();

// Attach/detach siblings
AttachSibling(new BuffComponent());
DetachSibling<BuffComponent>();
```

## GameObjectBuilder

Use `GameObjectBuilder` to create GameObjects with multiple components. This provides a two-phase lifecycle: `OnAttach` runs for all components first, then `OnReady` runs for all, guaranteeing siblings exist during `OnReady`.

Create one builder and reuse it for multiple GameObjects — each `Build` resets the builder's internal state. Avoid creating a new builder per GameObject, as that defeats the purpose of reuse.

```csharp
GameObjectBuilder builder = gameWorld.CreateGameObjectBuilder();

builder
    .With(new TransformComponent { Position = pos })
    .With<AnimatedSpriteComponent>()
    .With<SilhouetteComponent>()
    .Build();

// Reuse the same builder for the next GameObject
builder
    .With(new TransformComponent { Position = otherPos })
    .With<CreatureAnimationComponent>()
    .Build();
```

Extension methods on `GameObjectBuilder` are a convenient way to bundle related components:

```csharp
public static class GameObjectBuilderExtensions
{
    public static GameObjectBuilder WithUnitComponents(this GameObjectBuilder builder, Vector2 position)
    {
        return builder
            .With(new TransformComponent { Position = position })
            .With<AnimatedSpriteComponent>()
            .With<SilhouetteComponent>();
    }
}

// Usage
builder.WithUnitComponents(pos).With<ArcherAIComponent>().Build();
```

## Key Points

- Components have explicit lifecycle via `OnAttach`/`OnReady`/`OnDetach`
- Use `GameObjectBuilder` when creating GameObjects with interdependent components
- `Attach` on a live GameObject returns the attached component
- Updates require manual registration with `UpdateSystem`
- Use `Services<T>.Instance` to access registered services
- Always clean up subscriptions in `OnDetach`
