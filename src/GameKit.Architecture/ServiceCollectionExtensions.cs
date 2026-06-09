using GameKit.Architecture.Events;
using GameKit.DependencyInjection;

namespace GameKit.Architecture;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the domain event stream as a singleton, aliased to <see cref="IDomainEventPublisher"/> and
    /// <see cref="IDomainEventStream"/>. Command and query handlers are registered per-game as closed types.
    /// </summary>
    public static ServiceCollection AddDomainEvents(this ServiceCollection services)
    {
        services.AddSingleton<DomainEventStream>();
        services.AddAlias<IDomainEventPublisher, DomainEventStream>();
        services.AddAlias<IDomainEventStream, DomainEventStream>();
        return services;
    }
}
