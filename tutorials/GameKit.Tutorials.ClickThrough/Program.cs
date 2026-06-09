using GameKit;
using GameKit.App;
using GameKit.Common;
using GameKit.Gpu;
using GameKit.Input;
using GameKit.RenderOrchestration;
using GameKit.Tutorials.ClickThrough;

static class Program
{
    // Matches the NDC quad rendered by ClickThroughRenderer in a 400x400 window.
    // Points outside this region return HitTestResult.Miss — clicks pass through to whatever is behind the window.
    static readonly Rectangle InteractiveRegion = new Rectangle(50, 50, 300, 300);

    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRenderManager();

        builder.AddSingleton(new AppConfig
        {
            Size = (400, 400),
            Title = "Click Through",
            Borderless = true,
            ClearColor = FColors.Black
        });
        builder.AddSingleton<IRenderPhase<DefaultRenderContext>>(ClickThroughRenderer.Create);

        builder.OnStart((Window window, IKeyboardService keyboardService, AppControl appControl) =>
        {
            window.SetHitTest(point => InteractiveRegion.Intersects(point) ? HitTestResult.Normal : HitTestResult.Miss);

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
