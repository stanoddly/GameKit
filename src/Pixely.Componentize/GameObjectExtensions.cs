using Pixely.DependencyInjection;

namespace Pixely.Componentize;

public static class GameObjectExtensions
{
    extension(GameObject gameObject)
    {
        public GameWorld World => gameObject.ServiceProvider.GetRequiredService<GameWorld>();

        public GlobalComponentRegistry GlobalComponents => gameObject.ServiceProvider.GetRequiredService<GlobalComponentRegistry>();
    }
}
