using GameKit.DependencyInjection;

namespace GameKit.Events;

public static class EventsServiceCollectionExtensions
{
    public static void AddEvents(this ServiceCollection services)
    {
        EventBus eventBus = new();
        services.AddSingleton(eventBus);
        services.OnActivated(eventBus.Subscribe);
        services.OnDisposing(eventBus.Unsubscribe);
    }
}
