using GameKit.Sprites;
using Yak;

namespace GameKit.Modules;

[Module]
public interface ISpriteAtlas : ISpriteLoading
{
    // Consumer-provided
    SpriteAtlasBuilderConfig SpriteAtlasBuilderConfig { get; }

    [Singleton]
    SpriteAtlasBuilder SpriteAtlasBuilder { get; }
}
