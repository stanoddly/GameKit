# Logging

`GameKit.Logging` integrates ZLogger with GameKit's service collection. The logger factory belongs to the root service provider, remains available across stage transitions, and drains queued entries when the application is disposed.

## Registration

Choose a writable directory supplied by the application or platform, then register the logger factory and category loggers:

```csharp
using GameKit.App;
using GameKit.Logging;
using Microsoft.Extensions.Logging;
using ZLogger;
using ZLogger.Providers;

string logDirectory = GetWritableLogDirectory();

GameKitAppBuilder builder = new();
builder.AddZLogger(logging =>
{
    logging.SetMinimumLevel(LogLevel.Information);
    logging.AddZLoggerRollingFileWithRetention(
        logDirectory,
        "game",
        10,
        static options =>
        {
            options.RollingInterval = RollingInterval.Day;
            options.RollingSizeKB = 10 * 1024;
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

`AddZLoggerRollingFileWithRetention` uses an unbounded asynchronous buffer. Logging does not wait for file I/O, but a sustained output failure can retain queued entries and their captured values until writing recovers or the application shuts down. The integration sets `BackgroundBufferFullMode.Grow` explicitly and does not enable shared-file mode.

The internal error callback must write directly to a separate destination such as standard error or a platform diagnostic API. Do not send it through the failing logger.

The retention helper deletes only files matching its configured prefix. Cleanup runs before the file provider opens and after it closes. The retained-file limit can therefore be exceeded during a long-running session and is restored at the next clean shutdown or application start.

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

Dispose `IGameKitApp`, normally with a `using` declaration. Disposal completes the logging channel, drains queued entries, flushes the stream, closes the current file, and applies retention.

Logging is best-effort. Returning from a logging call does not mean the entry is on disk, and a process crash can lose queued or operating-system-buffered entries.
