# Class Registration in GameKit

GameKit uses a dependency injection system built on Microsoft.Extensions.DependencyInjection with custom lifecycle management. All services are registered as singletons and are eagerly instantiated during `Build()`.

## Registration Methods

GameKit provides three registration methods on `GameKitAppBuilder`:

### 1. RegisterInstance

Register an already-created instance.

```csharp
builder.RegisterInstance(new AppConfig { Size = (1280, 720), Title = "Game" });
```

Use when:
- You have a concrete instance to provide
- The object requires no dependencies
- Configuration objects or simple data holders

### 2. RegisterType

Register a type that will be constructed via dependency injection.

```csharp
builder.RegisterType<MyService>();
builder.RegisterType<GameRenderContextProvider>().As<IRenderContextProvider<GameRenderContext>>();
```

Use when:
- The class has dependencies that are already registered
- Constructor parameters will be resolved automatically
- Most common registration method for services

### 3. RegisterFunc

Register a factory method for custom construction logic. Dependencies are declared as method parameters and resolved automatically.

```csharp
// Static factory method with dependencies
builder.RegisterFunc<RenderConfig>(RenderConfig.Create);

// Static method without dependencies
builder.RegisterFunc<Camera>(Camera.CreateDefault);
```

See @guides/static-factory-methods.md for when and how to implement factory methods.

## Interface Registration with .As<T>()

All registration methods return a `GameModuleRegistrar<T>` that allows interface binding via `.As<TInterface>()`:

```csharp
builder.RegisterType<ConcreteService>()
    .As<IService>();

builder.RegisterInstance(new MyConfig())
    .As<IConfiguration>();

builder.RegisterFunc<Implementation>(Implementation.Create)
    .As<IInterface>();
```

The concrete type is registered first, then aliased to the interface. Both can be resolved from the service provider.

## Module Pattern

Organize related registrations into extension methods:

```csharp
public static class GraphicsModuleRegistrar
{
    public static GameKitAppBuilder RegisterGraphics(this GameKitAppBuilder builder)
    {
        builder.RegisterType<CullingService>();
        builder.RegisterType<GeometryStageRenderer>()
            .As<IRenderer<GameRenderContext>>();

        builder.RegisterFunc<Camera>(GameCameraFactory.Create);

        return builder;
    }
}

// Usage
builder.RegisterGraphics();
```

## Lifecycle Hooks

Objects can implement lifecycle interfaces:

- `IInitializable` - `Initialize()` called after construction
- `IUpdatable` - `Update()` called each frame
- `IDisposable` - `Dispose()` called on shutdown

These are automatically detected and wired during registration.

## Important Notes

- All services are singletons
- Services are eagerly instantiated during `Build()`
- Registering the same concrete type twice throws `InvalidOperationException`
- Multiple implementations can be registered as the same interface via `.As<T>()` to support `IEnumerable<T>` constructor injection
- Constructor and factory method parameters are resolved automatically
