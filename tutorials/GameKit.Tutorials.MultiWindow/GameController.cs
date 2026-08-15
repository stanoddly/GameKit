using GameKit.App;
using GameKit.Input;

namespace GameKit.Tutorials.MultiWindow;

internal sealed class GameController : IDisposable
{
    private readonly IStageManager _stages;
    private readonly IMouseService _mouse;

    public GameController(
        IStageManager stages,
        IMouseService mouse)
    {
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
        }
    }
}
