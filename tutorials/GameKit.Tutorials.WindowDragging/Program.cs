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
            .UseDefaultRenderManager();

        builder.AddSingleton(new AppConfig
        {
            Size = (400, 400),
            Title = "Window Dragging",
            Borderless = true
        });

        builder.AddSingleton<IRenderPhase<DefaultRenderContext>>(static () => new ClearRenderPhase(FColors.SkyBlue));

        builder.OnStart((IWindow window, IMouseService mouseService, IKeyboardService keyboardService, AppControl appControl) =>
        {
            window.Draggable = true;

            mouseService.ButtonPress += (Mouse mouse, MouseButtonEventArgs e) =>
            {
                if (e.Button == MouseButton.Right)
                {
                    appControl.Quit();
                }
            };

            keyboardService.KeyDown += (Keyboard keyboard, KeyEventArgs e) =>
            {
                if (e.Key == VirtualKey.Escape)
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
