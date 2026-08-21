using Pixely.Architecture;
using Pixely.Architecture.Events;

namespace Pixely.Architecture.Testing.Tests.BoundaryFixtures;

// A small Model surface used to exercise ModelBoundary reachability in isolation.

// Command, public — part of the surface.
public record SpawnCommand(SpawnRequest Request);

// Reachable only through SpawnCommand's property — proves the transitive walk works.
public record SpawnRequest(int Count);

// Internal handler — not part of the public surface, discovered via its interface.
internal sealed class SpawnCommandHandler : ICommandHandler<SpawnCommand>
{
    internal SpawnCommandHandler()
    {
    }

    public CommandResult Handle(SpawnCommand command) => CommandResult.Success;
}

// Query whose result type is public and reachable only via the (internal) handler's Handle return type.
public record CountQuery(int Group);

internal sealed class CountQueryHandler : IQueryHandler<CountQuery, CountBdo>
{
    internal CountQueryHandler()
    {
    }

    public CountBdo Handle(CountQuery query) => new(query.Group);
}

public record CountBdo(int Total);

// Event, public — surface by virtue of deriving from DomainMessage.
public sealed record ThingSpawnedEvent(int Id) : DomainMessage;

// Public type referenced by nothing on the surface — a leak.
public sealed class LeakedInternals
{
    public int Secret { get; set; }
}
