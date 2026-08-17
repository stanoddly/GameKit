using GameKit.App;
using GameKit.Input;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.MouseWindowPresence;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .UseDefaultRendering(
                new WindowConfig(Size: (640, 480), Title: "Mouse Window Presence"));

        builder.OnStart((IMouseService mouseService) =>
        {
            Console.WriteLine($"Mouse starts in window: {mouseService.IsInWindow()}");
            Console.WriteLine("Move the mouse into and out of the window to see enter and leave events.");

            mouseService.WindowEnter += eventArgs =>
            {
                Console.WriteLine($"Mouse entered window at {eventArgs.Timestamp}. IsInWindow: {mouseService.IsInWindow()}");
            };

            mouseService.WindowLeave += eventArgs =>
            {
                Console.WriteLine($"Mouse left window at {eventArgs.Timestamp}. IsInWindow: {mouseService.IsInWindow()}");
            };
        });

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
