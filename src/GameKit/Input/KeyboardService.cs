using System.Runtime.InteropServices;
using SDL;

namespace GameKit.Input;

public class KeyEventArgs
{
    public Scancode Scancode { get; internal set; }
    public VirtualKey Key { get; internal set; }
    public ulong Timestamp { get; internal set; }
    public bool Consumed { get; internal set; }
    public void Consume() { Consumed = true; }
}

public delegate void KeyDownEventHandler(Keyboard keyboard, KeyEventArgs eventArgs);
public delegate void KeyUpEventHandler(Keyboard keyboard, KeyEventArgs eventArgs);

public class KeyboardService : IKeyboardService
{
    private readonly AppControl _appControl;

    // TODO: Dictionary isn't necessary, the amount of keyboards is usually truly small
    private readonly Dictionary<SDL_KeyboardID, Keyboard> _keyboards = new();

    // Cached to avoid per-event allocations. Do not hold references to event args beyond the callback.
    private readonly KeyEventArgs _keyEventArgs = new();
    private readonly ViewScopedPriorityEventHandlers<KeyDownEventHandler> _keyDownHandlers = new();
    private readonly ViewScopedPriorityEventHandlers<KeyUpEventHandler> _keyUpHandlers = new();

    public event KeyDownEventHandler KeyDown
    {
        add => _keyDownHandlers.Add(default, 0, value);
        remove => _keyDownHandlers.Remove(default, value);
    }

    public event KeyUpEventHandler KeyUp
    {
        add => _keyUpHandlers.Add(default, 0, value);
        remove => _keyUpHandlers.Remove(default, value);
    }

    public void SubscribeKeyDown(int priority, KeyDownEventHandler handler)
    {
        _keyDownHandlers.Add(default, priority, handler);
    }

    public void SubscribeKeyUp(int priority, KeyUpEventHandler handler)
    {
        _keyUpHandlers.Add(default, priority, handler);
    }

    public void SubscribeKeyDown(
        ViewScope viewScope,
        int priority,
        KeyDownEventHandler handler)
    {
        _keyDownHandlers.Add(viewScope, priority, handler);
    }

    public void SubscribeKeyUp(
        ViewScope viewScope,
        int priority,
        KeyUpEventHandler handler)
    {
        _keyUpHandlers.Add(viewScope, priority, handler);
    }

    internal KeyboardService(AppControl appControl)
    {
        _appControl = appControl;
    }

    internal void OnKeyEvent(ViewScope viewScope, in SDL_KeyboardEvent keyboardEvent)
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

        _keyEventArgs.Scancode = scancode;
        _keyEventArgs.Key = virtualKey;
        _keyEventArgs.Timestamp = timestamp;
        _keyEventArgs.Consumed = false;

        if (keyboardEvent.down)
        {
            if (keyboard.Set(scancode))
            {
                if (keyboard.Ctrl && scancode == Scancode.Q)
                {
                    _appControl.Quit();
                }

                foreach ((_, KeyDownEventHandler handler) in _keyDownHandlers.GetSorted(viewScope))
                {
                    handler(keyboard, _keyEventArgs);

                    if (_keyEventArgs.Consumed)
                    {
                        break;
                    }
                }
            }
        }
        else
        {
            keyboard.Unset(scancode);

            foreach ((_, KeyUpEventHandler handler) in _keyUpHandlers.GetSorted(viewScope))
            {
                handler(keyboard, _keyEventArgs);

                if (_keyEventArgs.Consumed)
                {
                    break;
                }
            }
        }
    }
}
