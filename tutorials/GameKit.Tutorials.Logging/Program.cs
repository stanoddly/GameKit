using GameKit.App;
using GameKit.Logging;
using GameKit.RenderOrchestration;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace GameKit.Tutorials.Logging;

sealed class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .UseDefaultRenderManager();

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
        builder.AddLogger<Program>();
        builder.AddSingleton(new AppConfig { Size = (1280, 720), Title = "Logging" });
        builder.AddSingleton<IRenderPhase<DefaultRenderContext>, NullRenderPhase<DefaultRenderContext>>();

        using IGameKitApp gameKitApp = builder.Build();
        ILogger<Program> logger = gameKitApp.GetRequiredService<ILogger<Program>>();

        logger.ZLogInformation($"Application started in process {Environment.ProcessId}");
        logger.ZLogConditionalDebug($"Debug diagnostics are enabled");

        try
        {
            return gameKitApp.Run();
        }
        catch (Exception exception)
        {
            logger.ZLogError(exception, $"Application terminated unexpectedly");
            return 1;
        }
    }
}
