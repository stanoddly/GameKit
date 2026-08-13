using System.Numerics;
using GameKit;
using GameKit.App;
using GameKit.Gpu;
using GameKit.Input;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.WindowDragging;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .UseDefaultRenderCoordinator();

        builder.AddWindow(new WindowOptions(
            Size: (400, 400),
            Title: "Window Dragging",
            Borderless: true));

        builder.AddSingleton<IRenderPhase<DefaultRenderContext>>(static () => new ClearRenderPhase(FColors.SkyBlue));

        builder.OnStart((Window window, IMouseService mouseService, IKeyboardService keyboardService, UpdateSystem updateSystem, AppControl appControl) =>
        {
            if (window.SupportsSetWindowPosition)
            {
                Console.WriteLine("Active window dragging path: programmatic positioning");
                Console.WriteLine("Hold the middle mouse button to drag the window.");

                bool wasMiddleButtonPressed = false;
                Vector2 initialCursorPosition = default;
                Vector2 initialWindowPosition = default;

                updateSystem.Add(() =>
                {
                    MouseState mouseState = mouseService.GetGlobalState();
                    bool middleButtonPressed = mouseState.IsPressed(MouseButton.Middle);

                    if (middleButtonPressed && !wasMiddleButtonPressed)
                    {
                        initialCursorPosition = mouseState.Position;
                        initialWindowPosition = window.Position;
                    }

                    if (middleButtonPressed)
                    {
                        Vector2 targetPosition = initialWindowPosition
                            + mouseState.Position
                            - initialCursorPosition;

                        window.Position = new Vector2Int(
                            (int)MathF.Round(targetPosition.X),
                            (int)MathF.Round(targetPosition.Y));
                    }

                    wasMiddleButtonPressed = middleButtonPressed;
                });
            }
            else
            {
                Console.WriteLine("Active window dragging path: native window-manager dragging");
                Console.WriteLine("Hold Ctrl and drag with the left mouse button.");

                keyboardService.KeyDown += (Keyboard keyboard, KeyEventArgs eventArgs) =>
                {
                    if (eventArgs.Scancode == Scancode.LeftCtrl || eventArgs.Scancode == Scancode.RightCtrl)
                    {
                        window.Draggable = keyboard.Ctrl;
                    }
                };

                keyboardService.KeyUp += (Keyboard keyboard, KeyEventArgs eventArgs) =>
                {
                    if (eventArgs.Scancode == Scancode.LeftCtrl || eventArgs.Scancode == Scancode.RightCtrl)
                    {
                        window.Draggable = keyboard.Ctrl;
                    }
                };
            }

            Console.WriteLine("Right mouse button or Escape: quit");

            mouseService.ButtonPress += (Mouse mouse, MouseButtonEventArgs eventArgs) =>
            {
                if (eventArgs.Button == MouseButton.Right)
                {
                    appControl.Quit();
                }
            };

            keyboardService.KeyDown += (Keyboard keyboard, KeyEventArgs eventArgs) =>
            {
                if (eventArgs.Key == VirtualKey.Escape)
                {
                    appControl.Quit();
                }
            };
        });

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}

internal sealed class ClearRenderPhase : IRenderPhase<DefaultRenderContext>
{
    private readonly FColor _color;

    public ClearRenderPhase(FColor color)
    {
        _color = color;
    }

    public void Render(DefaultRenderContext renderContext)
    {
        using IRenderPass renderPass = new RenderPassBuilder(renderContext.CommandBuffer)
            .AddColorTarget(renderContext.SwapchainTexture)
            .SetSharedColorTargetSettings(new ColorTargetSettings
            {
                ClearColorValue = _color,
                LoadOperation = LoadOperation.Clear
            })
            .Build();
    }
}
