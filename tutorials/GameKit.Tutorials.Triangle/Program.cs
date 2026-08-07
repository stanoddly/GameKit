using GameKit.App;
using GameKit.Logging;
using GameKit.RenderOrchestration;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace GameKit.Tutorials.Triangle;

static class Program
{
    static int Main(string[] args)
    {
        string logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GameKit",
            "Triangle",
            "Logs");

        GameKitAppBuilder builder = new GameKitAppBuilder()
            //.AddContentFromZipPattern("data*.pak")
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRenderManager();

        builder.AddZLogger(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Information);
            logging.AddZLoggerFileWithRetention(
                logDirectory,
                "triangle",
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
        builder.AddLogger<TriangleRenderer>();
        builder.AddSingleton(new AppConfig { Size = (1280, 720), Title = "Game" });
        builder.AddSingleton<IRenderPhase<DefaultRenderContext>>(TriangleRenderer.Create);

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
