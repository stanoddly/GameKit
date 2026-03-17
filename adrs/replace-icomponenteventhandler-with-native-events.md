# Replace IComponentEventHandler with native C# events

## Context

Components communicated via `IComponentEventHandler<TEventArgs>` — an interface that `GameObject` auto-subscribed through reflection at attach time. The mechanism had two problems:

1. **Implicit subscription** — a component had no way to know if anyone was listening without chasing pointers through the GameObject's internal handler dictionary.
2. **Indirection on hot paths** — every event dispatch went through a `Dictionary<int, List<object>>` lookup and an `Unsafe.As` cast per subscriber. For high-frequency events (e.g. position changes on thousands of sprites), this indirection blocked optimization.

The full machinery included `IComponentEventHandler<T>`, `ComponentTypeHelper.GetComponentTypeHandledEventArgs` (reflection-based interface scanning with per-type cache), `EventTypeId` / `EventTypeId<T>` (type-to-int mapping), and `Subscribe`/`Unsubscribe`/`PublishEvent` on `GameObject`.

## Decision

Remove the entire `IComponentEventHandler<T>` mechanism. Components expose native C# events instead.

Native C# events solve both original problems: a `null` check on the delegate tells a component whether anyone is subscribed, and custom `add`/`remove` accessors let components react to subscription changes (e.g. syncing a flag into a cache-friendly struct). Subscription is explicit — subscribers wire up in `OnAttach` and unwire in `OnDetach`.

The built-in `ComponentAddedArgs` / `ComponentRemovedArgs` events were also removed. No consumers existed, and `GameWorld.OnComponentAttached<T>` / `OnComponentDetached<T>` already covers the world-level component lifecycle tracking use case.

## Consequences

- Components that previously implemented `IComponentEventHandler<T>` must migrate to subscribing to native C# events on siblings in `OnAttach`/`OnDetach`.
- Publishers replace `PublishEvent(new XArgs(...))` with a simple event invocation (`SomeEvent?.Invoke(...)`).
- Event args record structs can often be eliminated — the data goes directly through the delegate signature.
- `EventTypeId` and `ComponentTypeHelper` are deleted, removing runtime reflection from the component attach path.
- `GameObject` no longer holds a `Dictionary<int, List<object>>` per instance, reducing per-object memory overhead.
