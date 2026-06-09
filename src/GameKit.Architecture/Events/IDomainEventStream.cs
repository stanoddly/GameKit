namespace GameKit.Architecture.Events;

public interface IDomainEventStream
{
    DomainEventCursor CreateCursor();
}
