# Components

GameKit provides a lightweight component system for game logic through `GameKit.Componentize`.

## Core Types

### GameWorld

Container for all game objects. Register it and create objects at startup:

```csharp
builder.RegisterType<GameWorld>();

builder.OnStart((GameWorld gameWorld) =>
{
    GameObjectBuilder builder = gameWorld.CreateGameObjectBuilder();
    builder
        .With<MovementComponent>()
        .With<HealthComponent>()
        .Build();
});
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

### GameComponent and OwnedComponent

There are two base classes for components:

- **`GameComponent`** — minimal base. Lifecycle hooks receive `GameObject` and `ServiceProvider` as parameters. No cached fields.
- **`OwnedComponent`** — extends `GameComponent`. Caches the owner and service provider at attach, exposes `Owner`, `ServiceProvider`, sibling helpers, `World`, and `GetRequiredService`. Provides parameterless `OnAttach()` / `OnReady()` / `OnDetach()` overrides.

Use `OwnedComponent` when your component needs to access siblings, services, or its owner after the lifecycle hooks complete. Use plain `GameComponent` when you only need the parameters during attach/detach and can store what you need in fields yourself.

#### GameComponent (plain)

```csharp
public class MovementComponent : GameComponent
{
    private Handle<UpdateTag> _updateHandle;
    private UpdateSystem _updateSystem;

    protected override void OnAttach(GameObject owner, ServiceProvider services)
    {
        // Cache services you need past attach
        _updateSystem = services.GetRequiredService<UpdateSystem>();
        _updateHandle = _updateSystem.Add(Update);
    }

    protected override void OnDetach(GameObject owner, ServiceProvider services)
    {
        _updateSystem.Remove(_updateHandle);
    }

    private void Update()
    {
        // Called each frame
    }
}
```

#### OwnedComponent

```csharp
public class MovementComponent : OwnedComponent
{
    private Handle<UpdateTag> _updateHandle;

    protected override void OnAttach()
    {
        _updateHandle = GetRequiredService<UpdateSystem>().Add(Update);
    }

    protected override void OnReady()
    {
        // Safe to access siblings here
        HealthComponent health = GetSibling<HealthComponent>();
    }

    protected override void OnDetach()
    {
        GetRequiredService<UpdateSystem>().Remove(_updateHandle);
    }

    private void Update()
    {
        // Called each frame
    }
}
```

Lifecycle hooks:

- **`OnAttach`** — component is placed on the GameObject. Set up self-contained state. When using `GameObjectBuilder`, sibling `OnAttach` may not have run yet.
- **`OnReady`** — all siblings are attached and their `OnAttach` has completed. Safe to resolve sibling references and subscribe to sibling events.
- **`OnDetach`** — component is being removed. Clean up subscriptions and resources.

When attaching to a live GameObject via `Attach`, both `OnAttach` and `OnReady` are called immediately in sequence.

## Services Access

`OwnedComponent` provides `GetRequiredService<T>()` and `GetService<T>()` delegating to the cached `ServiceProvider`. For plain `GameComponent`, capture services from the `services` parameter in `OnAttach` and store them in fields.

## Update Registration

Updates are **not automatic**. Components must explicitly register with `UpdateSystem`:

```csharp
private Handle<UpdateTag> _updateHandle;

protected override void OnAttach()
{
    _updateHandle = GetRequiredService<UpdateSystem>().Add(Update);
}

protected override void OnDetach()
{
    GetRequiredService<UpdateSystem>().Remove(_updateHandle);
}
```

The handle must be stored to unregister later.

## Sibling Components

Sibling access requires `OwnedComponent`:

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

- `GameComponent` receives `GameObject` and `ServiceProvider` as lifecycle parameters; cache what you need in fields
- `OwnedComponent` caches owner and services, providing sibling helpers, `World`, and `GetRequiredService`
- Use `GameObjectBuilder` when creating GameObjects with interdependent components
- `Attach` on a live GameObject returns the attached component
- Updates require manual registration with `UpdateSystem`
- Always clean up subscriptions in `OnDetach`
