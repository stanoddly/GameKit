using GameKit.App;
using GameKit.Input;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.MultiWindow;

internal sealed class GameController : IDisposable
{
    private readonly WindowManager _windows;
    private readonly IWindowRendering<SecondaryRenderContext> _secondaryRendering;
    private readonly IStageManager _stages;
    private readonly IMouseService _mouse;
    private IWindowRenderBinding? _secondaryWindow;

    public GameController(
        WindowManager windows,
        IWindowRendering<SecondaryRenderContext> secondaryRendering,
        IStageManager stages,
        IMouseService mouse)
    {
        _windows = windows;
        _secondaryRendering = secondaryRendering;
        _stages = stages;
        _mouse = mouse;
        _mouse.ButtonPress += OnButtonPress;
    }

    public void Dispose()
    {
        _mouse.ButtonPress -= OnButtonPress;
        _secondaryWindow?.Dispose();
    }

    private void OnButtonPress(Mouse mouse, MouseButtonEventArgs eventArgs)
    {
        if (eventArgs.Button == MouseButton.Right)
        {
            _stages.Load(MenuStage.Configure);
            return;
        }

        if (eventArgs.Button != MouseButton.Left || _secondaryWindow?.IsActive == true)
        {
            return;
        }

        WindowId windowId = _windows.CreateWindow(new WindowOptions(
            Size: new Size<uint>(480, 360),
            Title: "Secondary window - right-click to return"));
        _secondaryWindow = _secondaryRendering.Attach(windowId);
    }
}
