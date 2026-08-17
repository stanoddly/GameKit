using System.Runtime.InteropServices;
using SDL;

namespace GameKit.Input;

public class KeyEventArgs : ConsumableInputEventArgs
{
    public Scancode Scancode { get; internal set; }
    public VirtualKey Key { get; internal set; }
    public ulong Timestamp { get; internal set; }
}

public class KeyboardService : IKeyboardService
{
    private readonly AppControl _appControl;

    // TODO: Dictionary isn't necessary, the amount of keyboards is usually truly small
    private readonly Dictionary<SDL_KeyboardID, Keyboard> _keyboards = new();

    // Cached to avoid per-event allocations. Do not hold references to event args beyond the callback.
    private readonly KeyEventArgs _keyEventArgs = new();
    private readonly ViewScopedPriorityEventHandlers<Keyboard, KeyEventArgs> _keyDownHandlers = new();
    private readonly ViewScopedPriorityEventHandlers<Keyboard, KeyEventArgs> _keyUpHandlers = new();

    public event InputEventHandler<Keyboard, KeyEventArgs> KeyDown
    {
        add => _keyDownHandlers.Add(default, 0, value);
        remove => _keyDownHandlers.Remove(default, value);
    }

    public event InputEventHandler<Keyboard, KeyEventArgs> KeyUp
    {
        add => _keyUpHandlers.Add(default, 0, value);
        remove => _keyUpHandlers.Remove(default, value);
    }

    public void SubscribeKeyDown(int priority, InputEventHandler<Keyboard, KeyEventArgs> handler)
    {
        _keyDownHandlers.Add(default, priority, handler);
    }

    public void SubscribeKeyUp(int priority, InputEventHandler<Keyboard, KeyEventArgs> handler)
    {
        _keyUpHandlers.Add(default, priority, handler);
    }

    public void SubscribeKeyDown(
        ViewScope viewScope,
        int priority,
        InputEventHandler<Keyboard, KeyEventArgs> handler)
    {
        _keyDownHandlers.Add(viewScope, priority, handler);
    }

    public void SubscribeKeyUp(
        ViewScope viewScope,
        int priority,
        InputEventHandler<Keyboard, KeyEventArgs> handler)
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
        if (keyboardEvent.down)
        {
            if (keyboard.Set(scancode))
            {
                if (keyboard.Ctrl && scancode == Scancode.Q)
                {
                    _appControl.Quit();
                }

                _keyDownHandlers.Invoke(viewScope, keyboard, _keyEventArgs);
            }
        }
        else
        {
            keyboard.Unset(scancode);

            _keyUpHandlers.Invoke(viewScope, keyboard, _keyEventArgs);
        }
    }
}
