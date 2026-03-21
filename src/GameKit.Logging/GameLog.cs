using System.Diagnostics;
using System.Threading.Channels;

namespace GameKit.Logging;

public static class GameLog
{
    private static readonly Channel<string> LogChannel = Channel.CreateBounded<string>(
        new BoundedChannelOptions(1024)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true
        });

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
        LogChannel.Writer.TryWrite(message);
    }

    [Conditional("DEBUG")]
    public static void Debug<T0>(string format, T0 arg0)
    {
        LogChannel.Writer.TryWrite(string.Format(format, arg0));
    }

    [Conditional("DEBUG")]
    public static void Debug<T0, T1>(string format, T0 arg0, T1 arg1)
    {
        LogChannel.Writer.TryWrite(string.Format(format, arg0, arg1));
    }

    [Conditional("DEBUG")]
    public static void Debug<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
    {
        LogChannel.Writer.TryWrite(string.Format(format, arg0, arg1, arg2));
    }

    private static void FlushLoop()
    {
        ChannelReader<string> reader = LogChannel.Reader;

        while (true)
        {
            try
            {
                while (reader.TryRead(out string? message))
                {
                    Console.WriteLine(message);
                }

                reader.WaitToReadAsync().AsTask().GetAwaiter().GetResult();
            }
            catch (ChannelClosedException)
            {
                break;
            }
        }
    }
}
