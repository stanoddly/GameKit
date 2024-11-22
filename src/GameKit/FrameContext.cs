namespace GameKit;

public class FrameContext
{
    public float TimeDelta { get; protected set; }
    public ulong FrameNumber { get; protected set; }
    public float TimeSinceStarted { get; protected set; }

    // TODO: change access level
    public void Update(float delta)
    {
        TimeDelta = delta;
        TimeSinceStarted += delta;
        FrameNumber = +1;
    }
}