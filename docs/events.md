# Events

`GameKit.Events` provides a small in-process event bus.

## Core API

Implement `IEventHandler<TEventArgs>` on a class that should receive events of type `TEventArgs`:

```csharp
public sealed record DamageEvent(int Amount);

public sealed class DamageEventHandler : IEventHandler<DamageEvent>
{
    public void Process(DamageEvent args)
    {
        Console.WriteLine(args.Amount);
    }
}
```

`EventBus` exposes:

- `Subscribe<TSubscriber>(TSubscriber instance)` - discovers every `IEventHandler<TEventArgs>` interface implemented by `TSubscriber` and subscribes the instance for each event type.
- `Subscribe(object instance, Type type)` - the AOT-friendly path used by DI callbacks when an annotated type is already available.
- `Subscribe<TEventArgs>(IEventHandler<TEventArgs> handler)` - subscribes one handler to one event type directly.
- `Unsubscribe(...)` - symmetric overloads for removing subscriptions.
- `PublishEvent<TEventArgs>(TEventArgs args)` - publishes one event.
- `PublishEvents<TEventArgs>(ReadOnlySpan<TEventArgs> args)` and `PublishEvents<TEventArgs>(List<TEventArgs> args)` - publish a batch in order.

Handlers for the same event type run in subscription order.

## DI Integration

`AddEvents()` integrates the event bus with `GameKit.DependencyInjection`:

```csharp
ServiceCollection services = new();
services.AddEvents();
services.AddSingleton<DamageEventHandler>();

ServiceProvider provider = services.BuildServiceProvider();
EventBus eventBus = provider.GetRequiredService<EventBus>();

eventBus.PublishEvent(new DamageEvent(5));
```

`AddEvents()` registers `EventBus` as a singleton, then wires activation and disposal callbacks. When the provider builds, each singleton is inspected for `IEventHandler<TEventArgs>` interfaces and automatically subscribed. When the provider is disposed, those same services are unsubscribed before their own `Dispose()` methods run.

The integration uses the callback's annotated `Type` parameter instead of `instance.GetType()`, preserving interface metadata for NativeAOT and trimming.
