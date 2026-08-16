using GameKit.App;
using GameKit.Input;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.MouseWindowPresence;

static class Program
{
    internal static readonly ViewScope ViewScope = new(0);

    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .UseWindowRendering(
                ViewScope,
                new WindowConfig(Size: (640, 480), Title: "Mouse Window Presence"));

        builder.AddSingleton<IViewRenderer>(new NullViewRenderer(ViewScope));

        builder.OnStart((IMouseService mouseService) =>
        {
            Console.WriteLine($"Mouse starts in window: {mouseService.IsInWindow(ViewScope)}");
            Console.WriteLine("Move the mouse into and out of the window to see enter and leave events.");

            mouseService.WindowEnter += eventArgs =>
            {
                Console.WriteLine($"Mouse entered window at {eventArgs.Timestamp}. IsInWindow: {mouseService.IsInWindow(ViewScope)}");
            };

            mouseService.WindowLeave += eventArgs =>
            {
                Console.WriteLine($"Mouse left window at {eventArgs.Timestamp}. IsInWindow: {mouseService.IsInWindow(ViewScope)}");
            };
        });

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
