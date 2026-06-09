using GameKit.Architecture;

namespace GameKit.Architecture.Events;

/// <summary>
/// A post-dispatch hook that drains the buffered domain events accumulated during a command batch and fans
/// each one out to every <see cref="IDomainEventListener"/>. Recipient filtering and per-type routing are the
/// listener's concern.
/// </summary>
public sealed class DomainEventPump : IPostDispatchHook
{
    private readonly DomainEventCursor _events;
    private readonly IDomainEventListener[] _listeners;

    public DomainEventPump(IDomainEventStream events, IEnumerable<IDomainEventListener> listeners)
    {
        _events = events.CreateCursor();
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
