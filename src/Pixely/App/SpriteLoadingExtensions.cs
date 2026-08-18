using Pixely.Sprites;

namespace Pixely.App;

public static class SpriteLoadingExtensions
{
    public static PixelyAppBuilder RegisterSpriteLoading(this PixelyAppBuilder builder)
    {
        builder.AddSingleton<SpriteAssetStorage>();
        builder.AddSingleton<ISpriteAssetLoader, SpriteAssetLoader>();
        builder.AddSingleton<IAnimatedSpriteAssetLoader, AnimatedSpriteAssetLoader>();
        return builder;
    }

    public static PixelyAppBuilder RegisterAtlas(this PixelyAppBuilder builder, params string[] paths)
    {
        builder.AddSingleton(new SpriteAtlasBuilderConfig(paths));
        builder.AddSingleton<SpriteAtlasBuilder>(SpriteAtlasBuilder.Create);
        return builder;
    }
}
