using GameKit.App;
using GameKit.DependencyInjection;
using GameKit.Logging;
using Microsoft.Extensions.Logging;
using ZLogger;

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
                logging.AddZLoggerFileWithRetention(
                    directoryPath,
                    "game",
                    static options =>
                    {
                        options.InternalErrorLogger = static _ => { };
                    });
            });

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IGameKitApp app = new GameKitApp(serviceProvider);
            ILogger logger = app.GetRequiredService<ILogger>();
            logger.ZLogInformation($"last message");

            app.Dispose();

            string logPath = Directory.GetFiles(directoryPath, "game_*.log").Single();
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
