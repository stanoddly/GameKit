using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZLogger;
using ZLogger.Providers;

namespace Pixely.Logging;

public static class RetainedFileLoggingExtensions
{
    private const int RetainedFileCount = 10;

    public static ILoggingBuilder AddZLoggerFileWithRetention(
        this ILoggingBuilder builder,
        string fileNamePrefix,
        Action<ZLoggerFileOptions> configure)
    {
        return AddZLoggerFileWithRetention(
            builder,
            GetDefaultDirectoryPaths(),
            fileNamePrefix,
            configure,
            true);
    }

    public static ILoggingBuilder AddZLoggerFileWithRetention(
        this ILoggingBuilder builder,
        string directoryPath,
        string fileNamePrefix,
        Action<ZLoggerFileOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        return AddZLoggerFileWithRetention(
            builder,
            [Path.GetFullPath(directoryPath)],
            fileNamePrefix,
            configure,
            false);
    }

    internal static ILoggingBuilder AddZLoggerFileWithRetention(
        ILoggingBuilder builder,
        IReadOnlyList<string> directoryPaths,
        string fileNamePrefix,
        Action<ZLoggerFileOptions> configure,
        bool allowFallback)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileNamePrefix);
        ArgumentNullException.ThrowIfNull(configure);

        if (fileNamePrefix.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            fileNamePrefix.Contains('*') ||
            fileNamePrefix.Contains('?'))
        {
            throw new ArgumentException("The file name prefix contains invalid or wildcard characters.", nameof(fileNamePrefix));
        }

        builder.Services.AddSingleton<ILoggerProvider>(_ =>
        {
            ZLoggerFileOptions options = new();
            configure(options);

            if (options.InternalErrorLogger == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ZLoggerOptions.InternalErrorLogger)} must be configured with a non-recursive fallback.");
            }

            options.FullMode = BackgroundBufferFullMode.Grow;
            options.FileShared = false;

            DateTimeOffset timestamp = (options.TimeProvider?.GetUtcNow() ?? DateTimeOffset.UtcNow).ToUniversalTime();
            string fileName = string.Create(
                CultureInfo.InvariantCulture,
                $"{fileNamePrefix}_{timestamp:yyyyMMdd_HHmmss}Z_pid{Environment.ProcessId}.log");
            Exception? lastException = null;

            foreach (string directoryPath in directoryPaths)
            {
                try
                {
                    return CreateProvider(directoryPath, fileNamePrefix, fileName, options);
                }
                catch (Exception exception) when (allowFallback)
                {
                    ReportError(options.InternalErrorLogger, exception);
                    lastException = exception;
                }
                catch (Exception exception)
                {
                    ReportError(options.InternalErrorLogger, exception);
                    throw;
                }
            }

            throw new InvalidOperationException("No writable log directory is available.", lastException);
        });

        return builder;
    }

    private static ZLoggerFileLoggerProvider CreateProvider(
        string directoryPath,
        string fileNamePrefix,
        string fileName,
        ZLoggerFileOptions options)
    {
        LogFileRetention retention = new(
            directoryPath,
            $"{fileNamePrefix}_*.log",
            RetainedFileCount - 1,
            options.InternalErrorLogger!);
        retention.Apply();

        string filePath = Path.Combine(directoryPath, fileName);
        return new ZLoggerFileLoggerProvider(filePath, options);
    }

    private static IReadOnlyList<string> GetDefaultDirectoryPaths()
    {
        List<string> directoryPaths = [Path.GetFullPath(AppContext.BaseDirectory)];
        string localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (!string.IsNullOrWhiteSpace(localApplicationData))
        {
            string fallbackPath = Path.GetFullPath(Path.Combine(localApplicationData, "Pixely", "Logs"));
            StringComparison comparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            if (!string.Equals(directoryPaths[0], fallbackPath, comparison))
            {
                directoryPaths.Add(fallbackPath);
            }
        }

        return directoryPaths;
    }

    private static void ReportError(Action<Exception> internalErrorLogger, Exception exception)
    {
        try
        {
            internalErrorLogger(exception);
        }
        catch
        {
        }
    }
}

internal sealed class LogFileRetention
{
    private readonly string _directoryPath;
    private readonly string _searchPattern;
    private readonly int _retainedFileCount;
    private readonly Action<Exception> _internalErrorLogger;

    public LogFileRetention(
        string directoryPath,
        string searchPattern,
        int retainedFileCount,
        Action<Exception> internalErrorLogger)
    {
        _directoryPath = directoryPath;
        _searchPattern = searchPattern;
        _retainedFileCount = retainedFileCount;
        _internalErrorLogger = internalErrorLogger;
    }

    public void Apply()
    {
        Directory.CreateDirectory(_directoryPath);

        FileInfo[] files = new DirectoryInfo(_directoryPath)
            .EnumerateFiles(_searchPattern, SearchOption.TopDirectoryOnly)
            .OrderByDescending(static file => file.Name, StringComparer.Ordinal)
            .ToArray();

        for (int i = _retainedFileCount; i < files.Length; i++)
        {
            try
            {
                files[i].Delete();
            }
            catch (Exception exception)
            {
                ReportError(exception);
            }
        }
    }

    public void ReportError(Exception exception)
    {
        try
        {
            _internalErrorLogger(exception);
        }
        catch
        {
        }
    }
}
