# GameKit.Componentize

A component-based game architecture: GameObjects hold GameComponents, components react to events, Behaviors implement state machines, and a ServiceLocator provides global access to shared services.

## Core Types

### GameObject

Container for components. Stores components in a list — multiple components of the same type can coexist. Implements `IEnumerable<GameComponent>`.

```csharp
// Batch creation with two-phase lifecycle
GameObjectBuilder builder = gameWorld.CreateGameObjectBuilder();
builder
    .With<TransformComponent>()
    .With(new DynamicBodyComponent { Radius = 0.4f })
    .Build();

// Single attachment to a live object (returns the component)
GameObject dude = gameWorld.CreateGameObject();
TransformComponent transform = dude.Attach<TransformComponent>();

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
    // setup self-contained state (sibling OnAttach may not have run yet)
    protected override void OnAttach()  { }

    // all siblings attached and initialized, safe to resolve references
    protected override void OnReady()   { }

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

### GameWorld

Registry of GameObjects, identified by `Handle<GameObject>`.

```csharp
GameWorld world = ServiceLocator.GetService<GameWorld>();
GameObject obj = world.CreateGameObject();

// Removes the object and calls DetachAll
world.RemoveGameObject(obj);
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

## Assembly Composition

```
Program.Main
  ├─ Build IServiceProvider               // create services
  ├─ ServiceLocator.SetServiceProvider()   // make them global
  ├─ InitialScene(gameWorld)              // create GameObjects, attach components
  └─ gameKitApp.Run()                     // game loop (systems update/render)
```
