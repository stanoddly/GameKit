using GameKit.Input;
using GameKit.Pencuil;
using GameKit.Text;

namespace GameKit.Tests;

public class PencilViewportTests
{
    [Test]
    public void UpdateViewport_WithDifferentSize_UpdatesViewportAndInvalidates()
    {
        Pencil pencil = CreatePencil();
        pencil.NeedsUpdate = false;

        pencil.UpdateViewport(300, 200);

        Assert.That(pencil.ViewportSize, Is.EqualTo(new ShortSize(300, 200)));
        Assert.That(pencil.NeedsUpdate, Is.True);
    }

    [Test]
    public void UpdateViewport_WithSameSize_DoesNotInvalidate()
    {
        Pencil pencil = CreatePencil();
        pencil.UpdateViewport(300, 200);
        pencil.NeedsUpdate = false;

        pencil.UpdateViewport(300, 200);

        Assert.That(pencil.NeedsUpdate, Is.False);
    }

    [Test]
    public void MarkInstructionsCompleted_TracksViewportUsedByCompletedInstructions()
    {
        Pencil pencil = CreatePencil();

        pencil.UpdateViewport(100, 80);
        pencil.MarkInstructionsCompleted();
        pencil.UpdateViewport(300, 200);

        Assert.That(pencil.ViewportSize, Is.EqualTo(new ShortSize(300, 200)));
        Assert.That(pencil.CompletedInstructionViewportSize, Is.EqualTo(new ShortSize(100, 80)));
    }

    private static Pencil CreatePencil()
    {
        return new Pencil(new ViewScope(0), new ThrowingFontSystem(), new TestClipboardService(), GuiStyles.Style);
    }

    private sealed class TestClipboardService : IClipboardService
    {
        public bool HasText => false;

        public string? GetText() => null;

        public void SetText(string text)
        {
        }
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
}
