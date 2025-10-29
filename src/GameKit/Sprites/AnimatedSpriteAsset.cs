using System.Collections.Immutable;
using System.Numerics;
using GameKit.Common;
using GameKit.Gpu;

namespace GameKit.Sprites;

public record AnimatedSpriteAsset(
    float FrameDuration,
    Texture Texture,
    ImmutableArray<ShortRectangle> Frames,
    bool Repeat)
{
    public Vector4 CalculateTextureRegionUVs(int frameIndex)
    {
        return Texture.CalculateTextureRegionUVs(Frames[frameIndex]);
    }
}
