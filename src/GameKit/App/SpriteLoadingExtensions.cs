using GameKit.Content;
using GameKit.Sprites;

namespace GameKit.App;

public static class SpriteLoadingExtensions
{
    public static GameKitAppBuilder RegisterSpriteLoading(this GameKitAppBuilder builder)
    {
        builder.RegisterType<SpriteAssetStorage>();
        builder.RegisterType<SpriteAssetLoader>().As<IContentLoader<SpriteAsset>>();
        builder.RegisterType<AnimatedSpriteAssetLoader>().As<IContentLoader<AnimatedSpriteAsset>>();
        return builder;
    }

    public static GameKitAppBuilder RegisterAtlas(this GameKitAppBuilder builder, params string[] paths)
    {
        builder.RegisterInstance(new SpriteAtlasBuilderConfig(paths));
        builder.RegisterFunc<SpriteAtlasBuilder>(SpriteAtlasBuilder.Create);
        return builder;
    }
}
