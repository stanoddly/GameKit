using GameKit.DependencyInjection;

namespace GameKit.Events;

public static class EventsServiceCollectionExtensions
{
    public static void AddEvents(this ServiceCollection services)
    {
        services.AddSingleton<EventBus>();
        services.AddActivationCallback(static (instance, type, sp) =>
            sp.GetRequiredService<EventBus>().Subscribe(instance, type));
        services.AddDisposalCallback(static (instance, type, sp) =>
            sp.GetRequiredService<EventBus>().Unsubscribe(instance, type));
    }
}
