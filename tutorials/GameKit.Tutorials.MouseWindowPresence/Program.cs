using GameKit.App;
using GameKit.Input;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.MouseWindowPresence;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .UseDefaultRendering(new WindowConfig { Size = (640, 480), Title = "Mouse Window Presence" });

        builder.AddSingleton<IRenderer<DefaultRenderContext>, NullRenderer<DefaultRenderContext>>();

        builder.OnStart((Window<DefaultRenderContext> window, IMouseService mouseService) =>
        {
            Console.WriteLine($"Mouse starts in window: {mouseService.IsInWindow(window)}");
            Console.WriteLine("Move the mouse into and out of the window to see enter and leave events.");

            mouseService.WindowEnter += eventArgs =>
            {
                Console.WriteLine($"Mouse entered window at {eventArgs.Timestamp}. IsInWindow: {mouseService.IsInWindow(window)}");
            };

            mouseService.WindowLeave += eventArgs =>
            {
                Console.WriteLine($"Mouse left window at {eventArgs.Timestamp}. IsInWindow: {mouseService.IsInWindow(window)}");
            };
        });

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
