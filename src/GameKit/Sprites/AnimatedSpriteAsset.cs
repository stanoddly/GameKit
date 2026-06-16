using System.Collections.Immutable;
using System.Numerics;
using GameKit.Gpu;

namespace GameKit.Sprites;

public record AnimatedSpriteAsset(
    float FrameDuration,
    Texture Texture,
    ImmutableArray<ShortRectangle> Frames,
    Vector2 AnchorOffset,
    SpriteFlip Flip = SpriteFlip.None)
{
    public Vector4 CalculateTextureRegionUVs(int frameIndex)
    {
        return Texture.CalculateTextureRegionUVs(Frames[frameIndex], Flip);
    }
}
