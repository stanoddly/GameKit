using GameKit.DependencyInjection;

namespace GameKit.Componentize;

public static class GameObjectExtensions
{
    extension(GameObject gameObject)
    {
        public GameWorld World
        {
            get
            {
                ServiceProvider serviceProvider = gameObject.InternalServiceProvider
                    ?? throw new InvalidOperationException("GameObject has been removed and has no World.");
                return serviceProvider.GetRequiredService<GameWorld>();
            }
        }

        public GlobalComponentRegistry GlobalComponents
        {
            get
            {
                ServiceProvider serviceProvider = gameObject.InternalServiceProvider
                    ?? throw new InvalidOperationException("GameObject has been removed.");
                return serviceProvider.GetRequiredService<GlobalComponentRegistry>();
            }
        }
    }
}
