using GameKit.DependencyInjection;

namespace GameKit.Componentize;

public static class GameObjectExtensions
{
    extension(GameObject gameObject)
    {
        public GameWorld World => gameObject.GetRequiredService<GameWorld>();

        public GlobalComponentRegistry GlobalComponents => gameObject.GetRequiredService<GlobalComponentRegistry>();
    }
}
