using GameKit.DependencyInjection;

namespace GameKit.Events;

public static class EventsServiceCollectionExtensions
{
    public static void AddEvents(this ServiceCollection services)
    {
        // EventBus is internal, so a registration here is treated as this integration's marker.
        // External callers resolve IEventBus; repeated AddEvents calls must not duplicate callbacks.
        if (services.IsRegistered<EventBus>())
        {
            return;
        }

        EventBus eventBus = new();
        services.AddSingleton(eventBus);
        services.AddAlias<IEventBus, EventBus>();
        services.OnActivated(eventBus.Subscribe);
        services.OnDisposing(eventBus.Unsubscribe);
    }
}
