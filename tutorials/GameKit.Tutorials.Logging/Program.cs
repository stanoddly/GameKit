using GameKit.App;
using GameKit.Logging;
using GameKit.RenderOrchestration;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace GameKit.Tutorials.Logging;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .UseDefaultRenderCoordinator();

        builder.AddZLogger(logging =>
        {
#if DEBUG
            logging.SetMinimumLevel(LogLevel.Debug);
#else
            logging.SetMinimumLevel(LogLevel.Information);
#endif
            logging.AddZLoggerFileWithRetention(
                "gamekit",
                static options =>
                {
                    options.InternalErrorLogger = static exception => Console.Error.WriteLine(exception);
                });

#if DEBUG
            logging.AddZLoggerConsole(static options =>
            {
                options.FullMode = BackgroundBufferFullMode.Grow;
                options.InternalErrorLogger = static exception => Console.Error.WriteLine(exception);
            });
#endif
        });
        builder.AddSingleton<PlayerInputService>(PlayerInputService.Create);
        builder.AddWindow(new WindowOptions(Size: (1280, 720), Title: "Logging"));
        builder.AddSingleton<IRenderPhase<DefaultRenderContext>, NullRenderPhase<DefaultRenderContext>>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
