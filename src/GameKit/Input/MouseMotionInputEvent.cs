using System.Numerics;

namespace GameKit.Input;

public class MouseMotionInputEvent : InputEvent
{
    public Vector2 Position { get; }
    public Vector2 RelativeMotion { get; }
    public ulong Timestamp { get; }

    public MouseMotionInputEvent(Vector2 position, Vector2 relativeMotion, ulong timestamp)
    {
        Position = position;
        RelativeMotion = relativeMotion;
        Timestamp = timestamp;
    }
}
