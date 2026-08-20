using Pixely.Architecture;

namespace Pixely.Architecture.Testing.Tests;

// A clean CQS slice: behaviourless record command/query, internal constructor-injected handlers.

internal sealed class DomainService;

internal record MoveCommand(int X, int Y);

internal sealed class MoveCommandHandler : ICommandHandler<MoveCommand>
{
    internal MoveCommandHandler(DomainService service)
    {
        _ = service;
    }

    public CommandResult Handle(MoveCommand command) => CommandResult.Success;
}

internal record UnitsInRangeQuery(int Radius);

internal sealed class UnitsInRangeQueryHandler : IQueryHandler<UnitsInRangeQuery, int>
{
    internal UnitsInRangeQueryHandler()
    {
    }

    public int Handle(UnitsInRangeQuery query) => query.Radius;
}

internal sealed record UnitQdo(int UnitId);

internal sealed record UnitsInRangeQdo(IReadOnlyList<UnitQdo> Units);

internal sealed record PageQdo<T>(IReadOnlyList<T> Items);

internal sealed class UnitsInRangeQdoQueryHandler : IQueryHandler<UnitsInRangeQuery, UnitsInRangeQdo>
{
    internal UnitsInRangeQdoQueryHandler()
    {
    }

    public UnitsInRangeQdo Handle(UnitsInRangeQuery query) => new([]);
}

internal sealed class PagedUnitsQueryHandler : IQueryHandler<UnitsInRangeQuery, PageQdo<UnitQdo>>
{
    internal PagedUnitsQueryHandler()
    {
    }

    public PageQdo<UnitQdo> Handle(UnitsInRangeQuery query) => new([]);
}

internal sealed record OrphanQdo(int Value);

internal sealed record QdoCommand(UnitQdo Unit);

internal sealed class QdoCommandHandler : ICommandHandler<QdoCommand>
{
    internal QdoCommandHandler()
    {
    }

    public CommandResult Handle(QdoCommand command) => CommandResult.Success;
}

internal sealed record QdoInputQuery(UnitQdo Unit);

internal sealed class QdoInputQueryHandler : IQueryHandler<QdoInputQuery, UnitsInRangeQdo>
{
    internal QdoInputQueryHandler()
    {
    }

    public UnitsInRangeQdo Handle(QdoInputQuery query) => new([]);
}

internal sealed record QdoEvent(UnitQdo Unit) : Pixely.Architecture.Events.DomainMessage;

internal sealed record MutableQdo
{
    public int Value { get; set; }

    public int Increment() => Value + 1;
}

// Deliberate violations, each isolated so a single rule fires.

// Command is a class, not a record, and carries behaviour.
internal sealed class BadCommand
{
    public int Value { get; set; }

    public void Mutate() => Value++;
}

internal sealed class BadCommandHandler : ICommandHandler<BadCommand>
{
    internal BadCommandHandler()
    {
    }

    public CommandResult Handle(BadCommand command) => CommandResult.Success;
}

// Handler is public and has a public constructor.
public record PublicCommand(int X);

public sealed class PublicCommandHandler : ICommandHandler<PublicCommand>
{
    public PublicCommandHandler()
    {
    }

    public CommandResult Handle(PublicCommand command) => CommandResult.Success;
}

// Command handler depends on another command handler.
internal record ChainingCommand(int X);

internal sealed class ChainingCommandHandler : ICommandHandler<ChainingCommand>
{
    internal ChainingCommandHandler(MoveCommandHandler other)
    {
        _ = other;
    }

    public CommandResult Handle(ChainingCommand command) => CommandResult.Success;
}

// Handler whose name does not end with the required suffix.
internal record OddlyNamedCommand(int X);

internal sealed class OddlyNamedExecutor : ICommandHandler<OddlyNamedCommand>
{
    internal OddlyNamedExecutor()
    {
    }

    public CommandResult Handle(OddlyNamedCommand command) => CommandResult.Success;
}
