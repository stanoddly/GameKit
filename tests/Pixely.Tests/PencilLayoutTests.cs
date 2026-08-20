using Pixely.Gpu;
using Pixely.Input;
using Pixely.Pencuil;
using Pixely.Text;

namespace Pixely.Tests;

public sealed class PencilLayoutTests
{
    [Test]
    public void Row_WithNestedOverlay_ContributesArrangedBoundsToParent()
    {
        Pencil pencil = CreatePencil(200, 100);
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
        Pencil pencil = CreatePencil(200, 100);

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
    public void Expanded_DistributesRemainingSpaceByFlex()
    {
        Pencil pencil = CreatePencil(100, 20);

        using (pencil.Sized(100, 20))
        using (pencil.Row(gap: 10, crossAxisAlignment: CrossAxisAlignment.Stretch))
        {
            using (pencil.Expanded())
            {
                pencil.Rectangle(1, 1, Colors.White);
            }

            using (pencil.Expanded(2))
            {
                pencil.Rectangle(1, 1, Colors.Black);
            }
        }

        Assert.That(pencil._coloredRectangleInstructions.Select(instruction => instruction.Area), Is.EqualTo(new[]
        {
            new Rectangle(0, 0, 30, 20),
            new Rectangle(40, 0, 60, 20)
        }));
    }

    [Test]
    public void Align_CentersContentWithinSizedBounds()
    {
        Pencil pencil = CreatePencil(100, 40);

        using (pencil.Sized(100, 40))
        using (pencil.Align(Alignment.Center))
        {
            pencil.Rectangle(20, 10, Colors.White);
        }

        Assert.That(pencil._coloredRectangleInstructions.Single().Area, Is.EqualTo(new Rectangle(40, 15, 20, 10)));
    }

    [Test]
    public void Panel_UsesCompletedLayoutHitAreaOnNextBuild()
    {
        Pencil pencil = CreatePencil(100, 40);
        object view = new();
        pencil.CursorPosition = new Vector2Int(10, 10);

        CursorState initialState = BuildPanel(pencil, view);
        pencil.CycleInstructions();
        pencil.ResetInteractionTests();
        CursorState nextState = BuildPanel(pencil, view);

        Assert.Multiple(() =>
        {
            Assert.That(initialState, Is.EqualTo(CursorState.None));
            Assert.That(nextState, Is.EqualTo(CursorState.Hovered));
        });
    }

    [Test]
    public void Panels_InDifferentViews_HaveIndependentGeneratedIdentity()
    {
        Pencil pencil = CreatePencil(100, 40);
        object leftView = new();
        object rightView = new();
        BuildPanelAt(pencil, leftView, 0);
        BuildPanelAt(pencil, rightView, 50);
        pencil.FinishBuild();
        pencil.CycleInstructions();
        pencil.ResetInteractionTests();
        pencil.CursorPosition = new Vector2Int(60, 10);

        CursorState leftState = BuildPanelAt(pencil, leftView, 0);
        CursorState rightState = BuildPanelAt(pencil, rightView, 50);
        pencil.FinishBuild();

        Assert.Multiple(() =>
        {
            Assert.That(leftState, Is.EqualTo(CursorState.None));
            Assert.That(rightState, Is.EqualTo(CursorState.Hovered));
        });
    }

    [Test]
    public void LayoutBuild_AfterWarmup_DoesNotAllocateManagedMemory()
    {
        Pencil pencil = CreatePencil(100, 40);
        BuildRectangles(pencil);
        pencil.CycleInstructions();
        pencil.ResetInteractionTests();
        BuildRectangles(pencil);
        pencil.CycleInstructions();
        pencil.ResetInteractionTests();

        long before = GC.GetAllocatedBytesForCurrentThread();
        BuildRectangles(pencil);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.That(allocated, Is.Zero);
    }

    private static CursorState BuildPanel(Pencil pencil, object view)
    {
        CursorState state = BuildPanelAt(pencil, view, 0);
        pencil.FinishBuild();
        return state;
    }

    private static CursorState BuildPanelAt(Pencil pencil, object view, int x)
    {
        pencil.BeginLayoutView(view);
        pencil.MoveTo(x, 0);
        CursorState state;
        using (pencil.Sized(40, 20))
        using (pencil.Overlay())
        {
            state = pencil.Panel(40, 20, Colors.White);
        }
        pencil.EndLayoutView(view);
        return state;
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

    private static Pencil CreatePencil(int width, int height)
    {
        Pencil pencil = new(new ThrowingFontSystem(), new TestClipboardService(), GuiStyles.Style);
        pencil.UpdateViewport(width, height);
        pencil.ResetInteractionTests();
        return pencil;
    }

    private sealed class ThrowingFontSystem : IFontSystem
    {
        public Font Load(
            string path,
            ushort size,
            FontRasterizationMode rasterizationMode = FontRasterizationMode.Blended,
            FontHintingMode hintingMode = FontHintingMode.Normal) =>
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

        public string? GetText() => null;

        public void SetText(string text)
        {
        }
    }
}
