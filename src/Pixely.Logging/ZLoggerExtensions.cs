using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Pixely.Logging;

public static class ZLoggerExtensions
{
    [Conditional("DEBUG")]
    public static void ZLogConditionalDebug(
        this ILogger logger,
        [InterpolatedStringHandlerArgument("logger")]
        ref ZLoggerDebugInterpolatedStringHandler message,
        EventId eventId = default,
        Exception? exception = null,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLog(
            LogLevel.Debug,
            eventId,
            exception,
            ref message.InnerHandler,
            context,
            memberName,
            filePath,
            lineNumber);
    }
}
