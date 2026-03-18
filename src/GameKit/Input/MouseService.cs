using System.Numerics;
using System.Runtime.InteropServices;
using SDL;

namespace GameKit.Input;

public enum MouseButton: uint
{
    Left = SDL.SDL_MouseButtonFlags.SDL_BUTTON_LMASK,
    Middle = SDL.SDL_MouseButtonFlags.SDL_BUTTON_MMASK,
    Right = SDL.SDL_MouseButtonFlags.SDL_BUTTON_RMASK,
    X1 = SDL.SDL_MouseButtonFlags.SDL_BUTTON_X1MASK,
    X2 = SDL.SDL_MouseButtonFlags.SDL_BUTTON_X2MASK
}

public class Mouse
{
    internal Mouse(SDL_MouseID mouseId)
    {
        MouseId = mouseId;
    }

    public SDL_MouseID MouseId { get; }
    public Vector2 Position { get; internal set; }
    public int ButtonFlags { get; internal set; }

    public bool IsPressed(MouseButton button)
    {
        return (ButtonFlags & (1 << ((int)button - 1))) != 0;
    }

    internal bool Set(MouseButton button)
    {
        int mask = 1 << ((int)button - 1);
        bool wasUnset = (ButtonFlags & mask) == 0;
        ButtonFlags |= mask;
        return wasUnset;
    }

    internal void Unset(MouseButton button)
    {
        int mask = 1 << ((int)button - 1);
        ButtonFlags &= ~mask;
    }
}

public class MouseButtonEventArgs
{
    public MouseButton Button { get; internal set; }
    public Vector2 Position { get; internal set; }
    public ulong Timestamp { get; internal set; }
    public bool Consumed { get; set; }
}

public class MouseMotionEventArgs
{
    public Vector2 Position { get; internal set; }
    public Vector2 RelativeMotion { get; internal set; }
    public ulong Timestamp { get; internal set; }
    public bool Consumed { get; set; }
}

public delegate void MouseButtonPressedHandler(Mouse mouse, MouseButtonEventArgs eventArgs);
public delegate void MouseButtonReleasedHandler(Mouse mouse, MouseButtonEventArgs eventArgs);
public delegate void MouseMotionHandler(Mouse mouse, MouseMotionEventArgs eventArgs);

public class MouseService : IMouseService
{
    private readonly Dictionary<SDL_MouseID, Mouse> _mice = new();
    private readonly MouseButtonEventArgs _buttonEventArgs = new();
    private readonly MouseMotionEventArgs _motionEventArgs = new();

    private readonly PriorityEventHandlers<MouseButtonPressedHandler> _buttonPressHandlers = new();
    private readonly PriorityEventHandlers<MouseButtonReleasedHandler> _buttonReleaseHandlers = new();
    private readonly PriorityEventHandlers<MouseMotionHandler> _motionHandlers = new();

    public event MouseButtonPressedHandler ButtonPress
    {
        add => _buttonPressHandlers.Add(0, value);
        remove => _buttonPressHandlers.Remove(value);
    }

    public event MouseButtonReleasedHandler ButtonRelease
    {
        add => _buttonReleaseHandlers.Add(0, value);
        remove => _buttonReleaseHandlers.Remove(value);
    }

    public event MouseMotionHandler Motion
    {
        add => _motionHandlers.Add(0, value);
        remove => _motionHandlers.Remove(value);
    }

    public void SubscribeButtonPress(int priority, MouseButtonPressedHandler handler)
    {
        _buttonPressHandlers.Add(priority, handler);
    }

    public void SubscribeButtonRelease(int priority, MouseButtonReleasedHandler handler)
    {
        _buttonReleaseHandlers.Add(priority, handler);
    }

    public void SubscribeMotion(int priority, MouseMotionHandler handler)
    {
        _motionHandlers.Add(priority, handler);
    }

    internal void OnMouseButtonEvent(in SDL_MouseButtonEvent mouseButtonEvent)
    {
        SDL_MouseID mouseId = mouseButtonEvent.which;
        MouseButton button = (MouseButton)mouseButtonEvent.button;
        Vector2 position = new(mouseButtonEvent.x, mouseButtonEvent.y);
        ulong timestamp = mouseButtonEvent.timestamp;

        ref Mouse? mouse = ref CollectionsMarshal.GetValueRefOrAddDefault(_mice, mouseId, out bool exists);

        if (!exists || mouse == null)
        {
            mouse = new Mouse(mouseId);
        }

        mouse.Position = position;

        _buttonEventArgs.Button = button;
        _buttonEventArgs.Position = position;
        _buttonEventArgs.Timestamp = timestamp;
        _buttonEventArgs.Consumed = false;

        if (mouseButtonEvent.down)
        {
            if (mouse.Set(button))
            {
                foreach ((_, MouseButtonPressedHandler handler) in _buttonPressHandlers.GetSorted())
                {
                    handler(mouse, _buttonEventArgs);
                }
            }
        }
        else
        {
            mouse.Unset(button);

            foreach ((_, MouseButtonReleasedHandler handler) in _buttonReleaseHandlers.GetSorted())
            {
                handler(mouse, _buttonEventArgs);
            }
        }
    }

    internal void OnMouseMotionEvent(in SDL_MouseMotionEvent mouseMotionEvent)
    {
        SDL_MouseID mouseId = mouseMotionEvent.which;
        Vector2 position = new(mouseMotionEvent.x, mouseMotionEvent.y);
        Vector2 relativeMotion = new(mouseMotionEvent.xrel, mouseMotionEvent.yrel);
        ulong timestamp = mouseMotionEvent.timestamp;

        ref Mouse? mouse = ref CollectionsMarshal.GetValueRefOrAddDefault(_mice, mouseId, out bool exists);

        if (!exists || mouse == null)
        {
            mouse = new Mouse(mouseId);
        }

        mouse.Position = position;

        _motionEventArgs.Position = position;
        _motionEventArgs.RelativeMotion = relativeMotion;
        _motionEventArgs.Timestamp = timestamp;
        _motionEventArgs.Consumed = false;

        foreach ((_, MouseMotionHandler handler) in _motionHandlers.GetSorted())
        {
            handler(mouse, _motionEventArgs);
        }
    }
}
