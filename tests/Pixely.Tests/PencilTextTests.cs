using System.Runtime.CompilerServices;
using Pixely.Gpu;
using Pixely.Input;
using Pixely.Pencuil;
using Pixely.Text;

namespace Pixely.Tests;

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

    [Test]
    public void CompletedInstructions_RetainTextSpriteUntilReplaced()
    {
        (Pencil pencil, WeakReference<TextSpriteAsset> textSprite) = CreateCompletedTextInstructions();

        Assert.That(IsAliveAfterCollection(textSprite), Is.True);

        pencil.CycleInstructions();

        Assert.That(IsAliveAfterCollection(textSprite), Is.False);
    }

    private static Pencil CreatePencil()
    {
        return new Pencil(new ThrowingFontSystem(), new TestClipboardService(), GuiStyles.Style);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (Pencil Pencil, WeakReference<TextSpriteAsset> TextSprite) CreateCompletedTextInstructions()
    {
        TrackingFontSystem fontSystem = new TrackingFontSystem();
        Pencil pencil = new Pencil(fontSystem, new TestClipboardService(), GuiStyles.Style);
        pencil.Text("text", null!, Colors.White);
        pencil.CycleInstructions();
        return (pencil, fontSystem.CreatedTextSprite!);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool IsAliveAfterCollection(WeakReference<TextSpriteAsset> textSprite)
    {
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
        return textSprite.TryGetTarget(out _);
    }

    private sealed class TrackingFontSystem : IFontSystem
    {
        internal WeakReference<TextSpriteAsset>? CreatedTextSprite { get; private set; }

        public Font Load(string path, ushort size, FontRasterizationMode rasterizationMode = FontRasterizationMode.Blended, FontHintingMode hintingMode = FontHintingMode.Normal) =>
            throw new AssertionException("Font loading is not expected.");

        public TextSpriteAsset CreateTextSprite(string text, Font font)
        {
            TextSpriteAsset textSprite = new TextSpriteAsset(new TestTexture(), new ShortRectangle(0, 0, 1, 1));
            CreatedTextSprite = new WeakReference<TextSpriteAsset>(textSprite);
            return textSprite;
        }

        public ShortSize MeasureTextSprite(string text, Font font) => throw new AssertionException("Text measurement is not expected.");

        public void ReleaseFont(Font font) => throw new AssertionException("Font release is not expected.");

        public void Dispose()
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

    private sealed class TestClipboardService : IClipboardService
    {
        public bool HasText => false;

        public string? GetText() => null;

        public void SetText(string text)
        {
        }
    }
}
