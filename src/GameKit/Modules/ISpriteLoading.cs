using GameKit.Content;
using GameKit.Sprites;
using Yak;

namespace GameKit.Modules;

[Module]
public interface ISpriteLoading : IGameKitCore
{
    [Singleton]
    SpriteAssetStorage SpriteAssetStorage { get; }

    [Singleton]
    SpriteAssetLoader SpriteAssetLoader { get; }

    [Singleton]
    AnimatedSpriteAssetLoader AnimatedSpriteAssetLoader { get; }
}
