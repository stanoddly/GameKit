using GameKit.App;
using GameKit.DependencyInjection;
using GameKit.Logging;
using Microsoft.Extensions.Logging;
using ZLogger;
using ZLogger.Providers;

namespace GameKit.Tests;

public class GameKitAppLoggingTests
{
    [Test]
    public void Dispose_DrainsAndClosesOwnedLoggerFactory()
    {
        string directoryPath = Path.Combine(Path.GetTempPath(), $"GameKitAppLoggingTests-{Guid.NewGuid():N}");

        try
        {
            ServiceCollection services = new();
            services.AddZLogger(logging =>
            {
                logging.AddZLoggerRollingFileWithRetention(
                    directoryPath,
                    "game",
                    2,
                    static options =>
                    {
                        options.RollingInterval = RollingInterval.Infinite;
                        options.RollingSizeKB = 1024;
                        options.InternalErrorLogger = static _ => { };
                    });
            });
            services.AddLogger<GameKitAppLoggingTests>();

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IGameKitApp app = new GameKitApp(serviceProvider);
            ILogger<GameKitAppLoggingTests> logger = app.GetRequiredService<ILogger<GameKitAppLoggingTests>>();
            logger.ZLogInformation($"last message");

            app.Dispose();

            string logPath = Directory.GetFiles(directoryPath, "game-*.log").Single();
            Assert.That(File.ReadAllText(logPath), Does.Contain("last message"));

            using FileStream exclusiveStream = new(logPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        finally
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }
    }
}
