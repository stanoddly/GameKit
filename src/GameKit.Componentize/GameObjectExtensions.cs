using GameKit.DependencyInjection;

namespace GameKit.Componentize;

public static class GameObjectExtensions
{
    extension(GameObject gameObject)
    {
        public GameWorld World => gameObject.ServiceProvider.GetRequiredService<GameWorld>();

        public GlobalComponentRegistry GlobalComponents => gameObject.ServiceProvider.GetRequiredService<GlobalComponentRegistry>();
    }
}
