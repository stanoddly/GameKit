using System.Runtime.InteropServices;
using SDL;

namespace GameKit.Input;

public delegate void KeyDownEventHandler(Keyboard keyboard, KeyInputEvent inputEvent);
public delegate void KeyUpEventHandler(Keyboard keyboard, KeyInputEvent inputEvent);

public class KeyboardService : IKeyboardService
{
    private readonly AppControl _appControl;
    private readonly IKeyDownHandler[] _keyDownHandlers;
    private readonly IKeyUpHandler[] _keyUpHandlers;

    // TODO: Dictionary isn't necessary, the amount of keyboards is usually truly small
    private readonly Dictionary<SDL_KeyboardID, Keyboard> _keyboards = new();

    public event KeyDownEventHandler? KeyDown;
    public event KeyUpEventHandler? KeyUp;

    internal KeyboardService(
        AppControl appControl,
        IEnumerable<IKeyDownHandler> keyDownHandlers,
        IEnumerable<IKeyUpHandler> keyUpHandlers)
    {
        _appControl = appControl;
        _keyDownHandlers = keyDownHandlers.OrderBy(h => h.Order).ToArray();
        _keyUpHandlers = keyUpHandlers.OrderBy(h => h.Order).ToArray();
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

        KeyInputEvent inputEvent = new(scancode, virtualKey, timestamp);

        if (keyboardEvent.down)
        {
            if (keyboard.Set(scancode))
            {
                if (keyboard.Ctrl && scancode == Scancode.Q)
                {
                    _appControl.Quit();
                }

                foreach (IKeyDownHandler handler in _keyDownHandlers)
                {
                    handler.OnKeyDown(keyboard, inputEvent);
                }

                if (!inputEvent.Consumed)
                {
                    KeyDown?.Invoke(keyboard, inputEvent);
                }
            }
        }
        else
        {
            keyboard.Unset(scancode);

            foreach (IKeyUpHandler handler in _keyUpHandlers)
            {
                handler.OnKeyUp(keyboard, inputEvent);
            }

            if (!inputEvent.Consumed)
            {
                KeyUp?.Invoke(keyboard, inputEvent);
            }
        }
    }
}
