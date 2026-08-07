using GameKit.DependencyInjection;
using GameKit.Logging;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using ZLogger;
using ZLogger.Providers;

namespace GameKit.Logging.Tests;

public class RetainedRollingFileTests
{
    [Test]
    public void Provider_PreservesOrderAndAppendsAcrossRuns()
    {
        using TemporaryDirectory temporaryDirectory = new();
        List<Exception> errors = new();

        WriteMessages(temporaryDirectory.Path, errors, "first", "second");
        WriteMessages(temporaryDirectory.Path, errors, "third");

        string logPath = Directory.GetFiles(temporaryDirectory.Path, "game-*.log").Single();
        string contents = File.ReadAllText(logPath);

        Assert.Multiple(() =>
        {
            Assert.That(contents.IndexOf("first", StringComparison.Ordinal), Is.LessThan(contents.IndexOf("second", StringComparison.Ordinal)));
            Assert.That(contents.IndexOf("second", StringComparison.Ordinal), Is.LessThan(contents.IndexOf("third", StringComparison.Ordinal)));
            Assert.That(errors, Is.Empty);
        });
    }

    [Test]
    public void Provider_DisposalDrainsAndClosesFile()
    {
        using TemporaryDirectory temporaryDirectory = new();
        ServiceProvider serviceProvider = CreateProvider(temporaryDirectory.Path, 5, static _ => { });
        ILogger<RetainedRollingFileTests> logger = serviceProvider.GetRequiredService<ILogger<RetainedRollingFileTests>>();

        logger.ZLogInformation($"final queued entry");
        serviceProvider.Dispose();

        string logPath = Directory.GetFiles(temporaryDirectory.Path, "game-*.log").Single();
        string contents = File.ReadAllText(logPath);

        using FileStream exclusiveStream = new(logPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Assert.That(contents, Does.Contain("final queued entry"));
    }

    [Test]
    public void Provider_DisposalAppliesRetentionToMatchingFilesOnly()
    {
        using TemporaryDirectory temporaryDirectory = new();
        string unrelatedPath = System.IO.Path.Combine(temporaryDirectory.Path, "unrelated.log");
        File.WriteAllText(unrelatedPath, "keep");

        for (int i = 0; i < 4; i++)
        {
            string oldLogPath = System.IO.Path.Combine(temporaryDirectory.Path, $"game-old-{i:D4}.log");
            File.WriteAllText(oldLogPath, i.ToString());
            File.SetLastWriteTimeUtc(oldLogPath, DateTime.UtcNow.AddDays(-10 + i));
        }

        using (ServiceProvider serviceProvider = CreateProvider(temporaryDirectory.Path, 2, static _ => { }))
        {
            ILogger<RetainedRollingFileTests> logger = serviceProvider.GetRequiredService<ILogger<RetainedRollingFileTests>>();
            logger.ZLogInformation($"current");
        }

        Assert.Multiple(() =>
        {
            Assert.That(Directory.GetFiles(temporaryDirectory.Path, "game-*.log"), Has.Length.EqualTo(2));
            Assert.That(File.Exists(unrelatedPath), Is.True);
        });
    }

    [Test]
    public void AddZLoggerRollingFileWithRetention_RequiresInternalErrorFallback()
    {
        using TemporaryDirectory temporaryDirectory = new();
        ServiceCollection services = new();
        services.AddZLogger(logging => logging.AddZLoggerRollingFileWithRetention(
            temporaryDirectory.Path,
            "game",
            2,
            static options =>
            {
                options.RollingInterval = RollingInterval.Infinite;
                options.RollingSizeKB = 1024;
            }));

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(
            () => services.BuildServiceProvider());

        Assert.That(exception!.Message, Does.Contain(nameof(ZLoggerOptions.InternalErrorLogger)));
    }

    [Test]
    public void Provider_RollsByInterval()
    {
        using TemporaryDirectory temporaryDirectory = new();
        ManualTimeProvider timeProvider = new(new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));
        using ServiceProvider serviceProvider = CreateProvider(
            temporaryDirectory.Path,
            5,
            static _ => { },
            options =>
            {
                options.RollingInterval = RollingInterval.Day;
                options.TimeProvider = timeProvider;
            });
        ILogger<RetainedRollingFileTests> logger = serviceProvider.GetRequiredService<ILogger<RetainedRollingFileTests>>();

        logger.ZLogInformation($"first day");
        WaitUntil(() => Directory.GetFiles(temporaryDirectory.Path, "game-*.log")
            .Any(static path => new FileInfo(path).Length > 0));

        timeProvider.Advance(TimeSpan.FromDays(1));
        logger.ZLogInformation($"second day");
        serviceProvider.Dispose();

        Assert.That(Directory.GetFiles(temporaryDirectory.Path, "game-*.log"), Has.Length.EqualTo(2));
    }

    [Test]
    public void Provider_RollsBySize()
    {
        using TemporaryDirectory temporaryDirectory = new();
        using ServiceProvider serviceProvider = CreateProvider(
            temporaryDirectory.Path,
            5,
            static _ => { },
            static options => options.RollingSizeKB = 1);
        ILogger<RetainedRollingFileTests> logger = serviceProvider.GetRequiredService<ILogger<RetainedRollingFileTests>>();
        string largeValue = new('x', 2 * 1024);

        logger.ZLogInformation($"{largeValue}");
        WaitUntil(() => Directory.GetFiles(temporaryDirectory.Path, "game-*.log")
            .Any(static path => new FileInfo(path).Length > 1024));

        logger.ZLogInformation($"after roll");
        serviceProvider.Dispose();

        Assert.That(Directory.GetFiles(temporaryDirectory.Path, "game-*.log"), Has.Length.EqualTo(2));
    }

    [Test]
    public void Provider_WhenFormattingFails_UsesInternalErrorFallback()
    {
        using TemporaryDirectory temporaryDirectory = new();
        List<Exception> errors = new();
        using ServiceProvider serviceProvider = CreateProvider(temporaryDirectory.Path, 5, errors.Add);
        ILogger<RetainedRollingFileTests> logger = serviceProvider.GetRequiredService<ILogger<RetainedRollingFileTests>>();

        logger.ZLogInformation($"{new ThrowingValue()}");
        serviceProvider.Dispose();

        Assert.That(errors, Has.Some.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void Provider_WhenFileCannotBeOpened_UsesInternalErrorFallback()
    {
        using TemporaryDirectory temporaryDirectory = new();
        string filePath = System.IO.Path.Combine(temporaryDirectory.Path, "not-a-directory");
        File.WriteAllText(filePath, "occupied");
        List<Exception> errors = new();

        Assert.Throws<InvalidOperationException>(() => CreateProvider(filePath, 5, errors.Add));

        Assert.That(errors, Is.Not.Empty);
    }

    private static void WriteMessages(string directoryPath, List<Exception> errors, params string[] messages)
    {
        using ServiceProvider serviceProvider = CreateProvider(directoryPath, 5, errors.Add);
        ILogger<RetainedRollingFileTests> logger = serviceProvider.GetRequiredService<ILogger<RetainedRollingFileTests>>();

        foreach (string message in messages)
        {
            logger.ZLogInformation($"{message}");
        }
    }

    private static ServiceProvider CreateProvider(
        string directoryPath,
        int retainedFileCount,
        Action<Exception> internalErrorLogger,
        Action<ZLoggerRollingFileOptions>? configure = null)
    {
        ServiceCollection services = new();
        services.AddZLogger(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Trace);
            logging.AddZLoggerRollingFileWithRetention(
                directoryPath,
                "game",
                retainedFileCount,
                options =>
                {
                    options.RollingInterval = RollingInterval.Infinite;
                    options.RollingSizeKB = 1024;
                    options.InternalErrorLogger = internalErrorLogger;
                    configure?.Invoke(options);
                });
        });
        services.AddLogger<RetainedRollingFileTests>();
        return services.BuildServiceProvider();
    }

    private static void WaitUntil(Func<bool> condition)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!condition())
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(2))
            {
                Assert.Fail("The logging operation did not complete within the timeout.");
            }

            Thread.Yield();
        }
    }

    private sealed class ManualTimeProvider : TimeProvider
    {
        private DateTimeOffset _utcNow;

        public ManualTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }

        public void Advance(TimeSpan duration)
        {
            _utcNow += duration;
        }
    }

    private sealed class ThrowingValue
    {
        public override string ToString()
        {
            throw new InvalidOperationException("Formatting failed.");
        }
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public string Path { get; }

        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"GameKit.Logging.Tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public void Dispose()
        {
            Directory.Delete(Path, true);
        }
    }
}
