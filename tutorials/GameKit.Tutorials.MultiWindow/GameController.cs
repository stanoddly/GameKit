using GameKit.App;
using GameKit.Input;

namespace GameKit.Tutorials.MultiWindow;

internal sealed class GameController : IDisposable
{
    private readonly WindowManager _windows;
    private readonly IStageManager _stages;
    private readonly IMouseService _mouse;

    public GameController(
        WindowManager windows,
        IStageManager stages,
        IMouseService mouse)
    {
        _windows = windows;
        _stages = stages;
        _mouse = mouse;
        _mouse.ButtonPress += OnButtonPress;
    }

    public void Dispose()
    {
        _mouse.ButtonPress -= OnButtonPress;
    }

    private void OnButtonPress(Mouse mouse, MouseButtonEventArgs eventArgs)
    {
        if (eventArgs.Button == MouseButton.Right)
        {
            _stages.Load(MenuStage.Configure);
            return;
        }

        if (eventArgs.Button != MouseButton.Left ||
            _windows.IsWindowOpen(GameStage.SecondaryWindowName))
        {
            return;
        }

        _windows.CreateWindow(GameStage.SecondaryWindowName, new WindowOptions(
            Size: new Size<uint>(480, 360),
            Title: "Secondary window - right-click to return"));
    }
}
