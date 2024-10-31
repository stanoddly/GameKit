using System.Runtime.InteropServices;
using SDL;

namespace GameKit.Input;

public readonly record struct KeyEventArgs(Scancode Scancode, VirtualKey Key, ulong Timestamp);

public delegate void KeyDownEventHandler(Keyboard keyboard, in KeyEventArgs keyEventArgs);
public delegate void KeyUpEventHandler(Keyboard keyboard, in KeyEventArgs keyEventArgs);


public class InputService
{
    private readonly Dictionary<SDL_KeyboardID, Keyboard> _keyboards = new();

    public event KeyDownEventHandler? KeyDown;
    public event KeyUpEventHandler? KeyUp;
    
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

        KeyEventArgs keyEventArgs = new KeyEventArgs(scancode, virtualKey, timestamp);
        if (keyboardEvent.down)
        {
            if (keyboard.Set(scancode))
            {
                KeyDown?.Invoke(keyboard, keyEventArgs);
            }
        }
        else
        {
            keyboard.Unset(scancode);
            KeyUp?.Invoke(keyboard, keyEventArgs);
        }
    }
}