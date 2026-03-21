using System.Collections.Concurrent;
using System.Diagnostics;

namespace GameKit.Logging;

public static class GameLog
{
    private static readonly BlockingCollection<string> LogQueue = new(boundedCapacity: 1024);

    private static readonly Thread FlushThread;

    static GameLog()
    {
        FlushThread = new Thread(FlushLoop)
        {
            Name = "GameKit.Logging",
            IsBackground = true
        };
        FlushThread.Start();
    }

    [Conditional("DEBUG")]
    public static void Debug(string message)
    {
        LogQueue.TryAdd(message);
    }

    [Conditional("DEBUG")]
    public static void Debug<T0>(string format, T0 arg0)
    {
        LogQueue.TryAdd(string.Format(format, arg0));
    }

    [Conditional("DEBUG")]
    public static void Debug<T0, T1>(string format, T0 arg0, T1 arg1)
    {
        LogQueue.TryAdd(string.Format(format, arg0, arg1));
    }

    [Conditional("DEBUG")]
    public static void Debug<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
    {
        LogQueue.TryAdd(string.Format(format, arg0, arg1, arg2));
    }

    private static void FlushLoop()
    {
        foreach (string message in LogQueue.GetConsumingEnumerable())
        {
            Console.WriteLine(message);
        }
    }
}
