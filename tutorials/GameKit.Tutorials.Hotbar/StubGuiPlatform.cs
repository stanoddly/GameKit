using GameKit.Common;
using GameKit.Gpu;
using GameKit.Pencuil;
namespace GameKit.Tutorials.Hotbar;

public class StubGuiPlatform : IGuiPlatform
{
    public ShortVector2 MeasureString(string text, ushort fontSize) => default;
    public void DrawRectangle(ShortRectangle rectangle, Color color) { }
    public void DrawText(string text, ushort size, Color color) { }
    public void DrawTexture(Texture texture, ShortRectangle region) { }
}
