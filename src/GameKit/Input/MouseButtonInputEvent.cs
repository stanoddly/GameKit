using System.Numerics;

namespace GameKit.Input;

public class MouseButtonInputEvent : InputEvent
{
    public MouseButton Button { get; }
    public Vector2 Position { get; }
    public ulong Timestamp { get; }

    public MouseButtonInputEvent(MouseButton button, Vector2 position, ulong timestamp)
    {
        Button = button;
        Position = position;
        Timestamp = timestamp;
    }
}
