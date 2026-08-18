namespace Pixely.Architecture.Events;

public interface IDomainEventPublisher
{
    void Publish(DomainMessage domainMessage);
}
