# GameKit.Componentize

A component-based game architecture: GameObjects hold GameComponents, components react to events, Behaviors implement state machines, and a ServiceLocator provides global access to shared services.

## Core Types

### GameObject

Container for components. Stores components in a list — multiple components of the same type can coexist. Implements `IEnumerable<GameComponent>`.

```csharp
Handle<GameObject> dudeHandle = gameWorld.CreateGameObject();
GameObject dude = gameWorld.GetGameObject(dudeHandle)!;

// new() shorthand and instance attachment (chainable)
dude.Attach<TransformComponent>()
    .Attach(new DynamicBodyComponent { Radius = 0.4f });

// Get throws ComponentNotFound
dude.Get<TransformComponent>();
// TryGet returns null
dude.TryGet<TransformComponent>();
dude.GetComponents<TransformComponent>();

// Detach removes first match
dude.Detach<TransformComponent>();
dude.Detach(componentInstance);
// DetachAll removes all
dude.DetachAll<TransformComponent>();
dude.DetachAll();
```

Collection initializer syntax is also supported via `Add<T>()`.

### GameComponent

Base class. Has an `Owner` (the parent GameObject) and lifecycle hooks.

```csharp
public class MyComponent : GameComponent
{
    // setup, cache siblings
    protected override void OnAttach()  { }

    // cleanup
    protected override void OnDetach()  { }
}
```

**Sibling access** (delegates to Owner):

| Method | Behavior |
|---|---|
| `GetSibling<T>()` | Get or throw |
| `TryGetSibling<T>()` | Get or null |
| `AttachSibling<T>(t)` | Attach instance |
| `DetachSibling<T>()` | Detach first match |

**Other members:**
- `HasOwner()` — returns true if attached to a GameObject
- `PublishEvent(in TEventArgs)` — publish an event to the Owner's subscribers

### GameWorld

Registry of GameObjects, identified by `Handle<GameObject>`.

```csharp
GameWorld world = ServiceLocator.GetService<GameWorld>();
Handle<GameObject> handle = world.CreateGameObject();
GameObject? obj = world.GetGameObject(handle);

// Removes the object and calls DetachAll
world.RemoveGameObject(handle);
```

## Events (PubSub)

Components that implement `IComponentEventHandler<TEventArgs>` are **automatically subscribed** when attached and unsubscribed when detached. `TEventArgs` must be a struct.

```csharp
public class SpriteComponent : GameComponent, IComponentEventHandler<PositionChangedArgs>
{
    public void HandleEvent(GameObject gameObject, in PositionChangedArgs args)
    {
        _storage.UpdatePosition(_handle, args.Value);
    }
}
```

A component can handle multiple event types:

```csharp
public class DebugComponent : GameComponent,
    IComponentEventHandler<PositionChangedArgs>,
    IComponentEventHandler<DirectionChangedArgs>
{ ... }
```

Built-in event emitted by GameObject:
- `ComponentAddedArgs(GameComponent)` — after any Attach

**Manual subscription** to another GameObject's events:

```csharp
_otherGameObject.Subscribe(this);
_otherGameObject.Unsubscribe(this);
```

## Behaviors (State Machines)

`Behavior<TSelf>` extends GameComponent. Each behavior type defines a state machine slot on its GameObject. Only one concrete behavior per slot exists at a time.

```csharp
// Define the slot
public abstract class PlayerBehavior : Behavior<PlayerBehavior>;

// Define states
public class PlayerIdleBehavior : PlayerBehavior { ... }
public class PlayerMovingBehavior : PlayerBehavior { ... }
```

**State transitions** — `ReplaceState(new NextState())` swaps the current behavior by calling `AttachSibling`, which adds the new state alongside the old one.

**Dynamic composition** — behaviors can add/remove components tied to their lifecycle:

```csharp
protected override void OnAttach()
{
    Owner.Attach(new DigBlockHighlighterComponent());
}
protected override void OnDetach()
{
    Owner.Detach<DigBlockHighlighterComponent>();
}
```

## ServiceLocator

Static global registry backed by `IServiceProvider`. Set once at startup, retrieve anywhere.

```csharp
// Registration — once, at boot
ServiceLocator.SetServiceProvider(serviceProvider);

// Retrieval — anywhere
var world = ServiceLocator.GetService<GameWorld>();
```

Throws `InvalidOperationException` if a service is not registered.

### Services

Generic static class that caches service lookups:

```csharp
// Access a service anywhere:
Services<SpriteStorage>.Instance
Services<GameWorld>.Instance
```

## Typical Patterns

**Storage-backed component** — component owns a handle into an external storage, creates on attach, removes on detach:

```csharp
public class DynamicBodyComponent : GameComponent
{
    private Handle<DynamicBodyTag> _handle;

    protected override void OnAttach()
    {
        _handle = Services<DynamicBodiesStorage>.Instance.Create(
            GetSibling<TransformComponent>(), Radius, Speed);
    }

    protected override void OnDetach()
    {
        Services<DynamicBodiesStorage>.Instance.Remove(_handle);
    }
}
```

**Reactive property** — setter publishes an event, other components react:

```csharp
public Vector3 Position
{
    get => _position;
    set { _position = value; PublishEvent(new PositionChangedArgs(value)); }
}
```

## Assembly Composition

```
Program.Main
  ├─ Build IServiceProvider               // create services
  ├─ ServiceLocator.SetServiceProvider()   // make them global
  ├─ InitialScene(gameWorld)              // create GameObjects, attach components
  └─ gameKitApp.Run()                     // game loop (systems update/render)
```
