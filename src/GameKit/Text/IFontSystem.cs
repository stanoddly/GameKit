using GameKit.Common;
using GameKit.Gpu;

namespace GameKit.Text;

public interface IFontSystem: IDisposable
{
    Font Load(string path, ushort size);
    TextSpriteAsset CreateTextSprite(string text, Font font);
    ShortSize MeasureTextSprite(string text, Font font);
    void ReleaseFont(Font font);
}