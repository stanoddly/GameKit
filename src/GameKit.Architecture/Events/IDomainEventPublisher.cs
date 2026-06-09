namespace GameKit.Architecture.Events;

public interface IDomainEventPublisher
{
    void Publish(DomainMessage domainMessage);
}
