using SDL;

namespace GameKit;

public class FrameContext
{
    public ulong FrameNumber { get; protected set; }
    
    public TimeSpan ElapsedTime { get; protected set; }
    public ulong ElapsedNanoseconds { get; protected set; }

    public float TimeDelta { get; protected set; }
    public double TimeDelta64 { get; protected set; }
    
    // internal to force SDL3 initialization
    internal FrameContext()
    {
    }

    public void StartFrame()
    {
        ulong previousElapsedNanoseconds = ElapsedNanoseconds;
        
        ElapsedNanoseconds = SDL3.SDL_GetTicksNS();

        // Yup divide! No rounding, that would give wrong results!
        // Also divide by 100, because TimeSpan accepts "ticks", where 1 tick = nanoseconds / 100
        ElapsedTime = new TimeSpan((long)(ElapsedNanoseconds / 100));

        // There could have been some loading, so the very first StartFrame would calculate in several seconds! 😅
        if (previousElapsedNanoseconds != 0)
        {
            TimeDelta64 = (ElapsedNanoseconds - previousElapsedNanoseconds) / 1_000_000.0;
            TimeDelta = (float)TimeDelta64;
        }

        FrameNumber = +1;
    }
}
