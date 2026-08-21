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

internal sealed record UnitBdo(int UnitId);

internal sealed record UnitsInRangeBdo(IReadOnlyList<UnitBdo> Units);

internal sealed record PageBdo<T>(IReadOnlyList<T> Items);

internal sealed class UnitsInRangeBdoQueryHandler : IQueryHandler<UnitsInRangeQuery, UnitsInRangeBdo>
{
    internal UnitsInRangeBdoQueryHandler()
    {
    }

    public UnitsInRangeBdo Handle(UnitsInRangeQuery query) => new([]);
}

internal sealed class PagedUnitsQueryHandler : IQueryHandler<UnitsInRangeQuery, PageBdo<UnitBdo>>
{
    internal PagedUnitsQueryHandler()
    {
    }

    public PageBdo<UnitBdo> Handle(UnitsInRangeQuery query) => new([]);
}

internal sealed record OrphanBdo(int Value);

internal sealed record BdoCommand(UnitBdo Unit);

internal sealed class BdoCommandHandler : ICommandHandler<BdoCommand>
{
    internal BdoCommandHandler()
    {
    }

    public CommandResult Handle(BdoCommand command) => CommandResult.Success;
}

internal sealed record BdoInputQuery(UnitBdo Unit);

internal sealed class BdoInputQueryHandler : IQueryHandler<BdoInputQuery, UnitsInRangeBdo>
{
    internal BdoInputQueryHandler()
    {
    }

    public UnitsInRangeBdo Handle(BdoInputQuery query) => new([]);
}

internal sealed record BdoEvent(UnitBdo Unit) : Pixely.Architecture.Events.DomainMessage;

internal sealed record SettingsBdo(int Volume);

internal sealed record GetSettingsQuery;

internal sealed class GetSettingsQueryHandler : IQueryHandler<GetSettingsQuery, SettingsBdo>
{
    internal GetSettingsQueryHandler()
    {
    }

    public SettingsBdo Handle(GetSettingsQuery query) => new(100);
}

internal sealed record SaveSettingsCommand(SettingsBdo Settings);

internal sealed class SaveSettingsCommandHandler : ICommandHandler<SaveSettingsCommand>
{
    internal SaveSettingsCommandHandler()
    {
    }

    public CommandResult Handle(SaveSettingsCommand command) => CommandResult.Success;
}

internal sealed record MutableBdo
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
