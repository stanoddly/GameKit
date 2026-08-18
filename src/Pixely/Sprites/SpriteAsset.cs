using System.Numerics;
using Pixely.Gpu;

namespace Pixely.Sprites;

public record SpriteAsset(Texture Texture, ShortRectangle ImageRegion, SpriteFlip Flip = SpriteFlip.None)
{
    public Vector2 AnchorOffset { get; init; }

    public Vector4 CalculateTextureRegionUVs() => Texture.CalculateTextureRegionUVs(ImageRegion, Flip);

    public UShortVector2 Size => ImageRegion.Size;
}
