using System.Runtime.InteropServices;
using SDL;

namespace GameKit.Input;

public record KeyEventArgs(Scancode Scancode, VirtualKey Key, ulong Timestamp)
{
    public bool Consumed { get; set; }
}

public delegate void KeyDownEventHandler(Keyboard keyboard, KeyEventArgs eventArgs);
public delegate void KeyUpEventHandler(Keyboard keyboard, KeyEventArgs eventArgs);

public class KeyboardService : IKeyboardService
{
    private readonly AppControl _appControl;

    // TODO: Dictionary isn't necessary, the amount of keyboards is usually truly small
    private readonly Dictionary<SDL_KeyboardID, Keyboard> _keyboards = new();

    private readonly PriorityEventHandlers<KeyDownEventHandler> _keyDownHandlers = new();
    private readonly PriorityEventHandlers<KeyUpEventHandler> _keyUpHandlers = new();

    public event KeyDownEventHandler KeyDown
    {
        add => _keyDownHandlers.Add(0, value);
        remove => _keyDownHandlers.Remove(value);
    }

    public event KeyUpEventHandler KeyUp
    {
        add => _keyUpHandlers.Add(0, value);
        remove => _keyUpHandlers.Remove(value);
    }

    public void SubscribeKeyDown(int priority, KeyDownEventHandler handler)
    {
        _keyDownHandlers.Add(priority, handler);
    }

    public void SubscribeKeyUp(int priority, KeyUpEventHandler handler)
    {
        _keyUpHandlers.Add(priority, handler);
    }

    internal KeyboardService(AppControl appControl)
    {
        _appControl = appControl;
    }

    internal void OnKeyEvent(in SDL_KeyboardEvent keyboardEvent)
    {
        Scancode scancode = (Scancode)keyboardEvent.scancode;
        ulong timestamp = keyboardEvent.timestamp;
        SDL_KeyboardID keyboardId = keyboardEvent.which;
        VirtualKey virtualKey = (VirtualKey)keyboardEvent.key;

        ref Keyboard? keyboard = ref CollectionsMarshal.GetValueRefOrAddDefault(_keyboards, keyboardId, out bool exists);

        if (!exists || keyboard == null)
        {
            keyboard = new Keyboard();
        }

        KeyEventArgs eventArgs = new(scancode, virtualKey, timestamp);

        if (keyboardEvent.down)
        {
            if (keyboard.Set(scancode))
            {
                if (keyboard.Ctrl && scancode == Scancode.Q)
                {
                    _appControl.Quit();
                }

                foreach ((_, KeyDownEventHandler handler) in _keyDownHandlers.GetSorted())
                {
                    handler(keyboard, eventArgs);
                }
            }
        }
        else
        {
            keyboard.Unset(scancode);

            foreach ((_, KeyUpEventHandler handler) in _keyUpHandlers.GetSorted())
            {
                handler(keyboard, eventArgs);
            }
        }
    }
}
