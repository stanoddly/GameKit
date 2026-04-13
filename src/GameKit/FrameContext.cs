using SDL;

namespace GameKit;

public abstract class FrameContext
{
    public ulong FrameNumber { get; protected set; }
    
    public TimeSpan ElapsedTime { get; protected set; }
    public ulong ElapsedNanoseconds { get; protected set; }

    public float TimeDelta { get; protected set; }
    public double TimeDelta64 { get; protected set; }
}

public class GameKitFrameContext: FrameContext
{
    // 100 ms = 0.1 seconds maximum delta time
    private const double MaxDeltaTime = 0.100;
    
    public GameKitFrameContext()
    {
    }

    public void StartFrame()
    {
        ulong previousElapsedNanoseconds = ElapsedNanoseconds;
        
        ElapsedNanoseconds = SDL3.SDL_GetTicksNS();

        // Yup divide! No rounding, that would give wrong results!
        // Also divide by 100, because TimeSpan accepts "ticks", where 1 tick = nanoseconds / 100
        ElapsedTime = new TimeSpan((long)(ElapsedNanoseconds / 100));

        // There could have been some loading, so the very first StartFrame would calculate several seconds! 😅
        if (previousElapsedNanoseconds != 0)
        {
            // Calculate the actual time delta
            double actualDelta = (ElapsedNanoseconds - previousElapsedNanoseconds) / 1_000_000_000.0;
            
            // Clamp the delta time to our maximum value
            TimeDelta64 = Math.Min(actualDelta, MaxDeltaTime);
            TimeDelta = (float)TimeDelta64;
        }

        FrameNumber += 1;
    }
}

public class TestFrameContext: FrameContext
{
    public void StartTestFrame(ulong elapsedMilliseconds)
    {
        ulong previousElapsedNanoseconds = ElapsedNanoseconds;
        
        ElapsedNanoseconds = elapsedMilliseconds * 1_000_000;

        // Yup divide! No rounding, that would give wrong results!
        // Also divide by 100, because TimeSpan accepts "ticks", where 1 tick = nanoseconds / 100
        ElapsedTime = new TimeSpan((long)(ElapsedNanoseconds / 100));

        TimeDelta64 = (ElapsedNanoseconds - previousElapsedNanoseconds) / 1_000_000_000.0;
        TimeDelta = (float)TimeDelta64;

        FrameNumber = +1;
    }
}