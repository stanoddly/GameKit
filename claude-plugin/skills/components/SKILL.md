---
name: components
description: GameKit component model, lifecycle, dependency injection, registration patterns, state machines, and collections reference.
user-invocable: false
---

## Dependency Injection

GameKit uses Microsoft.Extensions.DependencyInjection with custom lifecycle. All services are singletons, eagerly instantiated during `Build()`.

### Registration Methods

**RegisterInstance** — pre-created objects, no dependencies:
```csharp
builder.RegisterInstance(new AppConfig { Size = (1280, 720), Title = "Game" });
```

**RegisterType** — auto-resolved constructor parameters:
```csharp
builder.RegisterType<MyService>();
builder.RegisterType<GameRenderContextProvider>().As<IRenderContextProvider<GameRenderContext>>();
```

**RegisterFunc** — factory method, parameters resolved automatically:
```csharp
builder.RegisterFunc<RenderConfig>(RenderConfig.Create);
```

### Interface Binding

All registration methods support `.As<T>()` to alias to an interface:
```csharp
builder.RegisterType<ConcreteService>().As<IService>();
```

Both concrete and interface types can be resolved. Multiple implementations of the same interface support `IEnumerable<T>` injection.

### Static Factory Method Pattern

Use when construction involves complex setup or intermediate objects:
```csharp
public class MyService
{
    public static MyService Create(IDependency1 dep1, IDependency2 dep2)
    {
        ComplexObject obj = dep1.CreateComplexObject(dep2.CreateDifferentObject());
        return new MyService(obj);
    }

    private MyService(ComplexObject complexObject) { }
}
```
Register via `builder.RegisterFunc<MyService>(MyService.Create)`.

### Module Registrar Pattern

Organize related registrations into extension methods:
```csharp
public static class GraphicsModuleRegistrar
{
    public static GameKitAppBuilder RegisterGraphics(this GameKitAppBuilder builder)
    {
        builder.RegisterType<CullingService>();
        builder.RegisterType<GeometryStageRenderer>().As<IRenderer<GameRenderContext>>();
        builder.RegisterFunc<Camera>(GameCameraFactory.Create);
        return builder;
    }
}

// Usage in Program.cs
builder.RegisterGraphics();
```

### Lifecycle Hooks (Service-Level)

Services automatically detected by interface:
- `IInitializable` — `Initialize()` called after construction
- `IStartable` — `Start()` called after all services initialized
- `IUpdatable` — `Update()` called each frame
- `IDisposable` — `Dispose()` called on shutdown

### Important Rules

- All services are singletons
- Registering the same concrete type twice throws `InvalidOperationException`
- Constructor and factory method parameters are resolved automatically

## App Lifecycle

```csharp
var builder = new GameKitAppBuilder()
    .AddContentFromProjectDirectory("Content")
    .UseDefaultRenderManager<GameRenderContext>();

builder.RegisterInstance(new AppConfig { Size = (1280, 800), Title = "Game" });
builder.RegisterType<GameWorld>();
builder.RegisterGraphics();

builder.OnStart((GameWorld gameWorld) =>
{
    // Create initial scene after all services ready
});

IGameKitApp app = builder.Build();
app.Run();
```

Main loop: frame timing → event processing → update phase → render phase.

## Component Model

### GameWorld

Registry of GameObjects, identified by `Handle<GameObject>`:
```csharp
GameWorld world = ServiceLocator.GetService<GameWorld>();
GameObject obj = world.CreateGameObject();
world.RemoveGameObject(obj); // Calls DetachAll
```

### GameObject

Container for components. Multiple components of the same type can coexist.
```csharp
GameObject player = gameWorld.CreateGameObject();
player.Attach<MovementComponent>();
player.Attach(new HealthComponent(100));

MovementComponent movement = player.Get<MovementComponent>();
HealthComponent? health = player.TryGet<HealthComponent>();

player.Detach<HealthComponent>();
```

### GameComponent and OwnedComponent

Two base classes are available:

**`GameComponent`** — minimal base. Lifecycle hooks receive `GameObject` and `ServiceProvider` as parameters. No cached fields or owner access between calls.
```csharp
public class MyComponent : GameComponent
{
    // Self-contained setup. Cache services in fields if needed after attach.
    protected override void OnAttach(GameObject owner, ServiceProvider services) { }

    // Cleanup subscriptions and resources.
    protected override void OnDetach(GameObject owner, ServiceProvider services) { }
}
```

**`OwnedComponent`** — extends `GameComponent`. Caches the owner and service provider at attach time. Provides parameterless lifecycle overrides, sibling helpers, and `Owner`/`ServiceProvider`/`World`/`GetRequiredService`/`HasOwner()`.
```csharp
public class MyComponent : OwnedComponent
{
    // Self-contained setup. Sibling OnAttach may not have run yet.
    protected override void OnAttach() { }

    // All siblings attached and initialized. Safe to resolve sibling references.
    protected override void OnReady() { }

    // Cleanup subscriptions and resources.
    protected override void OnDetach() { }
}
```

When attaching to a live GameObject via `Attach`, both `OnAttach` and `OnReady` run immediately in sequence.

**Sibling access** (requires `OwnedComponent`):
- `GetSibling<T>()` — get or throw
- `TryGetSibling<T>()` — get or null
- `AttachSibling<T>(t)` — attach instance
- `DetachSibling<T>()` — detach first match

### GameObjectBuilder

Two-phase lifecycle: all `OnAttach` runs first, then all `OnReady`. Reuse one builder for multiple GameObjects:

```csharp
GameObjectBuilder builder = gameWorld.CreateGameObjectBuilder();

builder
    .With(new TransformComponent { Position = pos })
    .With<AnimatedSpriteComponent>()
    .With<SilhouetteComponent>()
    .Build();

// Reuse for next object
builder
    .With(new TransformComponent { Position = otherPos })
    .With<CreatureAnimationComponent>()
    .Build();
```

**Extension methods** bundle related components:
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

## ServiceLocator & Services\<T\>

Static global registry backed by `IServiceProvider`. Set once at startup:
```csharp
ServiceLocator.SetServiceProvider(serviceProvider);
```

Generic cached access anywhere:
```csharp
Services<SpriteStorage>.Instance
Services<GameWorld>.Instance
```

## Update Registration

Updates are **not automatic**. Components register with `UpdateSystem` explicitly:
```csharp
public class MovementComponent : OwnedComponent
{
    private Handle<UpdateTag> _updateHandle;

    protected override void OnAttach()
    {
        _updateHandle = Services<UpdateSystem>.Instance.Add(Update);
    }

    protected override void OnDetach()
    {
        Services<UpdateSystem>.Instance.Remove(_updateHandle);
    }

    private void Update() { /* Called each frame */ }
}
```

## Behaviors (State Machines)

`Behavior<TSelf>` extends GameComponent. Each behavior type defines a state machine slot — only one concrete behavior per slot exists at a time.

```csharp
// Define the slot
public abstract class PlayerBehavior : Behavior<PlayerBehavior>;

// Define states
public class PlayerIdleBehavior : PlayerBehavior { }
public class PlayerMovingBehavior : PlayerBehavior { }
```

**Transitions:** `ReplaceState(new NextState())` swaps the current behavior.

**Dynamic composition** — behaviors can add/remove components tied to their lifecycle:
```csharp
protected override void OnAttach()
{
    AttachSibling(new DigBlockHighlighterComponent());
}
protected override void OnDetach()
{
    DetachSibling<DigBlockHighlighterComponent>();
}
```

## Storage-Backed Components

Component owns a handle into external storage, creates on attach, removes on detach:
```csharp
public class DynamicBodyComponent : OwnedComponent
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

## Inter-Component Communication

Use native C# events. Wire in `OnAttach` or `OnReady`, unwire in `OnDetach`. `GetSibling` requires `OwnedComponent`:
```csharp
protected override void OnReady()
{
    GetSibling<TransformComponent>().PositionChanged += OnPositionChanged;
}

protected override void OnDetach()
{
    GetSibling<TransformComponent>().PositionChanged -= OnPositionChanged;
}
```

## Collections

- `Handle<T>` — type-safe entity identity
- `DenseSlotMap` — slot map with dense storage
- `FastList<T>` — fast list for performance-critical paths
- `MultiArray` — multi-array storage
- `SparseSet` — sparse set for entity tracking

## Assembly Composition

```
Program.Main
  ├─ Build IServiceProvider               // create services
  ├─ ServiceLocator.SetServiceProvider()   // make them global
  ├─ OnStart callback: create scene       // create GameObjects, attach components
  └─ gameKitApp.Run()                     // game loop
```
