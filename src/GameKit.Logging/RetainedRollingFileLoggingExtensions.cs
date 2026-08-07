using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZLogger;
using ZLogger.Providers;

namespace GameKit.Logging;

public static class RetainedRollingFileLoggingExtensions
{
    public static ILoggingBuilder AddZLoggerRollingFileWithRetention(
        this ILoggingBuilder builder,
        string directoryPath,
        string fileNamePrefix,
        int retainedFileCount,
        Action<ZLoggerRollingFileOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileNamePrefix);
        ArgumentNullException.ThrowIfNull(configure);

        if (retainedFileCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(retainedFileCount), "At least one log file must be retained.");
        }

        if (fileNamePrefix.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            fileNamePrefix.Contains('*') ||
            fileNamePrefix.Contains('?'))
        {
            throw new ArgumentException("The file name prefix contains invalid or wildcard characters.", nameof(fileNamePrefix));
        }

        string fullDirectoryPath = Path.GetFullPath(directoryPath);

        builder.Services.AddSingleton<ILoggerProvider>(_ =>
        {
            ZLoggerRollingFileOptions options = new();
            configure(options);

            if (options.RollingSizeKB < 1)
            {
                throw new InvalidOperationException($"{nameof(ZLoggerRollingFileOptions.RollingSizeKB)} must be positive.");
            }

            if (options.InternalErrorLogger == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ZLoggerOptions.InternalErrorLogger)} must be configured with a non-recursive fallback.");
            }

            options.FilePathSelector = (timestamp, sequence) => Path.Combine(
                fullDirectoryPath,
                $"{fileNamePrefix}-{FormatTimestamp(timestamp, options.RollingInterval)}-{sequence:D4}.log");
            options.FullMode = BackgroundBufferFullMode.Grow;
            options.FileShared = false;

            LogFileRetention retention = new(
                fullDirectoryPath,
                $"{fileNamePrefix}-*.log",
                retainedFileCount,
                options.InternalErrorLogger);

            return new RetainedRollingFileLoggerProvider(options, retention);
        });

        return builder;
    }

    private static string FormatTimestamp(DateTimeOffset timestamp, RollingInterval rollingInterval)
    {
        return rollingInterval switch
        {
            RollingInterval.Infinite => "all",
            RollingInterval.Year => timestamp.ToString("yyyy", CultureInfo.InvariantCulture),
            RollingInterval.Month => timestamp.ToString("yyyyMM", CultureInfo.InvariantCulture),
            RollingInterval.Day => timestamp.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
            RollingInterval.Hour => timestamp.ToString("yyyyMMdd-HH", CultureInfo.InvariantCulture),
            RollingInterval.Minute => timestamp.ToString("yyyyMMdd-HHmm", CultureInfo.InvariantCulture),
            _ => throw new ArgumentOutOfRangeException(nameof(rollingInterval))
        };
    }
}

internal sealed class RetainedRollingFileLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly ZLoggerRollingFileLoggerProvider _provider;
    private readonly LogFileRetention _retention;
    private bool _disposed;

    public RetainedRollingFileLoggerProvider(ZLoggerRollingFileOptions options, LogFileRetention retention)
    {
        _retention = retention;
        _retention.Apply();

        try
        {
            _provider = new ZLoggerRollingFileLoggerProvider(options);
        }
        catch (Exception exception)
        {
            _retention.ReportError(exception);
            throw;
        }
    }

    public ILogger CreateLogger(string categoryName)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _provider.CreateLogger(categoryName);
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _provider.SetScopeProvider(scopeProvider);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        try
        {
            _provider.Dispose();
        }
        finally
        {
            _retention.Apply();
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
        try
        {
            Directory.CreateDirectory(_directoryPath);

            FileInfo[] files = new DirectoryInfo(_directoryPath)
                .EnumerateFiles(_searchPattern, SearchOption.TopDirectoryOnly)
                .OrderByDescending(static file => file.LastWriteTimeUtc)
                .ThenByDescending(static file => file.Name, StringComparer.Ordinal)
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
