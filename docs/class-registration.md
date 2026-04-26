# Dependency Injection

GameKit's DI container (`GameKit.DependencyInjection`) is singleton-only. All services are instantiated eagerly during `BuildServiceProvider`. A Roslyn source generator intercepts specific registration overloads at each call site to emit type-safe construction code — several overloads throw at runtime if the generator is not active.

## Overview

- All services are singletons — one instance per `ServiceProvider`.
- `BuildServiceProvider` resolves every registered service immediately before returning.
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

### `AddAlias<TService, TImplementation>()`

Makes `TService` resolve to the same instance as the already-registered `TImplementation`. No source generator required.

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
    sp.GetRequiredService<SceneLoader>().LoadInitialScene();
});
```

---

### `OnStart(Delegate action)` — requires source generator

Convenience overload that resolves the delegate's parameters as services.

```csharp
services.OnStart((SceneLoader loader) => loader.LoadInitialScene());
```

---

### `OnActivation(Action<object> callback)`

Registers a callback invoked each time any service instance is first created. Receives the raw `object`. No source generator required.

```csharp
services.OnActivation(instance =>
{
    if (instance is ILoggable loggable)
    {
        loggable.SetLogger(logger);
    }
});
```

---

### `OnDispose(Action<ServiceProvider> callback)`

Registers a callback invoked at the start of `ServiceProvider.Dispose()`, before individual service `Dispose` calls. No source generator required.

```csharp
services.OnDispose(sp =>
{
    sp.GetRequiredService<NetworkManager>().Shutdown();
});
```

---

### Activation and disposal callbacks

`AddActivationCallback(ServiceActivationCallback callback)` registers a typed callback that runs after each singleton is constructed. For pre-constructed instances registered with `AddSingleton<T>(T instance)`, it runs when the provider is built.

`AddDisposalCallback(ServiceDisposalCallback callback)` registers a typed callback that runs during `ServiceProvider.Dispose()`, immediately before the service's own `IDisposable.Dispose()` call if it has one.

Both delegates receive:

- `object instance` - the singleton instance.
- `Type type` - the concrete implementation type. This parameter is annotated with `DynamicallyAccessedMemberTypes.Interfaces`.
- `ServiceProvider provider` - the provider that owns the service.

Activation callbacks run in the order services are constructed. Disposal callbacks run in reverse construction order, matching service disposal. Multiple callbacks of the same kind run in registration order for each service.

The annotated `Type` parameter is important for NativeAOT and trimming. Generator-emitted registrations pass a `typeof(T)` value from an annotated generic type parameter into the callback path, so consumers can inspect interface metadata without falling back to `instance.GetType()`. This is what allows integrations such as `GameKit.Events.AddEvents()` to discover `IEventHandler<T>` implementations in an AOT-clean way.

```csharp
services.AddActivationCallback(static (instance, type, provider) =>
{
    ILogger logger = provider.GetRequiredService<ILogger>();
    logger.Log($"Activated {type.Name}");
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

Returns all instances registered under `T` as `IReadOnlyList<T>`. The list is a real `T[]` built at `BuildServiceProvider` time and returned without allocation or copying.

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

Runs `OnDispose` callbacks, then disposes every registered service that implements `IDisposable` in reverse creation order. Services that are aliased to multiple types are disposed exactly once (deduplicated by reference).

## Lifecycle

1. **Registration** — call `AddSingleton`, `AddAlias`, `OnStart`, `OnActivation`, `OnDispose` on `ServiceCollection`.
2. **`BuildServiceProvider`** — all services are instantiated in dependency order; `OnActivation` callbacks fire per instance.
3. **`OnStart` callbacks** — fire in registration order after all services exist.
4. **Freeze** — the provider becomes immutable; build-time resolvers are cleared.
5. **Runtime resolution** — `GetRequiredService`, `GetService`, `GetServices` serve from the frozen flat array.
6. **`Dispose`** — `OnDispose` callbacks fire first, then services are disposed in reverse creation order.

## Multi-Registration and `GetServices<T>`

Registering the same type more than once is allowed. For single-service resolution (`GetRequiredService`, `GetService`), the last registration wins. All registrations are preserved in the collection returned by `GetServices<T>`.

```csharp
services.AddSingleton<IRenderer>(new BackgroundRenderer());
services.AddSingleton<IRenderer>(new SpriteRenderer());
services.AddSingleton<IRenderer>(new UiRenderer());

// GetRequiredService returns only UiRenderer (last wins)
IRenderer last = provider.GetRequiredService<IRenderer>();

// GetServices returns all three in registration order
IReadOnlyList<IRenderer> all = provider.GetServices<IRenderer>();
```

## Aliases

`AddAlias<TService, TImplementation>()` points `TService` at the same instance as `TImplementation`. The implementation must be registered first.

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

**Constructor requirements.** `AddSingleton<T>()` and `AddSingleton<TService, TImplementation>()` require the implementation type to have exactly one public constructor (or an implicit parameterless constructor). Multiple public constructors produce a compile-time error `GK0002`.

**`IEnumerable<T>` injection.** The generator intercepts `GetRequiredService<IEnumerable<T>>()` and `GetService<IEnumerable<T>>()` at call sites and rewrites them to `GetServices<T>()`. Constructor injection of `IEnumerable<T>` via `AddSingleton<MyService>()` is handled the same way — the generated constructor call uses `sp.GetServices<T>()` for any `IEnumerable<T>` parameter.
