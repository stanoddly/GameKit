namespace GameKit;

public abstract class FrameContext
{
    public ulong FrameNumber { get; protected set; }

    public TimeSpan ElapsedTime { get; protected set; }
    public ulong ElapsedNanoseconds { get; protected set; }

    public float TimeDelta { get; protected set; }
    public double TimeDelta64 { get; protected set; }
}

public class TestFrameContext : FrameContext
{
    public void StartTestFrame(ulong elapsedMilliseconds)
    {
        ulong previousElapsedNanoseconds = ElapsedNanoseconds;

        ElapsedNanoseconds = elapsedMilliseconds * 1_000_000;

        ElapsedTime = new TimeSpan((long)(ElapsedNanoseconds / 100));

        TimeDelta64 = (ElapsedNanoseconds - previousElapsedNanoseconds) / 1_000_000_000.0;
        TimeDelta = (float)TimeDelta64;

        FrameNumber = +1;
    }
}
