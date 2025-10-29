using GameKit.Common;
using GameKit.Gpu;
using GameKit.Sprites;

namespace GameKit.Text;

public record TextSpriteAsset(Texture Texture, ShortRectangle ImageRegion) : SpriteAsset(Texture, ImageRegion), IDisposable
{
    public void Dispose()
    {
        Texture.Dispose();
    }
}
