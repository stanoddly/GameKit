namespace GameKit;

public class FrameContext
{


    // TODO: change access level
    public void Update(float delta)
    {
        TimeDelta = delta;
        TimeSinceStarted += delta;
        FrameNumber = +1;
    }
}