# Components

GameKit provides a lightweight component system for game logic through `GameKit.Componentize`.

## Core Types

### GameWorld

Container for all game objects. Register it and create objects at startup:

```csharp
builder.RegisterType<GameWorld>();

builder.OnStart((GameWorld gameWorld) =>
{
    gameWorld.CreateGameObject()
        .Attach<MovementComponent>()
        .Attach<HealthComponent>();
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
gameWorld.RemoveGameObject(player.Handle); // Detaches all components
```

### GameComponent

Base class for all components. Override `OnAttach` and `OnDetach` for lifecycle:

```csharp
public class MovementComponent : GameComponent
{
    private Handle<UpdateTag> _updateHandle;

    protected override void OnAttach()
    {
        // Subscribe to input, register for updates, etc.
        _updateHandle = Services<UpdateSystem>.Instance.Add(Update);
    }

    protected override void OnDetach()
    {
        // Unsubscribe, cleanup
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
    IKeyboardService keyboard = Services<IKeyboardService>.Instance;
    keyboard.KeyDown += OnKeyDown;
}
```

**Note:** `Services<T>` caches the instance on first access. The service must be registered before any component accesses it.

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

## Events

Components can publish and handle events within a GameObject:

### Publishing Events

```csharp
public readonly record struct DamageTakenArgs(int Amount);

public class HealthComponent : GameComponent
{
    public void TakeDamage(int amount)
    {
        _health -= amount;
        PublishEvent(new DamageTakenArgs(amount));
    }
}
```

### Handling Events

Implement `IComponentEventHandler<T>`:

```csharp
public class DamageEffectComponent : GameComponent, IComponentEventHandler<DamageTakenArgs>
{
    public void HandleEvent(GameObject sender, in DamageTakenArgs args)
    {
        // React to damage
    }
}
```

Event handlers are automatically subscribed when the component is attached.

## Key Points

- Components have explicit lifecycle via `OnAttach`/`OnDetach`
- Updates require manual registration with `UpdateSystem`
- Use `Services<T>.Instance` to access registered services
- Events are scoped to a single GameObject
- Always clean up subscriptions in `OnDetach`
