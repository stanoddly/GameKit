using Pixely.Gpu;
using Pixely.Input;
using Pixely.Pencuil;
using Pixely.Text;

namespace Pixely.Tests;

public sealed class PencilLayoutTests
{
    [Test]
    public void Row_WithNestedOverlay_ContributesBoundsToParent()
    {
        Pencil pencil = CreatePencil();
        pencil.MoveTo(10, 20);

        using (pencil.Row(gap: 4))
        {
            pencil.Rectangle(20, 10, Colors.White);

            using (pencil.Sized(30, 30))
            using (pencil.Overlay(Alignment.Center))
            {
                pencil.Rectangle(30, 30, Colors.Black);
                pencil.Rectangle(10, 10, Colors.White);
            }
        }

        Assert.Multiple(() =>
        {
            Assert.That(pencil.CurrentSize, Is.EqualTo(new Vector2Int(54, 30)));
            Assert.That(pencil._coloredRectangleInstructions.Select(instruction => instruction.Area), Is.EqualTo(new[]
            {
                new Rectangle(10, 20, 20, 10),
                new Rectangle(34, 20, 30, 30),
                new Rectangle(44, 30, 10, 10)
            }));
        });
    }

    [Test]
    public void Padding_WithColumn_IncludesInsetsAndGap()
    {
        Pencil pencil = CreatePencil();

        using (pencil.Padding(5))
        using (pencil.Column(gap: 2))
        {
            pencil.Rectangle(10, 4, Colors.White);
            pencil.Rectangle(20, 6, Colors.White);
        }

        Assert.Multiple(() =>
        {
            Assert.That(pencil.CurrentSize, Is.EqualTo(new Vector2Int(30, 22)));
            Assert.That(pencil.CurrentPosition, Is.EqualTo(new Vector2Int(0, 22)));
            Assert.That(pencil._coloredRectangleInstructions.Select(instruction => instruction.Area), Is.EqualTo(new[]
            {
                new Rectangle(5, 5, 10, 4),
                new Rectangle(5, 11, 20, 6)
            }));
        });
    }

    [Test]
    public void Sized_ContributesFixedBoundsToRow()
    {
        Pencil pencil = CreatePencil();

        using (pencil.Row(gap: 4))
        {
            using (pencil.Sized(30, 20))
            using (pencil.Overlay())
            {
                pencil.Rectangle(10, 5, Colors.White);
            }

            pencil.Rectangle(5, 5, Colors.Black);
        }

        Assert.Multiple(() =>
        {
            Assert.That(pencil.CurrentSize, Is.EqualTo(new Vector2Int(39, 20)));
            Assert.That(pencil._coloredRectangleInstructions.Select(instruction => instruction.Area), Is.EqualTo(new[]
            {
                new Rectangle(0, 0, 10, 5),
                new Rectangle(34, 0, 5, 5)
            }));
        });
    }

    [Test]
    public void Overlay_CentersControlsWithinSizedBounds()
    {
        Pencil pencil = CreatePencil();

        using (pencil.Sized(100, 40))
        using (pencil.Overlay(Alignment.Center))
        {
            pencil.Rectangle(20, 10, Colors.White);
        }

        Assert.That(pencil._coloredRectangleInstructions.Single().Area, Is.EqualTo(new Rectangle(40, 15, 20, 10)));
    }

    [Test]
    public void Panel_UsesCurrentLayoutAreaForInteraction()
    {
        Pencil pencil = CreatePencil();
        pencil.CursorPosition = new Vector2Int(15, 10);
        pencil.MoveTo(10, 5);

        CursorState state;
        using (pencil.Sized(40, 20))
        using (pencil.Overlay())
        {
            state = pencil.Panel(40, 20, Colors.White);
        }

        Assert.Multiple(() =>
        {
            Assert.That(state, Is.EqualTo(CursorState.Hovered));
            Assert.That(pencil.IsOverInteractiveArea(pencil.CursorPosition), Is.True);
        });
    }

    [Test]
    public void Rectangle_DoesNotCreateInteractiveArea()
    {
        Pencil pencil = CreatePencil();
        pencil.CursorPosition = new Vector2Int(5, 5);

        pencil.Rectangle(10, 10, Colors.White);

        Assert.That(pencil.IsOverInteractiveArea(pencil.CursorPosition), Is.False);
    }

    [Test]
    public void AlignedOverlay_WithoutBounds_IsRejected()
    {
        Pencil pencil = CreatePencil();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => pencil.Overlay(Alignment.Center));

        Assert.That(exception.Message, Is.EqualTo("An aligned Overlay requires bounds from a Sized or Padding scope."));
    }

    [Test]
    public void LayoutBuild_AfterWarmup_DoesNotAllocateManagedMemory()
    {
        Pencil pencil = CreatePencil();
        BuildRectangles(pencil);
        pencil.CycleInstructions();
        BuildRectangles(pencil);
        pencil.CycleInstructions();

        long before = GC.GetAllocatedBytesForCurrentThread();
        BuildRectangles(pencil);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.That(allocated, Is.Zero);
    }

    private static void BuildRectangles(Pencil pencil)
    {
        using (pencil.Row(gap: 4))
        {
            pencil.Rectangle(10, 10, Colors.White);
            pencil.Rectangle(10, 10, Colors.Black);
        }

        pencil.FinishBuild();
    }

    private static Pencil CreatePencil()
    {
        Pencil pencil = new(new ThrowingFontSystem(), new TestClipboardService(), GuiStyles.Style);
        pencil.UpdateViewport(200, 100);
        return pencil;
    }

    private sealed class ThrowingFontSystem : IFontSystem
    {
        public Font Load(string path, ushort size, FontRasterizationMode rasterizationMode = FontRasterizationMode.Blended, FontHintingMode hintingMode = FontHintingMode.Normal) =>
            throw new AssertionException("Font system should not be called.");

        public TextSpriteAsset CreateTextSprite(string text, Font font) =>
            throw new AssertionException("Font system should not be called.");

        public ShortSize MeasureTextSprite(string text, Font font) =>
            throw new AssertionException("Font system should not be called.");

        public void ReleaseFont(Font font) =>
            throw new AssertionException("Font system should not be called.");

        public void Dispose()
        {
        }
    }

    private sealed class TestClipboardService : IClipboardService
    {
        public bool HasText => false;

        public string? GetText()
        {
            return null;
        }

        public void SetText(string text)
        {
        }
    }
}
