using GameKit.Architecture;

namespace GameKit.Architecture.Testing.Tests;

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

internal sealed record UnitsInRangeResult(IReadOnlyList<int> UnitIds);

internal sealed class UnitsInRangeResultQueryHandler : IQueryHandler<UnitsInRangeQuery, UnitsInRangeResult>
{
    internal UnitsInRangeResultQueryHandler()
    {
    }

    public UnitsInRangeResult Handle(UnitsInRangeQuery query) => new([]);
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
