# PR #289 Review

## Findings

1. `DomainEventPump` leaks its `DomainEventCursor`.

   `DomainEventPump` creates a cursor in its constructor, but the pump does not implement `IDisposable`, so DI never calls `DomainEventCursor.Dispose()`. In child/scoped providers using a parent `DomainEventStream`, disposing the child leaves a stalled cursor in the parent stream. After 8192 events, `DomainEventStream.Publish` starts throwing. Make `DomainEventPump : IDisposable` and dispose `_events`.

   References: `src/GameKit.Architecture/Events/DomainEventPump.cs:12`, `src/GameKit.Architecture/Events/DomainEventStream.cs:17`

2. Domain event draining is registration-order dependent.

   `CommandDispatcher` runs post-dispatch hooks once in registration order. Since `DomainEventPump` is just one hook, events published by any later `IPostDispatchHook` remain buffered until a future top-level dispatch. That contradicts the docs' "drains the buffered events after each batch" behavior. Either guarantee/register the pump last, document that hooks after the pump must not publish events, or make the dispatcher/pump drain until the batch is actually quiescent.

   References: `src/GameKit.Architecture/CommandDispatcher.cs:31`, `docs/architecture-library.md:108`

3. The concept doc conflicts with the shipped command API.

   `docs/architecture-concept.md` says a command "MUST return void unless it produces an identity", but `ICommandHandler<TCommand>.Handle` always returns `bool`, and the library doc also describes commands returning whether they were handled. One of these should change before merge so consumers do not get contradictory guidance.

   References: `docs/architecture-concept.md:27`, `src/GameKit.Architecture/ICommandHandler.cs:3`

## Verification

```bash
dotnet test tests/GameKit.Architecture.Tests/GameKit.Architecture.Tests.csproj --no-restore
dotnet test tests/GameKit.Architecture.Testing.Tests/GameKit.Architecture.Testing.Tests.csproj --no-restore
git diff --check origin/main...architecture
```

Both test projects passed, and `git diff --check` reported no whitespace issues.
