using System.Collections.Concurrent;
using System.Diagnostics;

namespace GameKit.Logging;

/// <summary>
/// Lightweight debug logging designed for game loops.
/// <para>
/// Standard .NET logging (ILogger / Microsoft.Extensions.Logging) is unsuitable for
/// per-frame debug output: the extension methods allocate a params object[] on every call,
/// scopes add overhead, and the abstraction layers are unnecessary for a single console sink.
/// </para>
/// <para>
/// All public methods use [Conditional("DEBUG")] so calls are stripped entirely from release
/// builds at the call site — zero cost, no level checks, no IL emitted. Generic overloads
/// avoid boxing value-type arguments. A background thread drains a bounded queue to
/// Console.WriteLine, keeping I/O off the calling thread.
/// </para>
/// </summary>
public static class Log
{
    private static BlockingCollection<string>? LogQueue;

    static Log()
    {
        Initialize();
    }

    [Conditional("DEBUG")]
    private static void Initialize()
    {
        LogQueue = new BlockingCollection<string>(boundedCapacity: 1024);

        Thread flushThread = new Thread(FlushLoop)
        {
            Name = "GameKit.Logging",
            IsBackground = true
        };
        flushThread.Start();
    }

    [Conditional("DEBUG")]
    public static void Debug(string message)
    {
        LogQueue!.TryAdd(message);
    }

    [Conditional("DEBUG")]
    public static void Debug<T0>(string format, T0 arg0)
    {
        LogQueue!.TryAdd(string.Format(format, arg0));
    }

    [Conditional("DEBUG")]
    public static void Debug<T0, T1>(string format, T0 arg0, T1 arg1)
    {
        LogQueue!.TryAdd(string.Format(format, arg0, arg1));
    }

    [Conditional("DEBUG")]
    public static void Debug<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
    {
        LogQueue!.TryAdd(string.Format(format, arg0, arg1, arg2));
    }

    private static void FlushLoop()
    {
        foreach (string message in LogQueue!.GetConsumingEnumerable())
        {
            Console.WriteLine(message);
        }
    }
}
