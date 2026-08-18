using Pixely.App;
using Pixely.DependencyInjection;
using Pixely.Logging;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Pixely.Tests;

public class PixelyAppLoggingTests
{
    [Test]
    public void Dispose_DrainsAndClosesOwnedLoggerFactory()
    {
        string directoryPath = Path.Combine(Path.GetTempPath(), $"PixelyAppLoggingTests-{Guid.NewGuid():N}");

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
            IPixelyApp app = new PixelyApp(serviceProvider);
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
