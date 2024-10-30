using System.Numerics;
using System.Runtime.InteropServices;
using SDL;

namespace GameKit.Input;

public readonly record struct KeyEventArgs(ScanCode Code, ulong Timestamp);

public delegate void KeyDownEventHandler(Keyboard keyboard, in KeyEventArgs keyEventArgs);
public delegate void KeyUpEventHandler(Keyboard keyboard, in KeyEventArgs keyEventArgs);


public class InputService
{
    private readonly Dictionary<SDL_KeyboardID, Keyboard> _keyboards = new();

    public event KeyDownEventHandler? KeyDown;
    public event KeyUpEventHandler? KeyUp;
    
    internal void OnKeyEvent(in SDL_KeyboardEvent keyboardEvent)
    {
        ScanCode scanCode = (ScanCode)keyboardEvent.scancode;
        ulong timestamp = keyboardEvent.timestamp;
        SDL_KeyboardID keyboardId = keyboardEvent.which;
        
        ref Keyboard? keyboard = ref CollectionsMarshal.GetValueRefOrAddDefault(_keyboards, keyboardId, out bool exists);

        if (!exists || keyboard == null)
        {
            keyboard = new Keyboard();
        }

        KeyEventArgs keyEventArgs = new KeyEventArgs(scanCode, timestamp);
        if (keyboardEvent.down)
        {
            if (keyboard.Set(scanCode))
            {
                KeyDown?.Invoke(keyboard, keyEventArgs);
            }
        }
        else
        {
            keyboard.Unset(scanCode);
            KeyUp?.Invoke(keyboard, keyEventArgs);
        }
    }
}