using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZLogger;
using ZLogger.Providers;

namespace GameKit.Logging;

public static class RetainedFileLoggingExtensions
{
    private const int RetainedFileCount = 10;

    public static ILoggingBuilder AddZLoggerFileWithRetention(
        this ILoggingBuilder builder,
        string directoryPath,
        string fileNamePrefix,
        Action<ZLoggerFileOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileNamePrefix);
        ArgumentNullException.ThrowIfNull(configure);

        if (fileNamePrefix.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            fileNamePrefix.Contains('*') ||
            fileNamePrefix.Contains('?'))
        {
            throw new ArgumentException("The file name prefix contains invalid or wildcard characters.", nameof(fileNamePrefix));
        }

        string fullDirectoryPath = Path.GetFullPath(directoryPath);

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
            string filePath = Path.Combine(fullDirectoryPath, fileName);
            LogFileRetention retention = new(
                fullDirectoryPath,
                $"{fileNamePrefix}_*.log",
                RetainedFileCount - 1,
                options.InternalErrorLogger);
            retention.Apply();

            try
            {
                return new ZLoggerFileLoggerProvider(filePath, options);
            }
            catch (Exception exception)
            {
                retention.ReportError(exception);
                throw;
            }
        });

        return builder;
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
        try
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
        catch (Exception exception)
        {
            ReportError(exception);
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
