using GameKit.Content;
using GameKit.Sprites;

namespace GameKit.App;

public static class SpriteLoadingExtensions
{
    public static GameKitAppBuilder RegisterSpriteLoading(this GameKitAppBuilder builder)
    {
        builder.AddSingleton<SpriteAssetStorage>();
        builder.AddSingleton<IContentLoader<SpriteAsset>, SpriteAssetLoader>();
        builder.AddSingleton<IContentLoader<AnimatedSpriteAsset>, AnimatedSpriteAssetLoader>();
        return builder;
    }

    public static GameKitAppBuilder RegisterAtlas(this GameKitAppBuilder builder, params string[] paths)
    {
        builder.AddSingleton(new SpriteAtlasBuilderConfig(paths));
        builder.AddSingleton<SpriteAtlasBuilder>(SpriteAtlasBuilder.Create);
        return builder;
    }
}
