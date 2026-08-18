namespace Pixely.Architecture.Events;

public interface IDomainEventStream
{
    DomainEventCursor CreateCursor();
}
