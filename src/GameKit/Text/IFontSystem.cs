using GameKit.Gpu;

namespace GameKit.Text;

public interface IFontSystem: IDisposable
{
    Font Load(
        string path,
        ushort size,
        FontRasterizationMode rasterizationMode = FontRasterizationMode.Blended,
        FontHintingMode hintingMode = FontHintingMode.Normal);
    TextSpriteAsset CreateTextSprite(string text, Font font);
    ShortSize MeasureTextSprite(string text, Font font);
    void ReleaseFont(Font font);
}
