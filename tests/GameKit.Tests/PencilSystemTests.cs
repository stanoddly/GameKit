using GameKit.Input;
using GameKit.Pencuil;
using GameKit.Text;

namespace GameKit.Tests;

public class PencilSystemTests
{
    [Test]
    public void Update_WithViewportChange_RebuildsViewsUsingNewViewport()
    {
        Pencil pencil = CreatePencil();
        ViewRegistry viewRegistry = new();
        CenterCaptureView view = new();
        TestTextInputService textInputService = new();
        PencilSystem system = PencilSystem.CreateForTests(pencil, viewRegistry, textInputService);

        viewRegistry.Add(view);

        pencil.UpdateViewport(100, 80);
        system.Update();

        pencil.UpdateViewport(300, 200);
        system.Update();

        Assert.That(view.Centers, Is.EqualTo(new[]
        {
            new Vector2Int(50, 40),
            new Vector2Int(150, 100)
        }));
        Assert.That(pencil.CompletedInstructionViewportSize, Is.EqualTo(new ShortSize(300, 200)));
        Assert.That(pencil.NeedsUpdate, Is.False);
    }

    [Test]
    public void UpdateViewport_AfterInstructionCompletion_MarksCompletedInstructionsStale()
    {
        Pencil pencil = CreatePencil();
        ViewRegistry viewRegistry = new();
        CenterCaptureView view = new();
        TestTextInputService textInputService = new();
        PencilSystem system = PencilSystem.CreateForTests(pencil, viewRegistry, textInputService);

        viewRegistry.Add(view);

        pencil.UpdateViewport(100, 80);
        system.Update();

        pencil.UpdateViewport(300, 200);

        Assert.That(pencil.ViewportSize, Is.EqualTo(new ShortSize(300, 200)));
        Assert.That(pencil.CompletedInstructionViewportSize, Is.EqualTo(new ShortSize(100, 80)));
    }

    private static Pencil CreatePencil()
    {
        return new Pencil(new ThrowingFontSystem(), new TestClipboardService(), GuiStyles.Style, new AppConfig());
    }

    private sealed class CenterCaptureView : IView
    {
        public List<Vector2Int> Centers { get; } = new();

        public bool ConsumeDirty() => false;

        public void Build(Pencil pencil)
        {
            Centers.Add(pencil.Center);
        }
    }

    private sealed class TestTextInputService : ITextInputService
    {
        public bool IsActive { get; private set; }

        public event TextInputHandler TextInput
        {
            add { }
            remove { }
        }

        public event TextEditingHandler TextEditing
        {
            add { }
            remove { }
        }

        public bool IsActiveFor(Window window) => IsActive;

        public void Start()
        {
            IsActive = true;
        }

        public void Stop()
        {
            IsActive = false;
        }

        public void Start(Window window)
        {
            IsActive = true;
        }

        public void Stop(Window window)
        {
            IsActive = false;
        }

        public void SubscribeTextInput(int priority, TextInputHandler handler)
        {
        }

        public void SubscribeTextEditing(int priority, TextEditingHandler handler)
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
