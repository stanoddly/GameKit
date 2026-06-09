# GameKit.Architecture

The CQS + domain-event infrastructure for a Model layer: command/query handler contracts, a command
dispatcher with post-dispatch hooks, and a pull-based domain-event stream. For the reasoning behind
the pattern see [architecture-concept.md](architecture-concept.md); for the tests that enforce it see
[architecture-testing.md](architecture-testing.md).

## Commands and queries

A command is a mutation request; its handler returns whether it was handled. A query is a side-effect-free
read; its handler returns the result.

```csharp
public sealed record MoveCommand(UnitId Unit, TilePoint Destination);

internal sealed class MoveCommandHandler : ICommandHandler<MoveCommand>
{
    private readonly UnitRegistry _units;
    internal MoveCommandHandler(UnitRegistry units) => _units = units;

    public bool Handle(MoveCommand command) { /* mutate */ return true; }
}

public sealed record MovementRangeQuery(UnitId Unit);

internal sealed class MovementRangeQueryHandler : IQueryHandler<MovementRangeQuery, MovementRange>
{
    public MovementRange Handle(MovementRangeQuery query) { /* compute */ }
}
```

Handlers are **internal** and constructor-injected; the command/query records are the public surface.
Register each as its closed interface so cross-assembly callers depend on the contract, not the handler:

```csharp
services.AddSingleton<ICommandHandler<MoveCommand>, MoveCommandHandler>();
services.AddSingleton<IQueryHandler<MovementRangeQuery, MovementRange>, MovementRangeQueryHandler>();
```

Queries are invoked directly — inject `IQueryHandler<TQuery, TResult>` where you need it and call `Handle`.
Commands go through the dispatcher.

## Dispatching commands

`AddCommandDispatching()` registers `ICommandDispatcher`. Inject it and dispatch; it resolves the
handler for the closed command type:

```csharp
services.AddCommandDispatching();

// in a caller (e.g. a Presenter):
_dispatcher.Dispatch(new MoveCommand(unit, destination));
```

`Dispatch` is depth-gated. A handler may dispatch further commands; those share the same batch. When the
**top-level** dispatch returns, every `IPostDispatchHook.OnBatchCompleted()` runs once, in registration
order — re-entrant commands do not re-trigger the hooks.

```csharp
internal sealed class TurnTriggerHook : IPostDispatchHook
{
    public void OnBatchCompleted() { /* run end-of-batch work */ }
}
services.AddSingleton<TurnTriggerHook>();
services.AddAlias<IPostDispatchHook, TurnTriggerHook>();
```

## Domain events

Handlers raise discrete domain events through `IDomainEventPublisher`. Events derive from the
recipient-less `DomainMessage` base; recipient/routing fields are a game concern, added in a derived base.

```csharp
public abstract record FactionDomainMessage(Faction Recipient) : DomainMessage;
public sealed record UnitMovedEvent(Faction Recipient, UnitId Unit) : FactionDomainMessage(Recipient);

// in a handler:
_publisher.Publish(new UnitMovedEvent(faction, unit));
```

`AddDomainEvents()` registers the stream as `IDomainEventPublisher` (write) and `IDomainEventStream`
(read). The stream is a ring buffer; each consumer reads through its own `DomainEventCursor`, so multiple
consumers drain at independent paces and a slow consumer doesn't drop events for the others.

## Consuming events

Two consumption models — pick by **cadence**, not preference:

**Own a cursor, drain in your loop** — for consumers that react on their own clock (View/render,
audio, per-frame AI). This is the point of per-consumer cursors.

```csharp
internal sealed class UnitSpritePresenter : IUpdatable
{
    private readonly DomainEventCursor _events;
    internal UnitSpritePresenter(IDomainEventStream events) => _events = events.CreateCursor();

    public void Update()
    {
        while (_events.TryRead(out DomainMessage? message))
        {
            // recipient filter + per-type routing here
        }
    }
}
```

**`DomainEventPump` + `IDomainEventListener`** — for reactions that must run *within the command batch*
(scenario triggers, AI that issues follow-up commands before control returns). The pump is an
`IPostDispatchHook` that drains the buffered events after each batch and fans each to every listener.

```csharp
internal sealed class DialogTrigger : IDomainEventListener
{
    public bool TryProcess(DomainMessage message) { /* react, maybe dispatch */ return true; }
}

services.AddDomainEventPump();                          // requires AddDomainEvents + AddCommandDispatching
services.AddSingleton<DialogTrigger>();
services.AddAlias<IDomainEventListener, DialogTrigger>();
```

Do not push View consumers through the pump — that ties rendering reactions to the model's dispatch
path instead of the frame loop.

## Registration summary

```csharp
services.AddDomainEvents();        // DomainEventStream as IDomainEventPublisher + IDomainEventStream
services.AddCommandDispatching();  // CommandDispatcher as ICommandDispatcher
services.AddDomainEventPump();     // DomainEventPump as IPostDispatchHook (drains to IDomainEventListeners)
```

Then register the game's handlers (closed types), post-dispatch hooks, and event listeners.
