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

    /// <summary>
    /// Registers the <see cref="CommandDispatcher"/> as <see cref="ICommandDispatcher"/>. The game registers its
    /// <see cref="ICommandHandler{TCommand}"/> implementations as closed types, and any
    /// <see cref="ICommandDispatchHook"/> implementations it wants run after each command batch.
    /// </summary>
    public static ServiceCollection AddCommandDispatching(this ServiceCollection services)
    {
        services.AddSingleton<CommandDispatcher>();
        services.AddAlias<ICommandDispatcher, CommandDispatcher>();
        return services;
    }

    /// <summary>
    /// Registers the <see cref="DomainEventPump"/> as an <see cref="ICommandDispatchHook"/> so buffered domain
    /// events are drained to the registered <see cref="IDomainEventListener"/>s after each command batch.
    /// Register it after any dispatch hook that publishes events. Requires <see cref="AddDomainEvents"/> and
    /// <see cref="AddCommandDispatching"/>.
    /// </summary>
    public static ServiceCollection AddDomainEventPump(this ServiceCollection services)
    {
        services.AddSingleton<DomainEventPump>();
        services.AddAlias<ICommandDispatchHook, DomainEventPump>();
        return services;
    }
}
