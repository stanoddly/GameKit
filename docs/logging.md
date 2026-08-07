# Logging

`GameKit.Logging` integrates ZLogger with GameKit's service collection. The logger factory belongs to the root service provider, remains available across stage transitions, and drains queued entries when the application is disposed.

## Registration

Register the logger factory and category loggers:

```csharp
using GameKit.App;
using GameKit.Logging;
using Microsoft.Extensions.Logging;
using ZLogger;

GameKitAppBuilder builder = new();
builder.AddZLogger(logging =>
{
    logging.SetMinimumLevel(LogLevel.Information);
    logging.AddZLoggerFileWithRetention(
        "game",
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
builder.AddLogger<PlayerSystem>();
```

`AddZLoggerFileWithRetention` creates one file for the process using this naming policy:

```text
{prefix}_20260807_090416Z_pid48545.log
```

The timestamp is UTC and the process ID is labeled explicitly. When no directory is supplied, the helper first attempts `AppContext.BaseDirectory`, then falls back to `LocalApplicationData/GameKit/Logs`. A failed preferred location is reported through `InternalErrorLogger`. Before opening the new file, the helper keeps the latest nine existing matching files, leaving at most 10 after the new file is created. Other prefixes and unrelated files are not changed.

Pass a directory explicitly when the application has a platform-provided location. Explicit paths are strict and do not fall back:

```csharp
logging.AddZLoggerFileWithRetention(
    logDirectory,
    "game",
    static options =>
    {
        options.InternalErrorLogger = static exception => Console.Error.WriteLine(exception);
    });
```

The file provider uses an unbounded asynchronous buffer. Logging does not wait for file I/O, but a sustained output failure can retain queued entries and their captured values until writing recovers or the application shuts down. The integration sets `BackgroundBufferFullMode.Grow` explicitly and does not enable shared-file mode.

The internal error callback must write directly to a separate destination such as standard error or a platform diagnostic API. Do not send it through the failing logger.

## Category loggers

GameKit registers category loggers explicitly because its dependency injection container does not use open-generic registrations:

```csharp
builder.AddLogger<PlayerSystem>();

public sealed class PlayerSystem
{
    private readonly ILogger<PlayerSystem> _logger;

    public PlayerSystem(ILogger<PlayerSystem> logger)
    {
        _logger = logger;
    }
}
```

A stage can call `services.AddLogger<StageSystem>()`. The category logger resolves the root factory; unloading the stage does not dispose the factory.

Use `ILoggerFactory.CreateLogger<T>()` directly when registering every category is unnecessary.

## Logging calls

Use ZLogger interpolated handlers in hot paths so disabled levels do not evaluate interpolation expressions:

```csharp
logger.ZLogInformation($"Loaded level {levelName}");
logger.ZLogDebug($"Entity {entityId} moved to {position}");
```

`ZLogDebug` is controlled by runtime log-level filters and remains available in Release builds. Use `ZLogConditionalDebug` for diagnostics that must be removed from a build when `DEBUG` is not defined:

```csharp
logger.ZLogConditionalDebug($"Entity {entityId} moved to {position}");
logger.ZLogConditionalDebug($"Entity {entityId} failed", exception: exception);
```

Release call sites contain no logging invocation, handler construction, or interpolation-expression evaluation. Debug call sites still respect `ILogger.IsEnabled(LogLevel.Debug)`.

The background writer formats captured values later. Prefer small immutable values or snapshots; do not capture mutable objects whose state may change before formatting.

## Shutdown and durability

Dispose `IGameKitApp`, normally with a `using` declaration. Disposal completes the logging channel, drains queued entries, flushes the stream, and closes the current file.

Logging is best-effort. Returning from a logging call does not mean the entry is on disk, and a process crash can lose queued or operating-system-buffered entries.
