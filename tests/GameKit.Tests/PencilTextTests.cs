using GameKit.Gpu;
using GameKit.Input;
using GameKit.Pencuil;
using GameKit.Text;

namespace GameKit.Tests;

public class PencilTextTests
{
    [Test]
    public void Text_WithEmptyString_DoesNotCallFontSystemOrChangeLayoutState()
    {
        Pencil pencil = CreatePencil();
        pencil.MoveTo(10, 20);
        pencil.CurrentSize = new Vector2Int(30, 40);
        pencil.CurrentGap = 5;

        Assert.DoesNotThrow(() => pencil.Text("", null!, Colors.White));

        Assert.That(pencil.CurrentPosition, Is.EqualTo(new Vector2Int(10, 20)));
        Assert.That(pencil.CurrentSize, Is.EqualTo(new Vector2Int(30, 40)));
        Assert.That(pencil.CurrentGap, Is.EqualTo(5));
    }

    [Test]
    public void MeasureText_WithEmptyString_ReturnsZeroAndDoesNotCallFontSystem()
    {
        Pencil pencil = CreatePencil();

        Vector2Int size = pencil.MeasureText("", null!);

        Assert.That(size, Is.EqualTo(default(Vector2Int)));
    }

    private static Pencil CreatePencil()
    {
        return new Pencil(new ThrowingFontSystem(), new TestClipboardService(), GuiStyles.Style);
    }

    private sealed class ThrowingFontSystem : IFontSystem
    {
        public Font Load(
            string path,
            ushort size,
            FontRasterizationMode rasterizationMode = FontRasterizationMode.Blended,
            FontHintingMode hintingMode = FontHintingMode.Normal) =>
            throw new AssertionException("Font system should not be called.");

        public TextSpriteAsset CreateTextSprite(string text, Font font) => throw new AssertionException("Font system should not be called.");

        public ShortSize MeasureTextSprite(string text, Font font) => throw new AssertionException("Font system should not be called.");

        public void ReleaseFont(Font font) => throw new AssertionException("Font system should not be called.");

        public void Dispose()
        {
        }
    }

    private sealed class TestClipboardService : IClipboardService
    {
        public bool HasText => false;

        public string? GetText() => null;

        public void SetText(string text)
        {
        }
    }
}
