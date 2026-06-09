# Dependency Injection

GameKit's DI container (`GameKit.DependencyInjection`) supports singleton and transient lifetimes. Singletons are instantiated eagerly during `BuildServiceProvider`; transients are constructed lazily each time they are requested. A Roslyn source generator intercepts specific registration overloads at each call site to emit type-safe construction code — several overloads throw at runtime if the generator is not active.

## Overview

- Singleton services have one instance per `ServiceProvider`.
- Transient services create a new instance for each resolution or injection site.
- `BuildServiceProvider` resolves singleton services immediately before returning and records transient factories for later resolution.
- `ServiceProvider` supports a parent chain: resolution falls back to the parent provider when a type is not registered locally.
- `ServiceProvider` itself is automatically registered and resolvable.
- Registration is done through `ServiceCollection`; the built `ServiceProvider` is immutable after `BuildServiceProvider` returns.

## Registration API

### `AddSingleton<T>()` — requires source generator

Registers `T` by constructing it via its single public constructor. Dependencies are resolved from the provider.

```csharp
services.AddSingleton<AudioSystem>();
services.AddSingleton<RenderPipeline>();
```

Use when:
- `T` has exactly one public constructor (or an implicit parameterless constructor).
- All constructor parameters are registered services.

Constraints: `T` must be a named concrete type at the call site — not a type parameter (see [Source Generator Caveats](#source-generator-caveats)).

---

### `AddSingleton<TService, TImplementation>()` — requires source generator

Registers `TImplementation` under the service type `TService`. `TImplementation` is constructed the same way as the single-type overload.

```csharp
services.AddSingleton<IInputService, KeyboardInputService>();
```

Use when:
- You want to resolve a service by an interface or base class.
- `TImplementation` has exactly one public constructor.

---

### `AddSingleton<TService, TFactory>()` — instance factory, requires source generator

When the second type argument is **not** assignable to the first, the source generator treats it as a factory type. The generator finds the single accessible instance method on `TFactory` that returns `TService`, resolves `TFactory` and the method's parameters from the provider, and calls the method to produce the `TService` instance.

```csharp
services.AddSingleton<AudioManager>();
services.AddSingleton<AudioDevice, AudioManager>();
// equivalent to: services.AddSingleton<AudioDevice>(static sp => sp.GetRequiredService<AudioManager>().CreateDevice())
```

Requirements:
- `TFactory` must already be registered (the generator emits `sp.GetRequiredService<TFactory>()`).
- `TFactory` must have exactly one accessible instance method whose return type is assignable to `TService`. Zero matches produce a compile-time error `GK0003`. Multiple matches produce `GK0004`.
- The method's parameters are resolved as services from the provider, the same way constructor parameters are for `AddSingleton<T>()`.

Use when:
- A registered object has a factory method that produces the desired service.
- The service type is different from the factory type (not assignable).

---

### `AddSingleton<T>(T instance)`

Registers an already-constructed instance. No source generator required.

```csharp
services.AddSingleton(new AppConfig { Width = 1280, Height = 720 });
services.AddSingleton<ILogger>(new ConsoleLogger());
```

Use when:
- You have an existing object to hand to the container.
- The instance requires setup that cannot be expressed as a constructor.

---

### `AddSingleton<T>(Delegate factory)` — requires source generator

Registers a factory delegate whose parameters are resolved as services. The delegate may be a static method group or a lambda; its parameter types must all be registered services.

```csharp
services.AddSingleton<Camera>(Camera.CreateDefault);
services.AddSingleton<RenderConfig>(RenderConfig.Create);
```

Use when:
- Construction logic lives in a static factory method.
- The factory has service dependencies as parameters.

---

### `AddSingleton<T>(Func<ServiceProvider, T> factory)`

Registers a typed factory that receives the `ServiceProvider` directly. No source generator required.

```csharp
services.AddSingleton<WorldMap>(static sp =>
{
    MapLoader loader = sp.GetRequiredService<MapLoader>();
    return loader.LoadDefault();
});
```

Use when:
- You need full control over construction, including conditional logic.
- The `Delegate` overload is not usable because `T` is a type parameter at the call site.

---

### `AddSingleton<TService, TImpl>(Func<ServiceProvider, TImpl> factory)`

Registers a typed factory that produces `TImpl` instances under the service type `TService`. Activation and disposal callbacks receive `typeof(TImpl)` rather than `typeof(TService)`. No source generator required.

```csharp
services.AddSingleton<IRenderer, SpriteRenderer>(static sp =>
    new SpriteRenderer(sp.GetRequiredService<GpuDevice>()));
```

Use when:
- The service type is an interface or base class but the concrete implementation type should drive activation/disposal callbacks (e.g. for `EventBus.Subscribe` interface discovery).

---

### `AddTransient<T>()` — requires source generator

Registers `T` as a transient by constructing it via its single public constructor. Dependencies are resolved from the provider each time `T` is requested.

```csharp
services.AddTransient<DomainEventCursor>();
```

Use when:
- Each consumer needs its own instance.
- The instance is short-lived or has per-consumer state.

---

### `AddTransient<TService, TImplementation>()` — requires source generator

Registers `TImplementation` under `TService` as a transient.

```csharp
services.AddTransient<IWidget, HealthBarWidget>();
```

Single-service resolution returns a new instance from the last registration. `GetServices<TService>()` includes transient registrations in registration order and creates fresh transient entries for each collection resolution.

---

### `AddTransient<TService, TFactory>()` — instance factory, requires source generator

When the second type argument is not assignable to the first, the source generator treats it as a factory type, the same way as `AddSingleton<TService, TFactory>()`. The factory method runs for each transient resolution.

---

### `AddTransient<T>(Delegate factory)` — requires source generator

Registers a transient factory delegate whose parameters are resolved as services each time the service is requested.

```csharp
services.AddTransient<ParticleEmitter>(ParticleEmitter.Create);
```

---

### `AddTransient<T>(Func<ServiceProvider, T> factory)`

Registers a typed transient factory that receives the provider directly. No source generator required.

```csharp
services.AddTransient<DomainEventCursor>(static sp =>
    sp.GetRequiredService<IDomainEventStream>().CreateCursor());
```

---

### `AddTransient<TService, TImpl>(Func<ServiceProvider, TImpl> factory)`

Registers a typed transient factory under an interface or base service type. Activation and disposal callbacks receive `typeof(TImpl)`.

---

### `AddAlias<TService, TImplementation>()`

Makes `TService` resolve through the already-registered `TImplementation`. No source generator required. When `TImplementation` is a singleton, the alias resolves to the same instance. When `TImplementation` is transient, the alias creates a transient implementation instance per alias resolution.

```csharp
services.AddSingleton<AudioManager>();
services.AddAlias<IAudioService, AudioManager>();
```

`TImplementation` must be registered before `AddAlias` is called, or an `InvalidOperationException` is thrown.

Use when:
- A concrete type should be resolvable under one or more interface types.
- You want `GetServices<TService>()` to include the implementation instance.

---

### `OnStart(Action<ServiceProvider> action)`

Registers a callback that runs after all services are constructed but before the provider is frozen. No source generator required.

```csharp
services.OnStart(sp =>
{
    sp.GetRequiredService<IStageManager>().Load(stage =>
    {
        stage.AddSingleton<IView, GameplayView>();
    });
});
```

---

### `OnStart(Delegate action)` — requires source generator

Convenience overload that resolves the delegate's parameters as services.

```csharp
services.OnStart((IStageManager stages) =>
{
    stages.Load(stage =>
    {
        stage.AddSingleton<IView, GameplayView>();
    });
});
```

---

### `OnActivated(ServiceActivatedCallback callback)`

Registers a callback invoked immediately after each singleton or transient is constructed. For pre-constructed instances registered with `AddSingleton<T>(T instance)`, it runs when the provider is built.

### `OnDisposing(ServiceDisposingCallback callback)`

Registers a callback invoked during `ServiceProvider.Dispose()`, immediately before a provider-owned service's own `IDisposable.Dispose()` call.

Both delegates receive:

- `object instance` — the service instance.
- `Type type` — the concrete implementation type. Annotated with `DynamicallyAccessedMemberTypes.Interfaces`.

`OnActivated` callbacks fire in the order services are constructed. `OnDisposing` callbacks fire in reverse construction order, matching service disposal. Transient `IDisposable` instances created by the provider are tracked and disposed by the provider that created them. Multiple callbacks of the same kind run in registration order for each service.

The annotated `Type` parameter is important for NativeAOT and trimming. Generator-emitted registrations pass a `typeof(T)` value from an annotated generic type parameter into the callback path, so consumers can inspect interface metadata without falling back to `instance.GetType()`. This is what allows integrations such as `GameKit.Events.AddEvents()` to discover `IEventHandler<T>` implementations in an AOT-clean way.

```csharp
services.OnActivated(static (instance, type) =>
{
    Console.WriteLine($"Activated {type.Name}");
});
```

---

### `IsRegistered(Type)` / `IsRegistered<T>()`

Returns `true` if the type has been registered at least once.

```csharp
if (!services.IsRegistered<DebugOverlay>())
{
    services.AddSingleton<DebugOverlay>();
}
```

---

### `BuildServiceProvider()` / `BuildServiceProvider(ServiceProvider? parent)`

Resolves all services, fires `OnStart` callbacks, freezes the provider, and returns it. The optional `parent` parameter sets up a fallback chain for resolution.

```csharp
ServiceProvider provider = services.BuildServiceProvider();

// Child provider with fallback to a parent
ServiceProvider child = childServices.BuildServiceProvider(parent: provider);
```

## Parent/Child Providers

`BuildServiceProvider(parent)` creates a child provider that inherits from a parent. This is the mechanism behind scoped lifetimes such as stage management — a stage creates a child provider, and disposing the child cleanly tears down only the stage's services.

### Service resolution

A child provider flattens the parent's singleton service array and transient descriptor array into its own at freeze time. Child registrations override parent registrations for the same type (last-wins). After freezing, singleton resolution is a single array lookup; transient resolution uses the flattened transient descriptor array.

```csharp
ServiceCollection rootCollection = new();
rootCollection.AddSingleton(new AppConfig());
ServiceProvider root = rootCollection.BuildServiceProvider();

ServiceCollection stageCollection = new();
stageCollection.AddSingleton<IView>(new GameplayView());
ServiceProvider stage = stageCollection.BuildServiceProvider(parent: root);

// stage can resolve both its own and parent services
AppConfig config = stage.GetRequiredService<AppConfig>();
IView view = stage.GetRequiredService<IView>();
```

### Service collections (`GetServices<T>`)

Multi-registrations compose across the hierarchy: parent entries appear first, followed by child entries. This is the opposite of single-service resolution (where child wins) — collections accumulate. Singleton-only collections are cached as `T[]` and returned without allocation. Collections containing any transient registration are rebuilt on each `GetServices<T>()` call so transient entries are fresh per collection resolution.

### Callback merging

`OnActivated` and `OnDisposing` callbacks registered on the parent's `ServiceCollection` are **merged into the child provider**. When the child provider constructs a service, the parent's `OnActivated` callbacks fire first, then the child's own. When the child provider disposes, its `OnDisposing` callbacks fire first, then the parent's.

This means child services automatically participate in any lifecycle hooks the parent set up. For example, `GameKitAppBuilder` registers `OnActivated` callbacks that add `IUpdatable` services to the `UpdateLoop` and `IView` services to the `ViewRegistry`. A child provider built with `BuildServiceProvider(parent: root)` inherits these callbacks — any `IUpdatable` or `IView` registered in the child is automatically discovered and unregistered on disposal.

```csharp
// Root sets up lifecycle hooks
ServiceCollection rootCollection = new();
UpdateLoop updateLoop = new();
rootCollection.AddSingleton(updateLoop);
rootCollection.OnActivated((instance, _) =>
{
    if (instance is IUpdatable updatable) { updateLoop.Register(updatable); }
});
rootCollection.OnDisposing((instance, _) =>
{
    if (instance is IUpdatable updatable) { updateLoop.Unregister(updatable); }
});
ServiceProvider root = rootCollection.BuildServiceProvider();

// Child inherits the hooks — PhysicsSystem is auto-registered with UpdateLoop
ServiceCollection stageCollection = new();
stageCollection.AddSingleton<IUpdatable, PhysicsSystem>();
ServiceProvider stage = stageCollection.BuildServiceProvider(parent: root);

// Disposing the child auto-unregisters PhysicsSystem from UpdateLoop
stage.Dispose();
```

### Disposal

Disposing a child provider:

1. Detaches from the parent (clears the parent reference).
2. Disposes its own children recursively (deepest first).
3. Disposes transient `IDisposable` instances it created, then walks its own singleton services in reverse creation order — `OnDisposing` callbacks fire, then `IDisposable.Dispose()`.

Parent-owned services are **not** disposed by the child. If a child resolves a transient registration inherited from a parent, the child creates and owns that transient instance. Disposing a parent cascades to all children before disposing its own services.

## Resolution API

### `GetRequiredService<T>()`

Returns the service or throws `InvalidOperationException` if not registered.

```csharp
AudioSystem audio = provider.GetRequiredService<AudioSystem>();
```

When `T` is `IEnumerable<TElement>`, the source generator intercepts the call and redirects it to `GetServices<TElement>()`.

---

### `GetService<T>()`

Returns the service or `null` if not registered.

```csharp
DebugOverlay? overlay = provider.GetService<DebugOverlay>();
```

When `T` is `IEnumerable<TElement>`, the source generator intercepts the call and redirects it to `GetServices<TElement>()`.

---

### `GetServices<T>()`

Returns all instances registered under `T` as `IReadOnlyList<T>`. If every entry is a singleton, the list is a real `T[]` built at `BuildServiceProvider` time and returned without allocation or copying. If any entry is transient, `GetServices<T>()` returns a new `T[]` each call; singleton entries are reused and transient entries are newly constructed.

```csharp
IReadOnlyList<IRenderer> renderers = provider.GetServices<IRenderer>();
foreach (IRenderer renderer in renderers)
{
    renderer.Draw(commandBuffer);
}
```

Returns an empty list if no services of type `T` are registered. Falls back to the parent provider if one is set.

---

### `Dispose()`

Disposes provider-owned services in reverse creation order. Transient `IDisposable` instances created by the provider are disposed before singleton services, so singleton dependencies remain available while transients are torn down. For each disposed service, `OnDisposing` callbacks fire first, then the service's own `IDisposable.Dispose()` runs. Services that are aliased to multiple types are disposed exactly once (deduplicated by reference).

## Lifecycle

1. **Registration** — call `AddSingleton`, `AddTransient`, `AddAlias`, `OnStart`, `OnActivated`, `OnDisposing` on `ServiceCollection`.
2. **`BuildServiceProvider`** — singleton services are instantiated in dependency order; `OnActivated` callbacks fire per singleton instance.
3. **`OnStart` callbacks** — fire in registration order after all singleton services exist.
4. **Freeze** — the provider becomes immutable; build-time resolvers are cleared.
5. **Runtime resolution** — `GetRequiredService`, `GetService`, `GetServices` serve singletons from frozen arrays and construct transients on demand.
6. **`Dispose`** — transient disposables and singleton disposables are visited in reverse creation order: `OnDisposing` callbacks fire, then `IDisposable.Dispose()` runs.

## Multi-Registration and `GetServices<T>`

Registering the same type more than once is allowed. For single-service resolution (`GetRequiredService`, `GetService`), the last registration wins. All registrations are preserved in the collection returned by `GetServices<T>`.

```csharp
services.AddSingleton<IRenderer>(new BackgroundRenderer());
services.AddTransient<IRenderer, SpriteRenderer>();
services.AddSingleton<IRenderer>(new UiRenderer());

// GetRequiredService returns only UiRenderer (last wins)
IRenderer last = provider.GetRequiredService<IRenderer>();

// GetServices returns all three in registration order; SpriteRenderer is fresh per call
IReadOnlyList<IRenderer> all = provider.GetServices<IRenderer>();
```

## Aliases

`AddAlias<TService, TImplementation>()` points `TService` at the already-registered `TImplementation`. The implementation must be registered first. Singleton aliases resolve to the same instance; transient aliases create a transient implementation instance per alias resolution.

```csharp
services.AddSingleton<PhysicsEngine>();
services.AddAlias<IPhysicsService, PhysicsEngine>();
services.AddAlias<ICollisionQuery, PhysicsEngine>();

// All three resolve to the same PhysicsEngine instance
PhysicsEngine engine   = provider.GetRequiredService<PhysicsEngine>();
IPhysicsService svc    = provider.GetRequiredService<IPhysicsService>();
ICollisionQuery query  = provider.GetRequiredService<ICollisionQuery>();
```

Aliases appear in `GetServices<TService>()` collections alongside any direct registrations under `TService`.

## Source Generator Caveats

The following overloads are **intercepted at each call site** by the Roslyn source generator (`GameKit.DependencyInjection.Generator`). Their runtime bodies throw `InvalidOperationException`. They only work when the generator is active and the type arguments are concrete at the call site:

| Overload | Interception requirement |
|---|---|
| `AddSingleton<T>()` | `T` must be a named concrete type, not a type parameter |
| `AddSingleton<TService, TImplementation>()` | Both types must be named concrete types |
| `AddSingleton<T>(Delegate factory)` | `T` must be a named concrete type; delegate argument must be resolvable at compile time |
| `AddTransient<T>()` | `T` must be a named concrete type, not a type parameter |
| `AddTransient<TService, TImplementation>()` | Both types must be named concrete types |
| `AddTransient<T>(Delegate factory)` | `T` must be a named concrete type; delegate argument must be resolvable at compile time |
| `OnStart(Delegate action)` | Delegate argument must be resolvable at compile time |

**The generic-helper failure mode.** If you wrap a call in a generic method where the type argument is itself a type parameter, the generator cannot see the concrete type and will not emit an interceptor. The runtime body throws:

```csharp
// Does NOT work — T is a type parameter, generator cannot intercept
void Register<T>(ServiceCollection services) where T : class
{
    services.AddSingleton<T>(); // throws InvalidOperationException at runtime
}

// Works — concrete type visible at each call site
services.AddSingleton<AudioSystem>();
services.AddSingleton<RenderPipeline>();
```

If you need a generic registration helper, use the non-intercepted overload with a factory:

```csharp
void Register<T>(ServiceCollection services, Func<ServiceProvider, T> factory) where T : class
{
    services.AddSingleton<T>(factory); // Func<ServiceProvider, T> overload — no generator needed
}
```

**Constructor requirements.** `AddSingleton<T>()`, `AddSingleton<TService, TImplementation>()`, `AddTransient<T>()`, and `AddTransient<TService, TImplementation>()` require the implementation type to have exactly one public constructor (or an implicit parameterless constructor). Multiple public constructors produce a compile-time error `GK0002`.

**`IEnumerable<T>` injection.** The generator intercepts `GetRequiredService<IEnumerable<T>>()` and `GetService<IEnumerable<T>>()` at call sites and rewrites them to `GetServices<T>()`. Constructor injection of `IEnumerable<T>` via generated registrations is handled the same way — the generated constructor call uses `sp.GetServices<T>()` for any `IEnumerable<T>` parameter. If a singleton receives an `IEnumerable<T>` containing transient entries, those transient instances are created during singleton construction and captured by that singleton, matching Microsoft.Extensions.DependencyInjection semantics.
