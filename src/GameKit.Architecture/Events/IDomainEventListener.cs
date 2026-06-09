namespace GameKit.Architecture.Events;

/// <summary>
/// Reacts to model-side domain events drained by the <see cref="DomainEventDispatchHook"/> after a command batch.
/// Returns whether it consumed the message; the hook fans every message out to all listeners regardless of the
/// result.
/// </summary>
public interface IDomainEventListener
{
    bool TryProcess(DomainMessage message);
}
