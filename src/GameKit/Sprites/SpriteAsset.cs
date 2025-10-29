using System.Numerics;
using GameKit.Common;
using GameKit.Gpu;

namespace GameKit.Sprites;

public record SpriteAsset(Texture Texture, ShortRectangle ImageRegion)
{
    public Vector4 CalculateTextureRegionUVs() => Texture.CalculateTextureRegionUVs(ImageRegion);

    public ShortVector2 Size => ImageRegion.Size;
}
