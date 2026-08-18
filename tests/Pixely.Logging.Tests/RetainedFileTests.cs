using Pixely.DependencyInjection;
using Pixely.Logging;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Pixely.Logging.Tests;

public class RetainedFileTests
{
    [Test]
    public void Provider_WithoutDirectoryUsesExecutableDirectory()
    {
        string fileNamePrefix = $"pixely-test-{Guid.NewGuid():N}";
        FixedTimeProvider timeProvider = new(new DateTimeOffset(2026, 8, 7, 9, 4, 16, TimeSpan.Zero));
        string expectedPath = Path.Combine(
            AppContext.BaseDirectory,
            $"{fileNamePrefix}_20260807_090416Z_pid{Environment.ProcessId}.log");

        try
        {
            ServiceCollection services = new();
            services.AddZLogger(logging => logging.AddZLoggerFileWithRetention(
                fileNamePrefix,
                options =>
                {
                    options.TimeProvider = timeProvider;
                    options.InternalErrorLogger = static _ => { };
                }));
            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                ILogger logger = serviceProvider.GetRequiredService<ILogger>();
                logger.ZLogInformation($"default directory");
            }

            Assert.That(File.ReadAllText(expectedPath), Does.Contain("default directory"));
        }
        finally
        {
            File.Delete(expectedPath);
        }
    }

    [Test]
    public void Provider_WhenPreferredDirectoryFails_ReportsAndUsesFallback()
    {
        using TemporaryDirectory temporaryDirectory = new();
        string blockedPath = Path.Combine(temporaryDirectory.Path, "not-a-directory");
        string fallbackPath = Path.Combine(temporaryDirectory.Path, "fallback");
        File.WriteAllText(blockedPath, "occupied");
        List<Exception> errors = new();
        ServiceCollection services = new();
        services.AddZLogger(logging => RetainedFileLoggingExtensions.AddZLoggerFileWithRetention(
            logging,
            [blockedPath, fallbackPath],
            "game",
            options => options.InternalErrorLogger = errors.Add,
            true));
        using (ServiceProvider serviceProvider = services.BuildServiceProvider())
        {
            ILogger logger = serviceProvider.GetRequiredService<ILogger>();
            logger.ZLogInformation($"fallback directory");
        }

        string logPath = Directory.GetFiles(fallbackPath, "game_*.log").Single();
        Assert.Multiple(() =>
        {
            Assert.That(File.ReadAllText(logPath), Does.Contain("fallback directory"));
            Assert.That(errors, Is.Not.Empty);
        });
    }

    [Test]
    public void Provider_WritesTimestampedFileAndPreservesOrder()
    {
        using TemporaryDirectory temporaryDirectory = new();
        List<Exception> errors = new();
        FixedTimeProvider timeProvider = new(new DateTimeOffset(2026, 8, 7, 9, 4, 16, TimeSpan.Zero));

        using (ServiceProvider serviceProvider = CreateProvider(temporaryDirectory.Path, errors.Add, timeProvider))
        {
            ILogger logger = serviceProvider.GetRequiredService<ILogger>();
            logger.ZLogInformation($"first");
            logger.ZLogInformation($"second");
        }

        string expectedFileName = $"game_20260807_090416Z_pid{Environment.ProcessId}.log";
        string logPath = Path.Combine(temporaryDirectory.Path, expectedFileName);
        string contents = File.ReadAllText(logPath);

        Assert.Multiple(() =>
        {
            Assert.That(contents.IndexOf("first", StringComparison.Ordinal), Is.LessThan(contents.IndexOf("second", StringComparison.Ordinal)));
            Assert.That(errors, Is.Empty);
        });
    }

    [Test]
    public void Provider_DifferentTimestampCreatesDifferentFile()
    {
        using TemporaryDirectory temporaryDirectory = new();
        List<Exception> errors = new();

        WriteMessage(
            temporaryDirectory.Path,
            errors.Add,
            new FixedTimeProvider(new DateTimeOffset(2026, 8, 7, 9, 4, 16, TimeSpan.Zero)),
            "first run");
        WriteMessage(
            temporaryDirectory.Path,
            errors.Add,
            new FixedTimeProvider(new DateTimeOffset(2026, 8, 7, 9, 4, 17, TimeSpan.Zero)),
            "second run");

        Assert.Multiple(() =>
        {
            Assert.That(Directory.GetFiles(temporaryDirectory.Path, "game_*.log"), Has.Length.EqualTo(2));
            Assert.That(errors, Is.Empty);
        });
    }

    [Test]
    public void Provider_DisposalDrainsAndClosesFile()
    {
        using TemporaryDirectory temporaryDirectory = new();
        ServiceProvider serviceProvider = CreateProvider(temporaryDirectory.Path, static _ => { });
        ILogger logger = serviceProvider.GetRequiredService<ILogger>();

        logger.ZLogInformation($"final queued entry");
        serviceProvider.Dispose();

        string logPath = Directory.GetFiles(temporaryDirectory.Path, "game_*.log").Single();
        string contents = File.ReadAllText(logPath);

        using FileStream exclusiveStream = new(logPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Assert.That(contents, Does.Contain("final queued entry"));
    }

    [Test]
    public void Provider_KeepsLatestTenMatchingFilesOnly()
    {
        using TemporaryDirectory temporaryDirectory = new();
        string unrelatedPath = Path.Combine(temporaryDirectory.Path, "unrelated.log");
        File.WriteAllText(unrelatedPath, "keep");

        for (int i = 0; i < 12; i++)
        {
            string oldLogPath = Path.Combine(
                temporaryDirectory.Path,
                $"game_20260807_0904{i:D2}Z_pid{i}.log");
            File.WriteAllText(oldLogPath, i.ToString());
        }

        WriteMessage(
            temporaryDirectory.Path,
            static _ => { },
            new FixedTimeProvider(new DateTimeOffset(2026, 8, 7, 10, 0, 0, TimeSpan.Zero)),
            "current");

        Assert.Multiple(() =>
        {
            Assert.That(Directory.GetFiles(temporaryDirectory.Path, "game_*.log"), Has.Length.EqualTo(10));
            Assert.That(File.Exists(unrelatedPath), Is.True);
        });
    }

    [Test]
    public void AddZLoggerFileWithRetention_RequiresInternalErrorFallback()
    {
        using TemporaryDirectory temporaryDirectory = new();
        ServiceCollection services = new();
        services.AddZLogger(logging => logging.AddZLoggerFileWithRetention(
            temporaryDirectory.Path,
            "game",
            static _ => { }));

        InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(
            () => services.BuildServiceProvider());

        Assert.That(exception!.Message, Does.Contain(nameof(ZLoggerOptions.InternalErrorLogger)));
    }

    [Test]
    public void Provider_WhenFormattingFails_UsesInternalErrorFallback()
    {
        using TemporaryDirectory temporaryDirectory = new();
        List<Exception> errors = new();
        using ServiceProvider serviceProvider = CreateProvider(temporaryDirectory.Path, errors.Add);
        ILogger logger = serviceProvider.GetRequiredService<ILogger>();

        logger.ZLogInformation($"{new ThrowingValue()}");
        serviceProvider.Dispose();

        Assert.That(errors, Has.Some.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void Provider_WhenFileCannotBeOpened_UsesInternalErrorFallback()
    {
        using TemporaryDirectory temporaryDirectory = new();
        string filePath = Path.Combine(temporaryDirectory.Path, "not-a-directory");
        File.WriteAllText(filePath, "occupied");
        List<Exception> errors = new();

        Assert.Throws<IOException>(() => CreateProvider(filePath, errors.Add));

        Assert.That(errors, Is.Not.Empty);
    }

    private static void WriteMessage(
        string directoryPath,
        Action<Exception> internalErrorLogger,
        TimeProvider timeProvider,
        string message)
    {
        using ServiceProvider serviceProvider = CreateProvider(directoryPath, internalErrorLogger, timeProvider);
        ILogger logger = serviceProvider.GetRequiredService<ILogger>();
        logger.ZLogInformation($"{message}");
    }

    private static ServiceProvider CreateProvider(
        string directoryPath,
        Action<Exception> internalErrorLogger,
        TimeProvider? timeProvider = null)
    {
        ServiceCollection services = new();
        services.AddZLogger(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Trace);
            logging.AddZLoggerFileWithRetention(
                directoryPath,
                "game",
                options =>
                {
                    options.InternalErrorLogger = internalErrorLogger;
                    options.TimeProvider = timeProvider;
                });
        });
        return services.BuildServiceProvider();
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
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
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"Pixely.Logging.Tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public void Dispose()
        {
            Directory.Delete(Path, true);
        }
    }
}
