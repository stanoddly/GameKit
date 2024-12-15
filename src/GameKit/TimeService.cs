using SDL;

namespace GameKit;

public class TimeService
{
    internal TimeService()
    {
    }

    public ulong ElapsedMilliseconds() => SDL3.SDL_GetTicks();
    
    public ulong ElapsedNanoseconds() => SDL3.SDL_GetTicksNS();
    
    public float TimeDelta { get; protected set; }
    private ulong _lastFrameTime;

    public void StartFrame()
    {
        ulong previousFrame = _lastFrameTime;
        _lastFrameTime = ElapsedMilliseconds();
        
        if (previousFrame != 0)
        {
            TimeDelta = (previousFrame - _lastFrameTime) - 1;
        }
    }
}
