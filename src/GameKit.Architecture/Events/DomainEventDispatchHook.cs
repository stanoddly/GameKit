using GameKit.Architecture;

namespace GameKit.Architecture.Events;

/// <summary>
/// A model-side command-dispatch hook that drains domain events after each top-level command batch, while the
/// originating <see cref="ICommandDispatcher.Dispatch{TCommand}(TCommand)"/> call is still active, and dispatches
/// them to every registered <see cref="IDomainEventListener"/>.
/// </summary>
/// <remarks>
/// Use this for model-owned reactions that must happen after every command regardless of who dispatched it,
/// such as scenario triggers, objective checks, or AI follow-up commands. Presenter, View, audio, and other
/// frame-loop consumers should usually create their own <see cref="DomainEventCursor"/> from
/// <see cref="IDomainEventStream"/> and drain it on their own cadence instead.
/// </remarks>
public sealed class DomainEventDispatchHook : ICommandDispatchHook
{
    private readonly DomainEventCursor _events;
    private readonly IDomainEventListener[] _listeners;

    public DomainEventDispatchHook(DomainEventCursor events, IEnumerable<IDomainEventListener> listeners)
    {
        _events = events;
        _listeners = listeners.ToArray();
    }

    public void OnBatchCompleted()
    {
        while (_events.TryRead(out DomainMessage? message))
        {
            foreach (IDomainEventListener listener in _listeners)
            {
                listener.TryProcess(message);
            }
        }
    }
}
