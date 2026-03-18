namespace GameKit.Input;

public class KeyInputEvent : InputEvent
{
    public Scancode Scancode { get; }
    public VirtualKey Key { get; }
    public ulong Timestamp { get; }

    public KeyInputEvent(Scancode scancode, VirtualKey key, ulong timestamp)
    {
        Scancode = scancode;
        Key = key;
        Timestamp = timestamp;
    }
}
