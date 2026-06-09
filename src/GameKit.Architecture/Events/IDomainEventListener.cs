namespace GameKit.Architecture.Events;

/// <summary>
/// Reacts to buffered domain events drained by the <see cref="DomainEventPump"/>. Returns whether it consumed
/// the message; the pump fans every message out to all listeners regardless of the result.
/// </summary>
public interface IDomainEventListener
{
    bool TryProcess(DomainMessage message);
}
