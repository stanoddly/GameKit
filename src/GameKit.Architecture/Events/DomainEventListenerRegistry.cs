namespace GameKit.Architecture.Events;

public sealed class DomainEventListenerRegistry
{
    private readonly List<IDomainEventListener> _listeners = new();

    public IReadOnlyList<IDomainEventListener> Listeners => _listeners;

    public void Subscribe(IDomainEventListener listener)
    {
        _listeners.Add(listener);
    }

    public void Unsubscribe(IDomainEventListener listener)
    {
        _listeners.Remove(listener);
    }
}
