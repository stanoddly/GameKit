using System.Numerics;
using System.Runtime.InteropServices;
using GameKit.Utilities;
using SDL;

namespace GameKit.Input;

public enum MouseButton : byte
{
    Left = 1,
    Middle = 2,
    Right = 3,
    X1 = 4,
    X2 = 5
}

public class Mouse
{
    internal Mouse(SDL_MouseID mouseId)
    {
        MouseId = mouseId;
    }

    public SDL_MouseID MouseId { get; }
    public MouseState State { get; private set; }

    public Vector2 Position
    {
        get
        {
            return State.Position;
        }
        internal set
        {
            State = State with { Position = value };
        }
    }

    public int ButtonFlags
    {
        get
        {
            return State.ButtonFlags;
        }
        internal set
        {
            State = State with { ButtonFlags = value };
        }
    }

    public bool IsPressed(MouseButton button)
    {
        return State.IsPressed(button);
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
    public ViewScope ViewScope { get; internal set; }
    public MouseButton Button { get; internal set; }
    public Vector2 Position { get; internal set; }
    public ulong Timestamp { get; internal set; }
    public bool Consumed { get; internal set; }
    public void Consume() { Consumed = true; }
}

public class MouseMotionEventArgs
{
    public ViewScope ViewScope { get; internal set; }
    public Vector2 Position { get; internal set; }
    public Vector2 RelativeMotion { get; internal set; }
    public ulong Timestamp { get; internal set; }
    public bool Consumed { get; internal set; }
    public void Consume() { Consumed = true; }
}

public class MouseWheelEventArgs
{
    public ViewScope ViewScope { get; internal set; }
    public Vector2 Delta { get; internal set; }
    public Vector2 Position { get; internal set; }
    public ulong Timestamp { get; internal set; }
    public bool Consumed { get; internal set; }
    public void Consume() { Consumed = true; }
}

public class MouseWindowPresenceEventArgs
{
    public ViewScope ViewScope { get; internal set; }
    public bool IsInWindow { get; internal set; }
    public ulong Timestamp { get; internal set; }
}

public delegate void MouseButtonPressedHandler(Mouse mouse, MouseButtonEventArgs eventArgs);
public delegate void MouseButtonReleasedHandler(Mouse mouse, MouseButtonEventArgs eventArgs);
public delegate void MouseMotionHandler(Mouse mouse, MouseMotionEventArgs eventArgs);
public delegate void MouseWheelHandler(Mouse mouse, MouseWheelEventArgs eventArgs);
public delegate void MouseWindowPresenceHandler(MouseWindowPresenceEventArgs eventArgs);

public class MouseService : IMouseService
{
    private readonly WindowRegistry _windowRegistry;
    private readonly Dictionary<SDL_MouseID, Mouse> _mice = new();

    // Cached to avoid per-event allocations. Do not hold references to event args beyond the callback.
    private readonly MouseButtonEventArgs _buttonEventArgs = new();
    private readonly MouseMotionEventArgs _motionEventArgs = new();
    private readonly MouseWheelEventArgs _wheelEventArgs = new();
    private readonly MouseWindowPresenceEventArgs _windowPresenceEventArgs = new();

    private readonly PriorityEventHandlers<MouseButtonPressedHandler> _buttonPressHandlers = new();
    private readonly PriorityEventHandlers<MouseButtonReleasedHandler> _buttonReleaseHandlers = new();
    private readonly PriorityEventHandlers<MouseMotionHandler> _motionHandlers = new();
    private readonly PriorityEventHandlers<MouseWheelHandler> _wheelHandlers = new();
    private readonly PriorityEventHandlers<MouseWindowPresenceHandler> _windowEnterHandlers = new();
    private readonly PriorityEventHandlers<MouseWindowPresenceHandler> _windowLeaveHandlers = new();

    internal MouseService(WindowRegistry windowRegistry)
    {
        _windowRegistry = windowRegistry;
    }

    public bool IsInWindow(ViewScope viewScope)
    {
        Window window = _windowRegistry.GetWindow(viewScope);
        unsafe
        {
            Pointer<SDL_Window> mouseFocusWindow = SDL3.SDL_GetMouseFocus();
            return !mouseFocusWindow.IsNull &&
                (uint)SDL3.SDL_GetWindowID(mouseFocusWindow) == window.SdlId;
        }
    }

    public MouseState GetGlobalState()
    {
        float x;
        float y;
        SDL_MouseButtonFlags buttonFlags;

        unsafe
        {
            buttonFlags = SDL3.SDL_GetGlobalMouseState(&x, &y);
        }

        return new MouseState(new Vector2(x, y), (int)buttonFlags);
    }

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

    public event MouseWheelHandler Wheel
    {
        add => _wheelHandlers.Add(0, value);
        remove => _wheelHandlers.Remove(value);
    }

    public event MouseWindowPresenceHandler WindowEnter
    {
        add => _windowEnterHandlers.Add(0, value);
        remove => _windowEnterHandlers.Remove(value);
    }

    public event MouseWindowPresenceHandler WindowLeave
    {
        add => _windowLeaveHandlers.Add(0, value);
        remove => _windowLeaveHandlers.Remove(value);
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

    public void SubscribeWheel(int priority, MouseWheelHandler handler)
    {
        _wheelHandlers.Add(priority, handler);
    }

    public void SubscribeWindowEnter(int priority, MouseWindowPresenceHandler handler)
    {
        _windowEnterHandlers.Add(priority, handler);
    }

    public void SubscribeWindowLeave(int priority, MouseWindowPresenceHandler handler)
    {
        _windowLeaveHandlers.Add(priority, handler);
    }

    public void SubscribeButtonPress(
        ViewScope viewScope,
        int priority,
        MouseButtonPressedHandler handler)
    {
        _buttonPressHandlers.Add(priority, (mouse, eventArgs) =>
        {
            if (eventArgs.ViewScope == viewScope)
            {
                handler(mouse, eventArgs);
            }
        });
    }

    public void SubscribeButtonRelease(
        ViewScope viewScope,
        int priority,
        MouseButtonReleasedHandler handler)
    {
        _buttonReleaseHandlers.Add(priority, (mouse, eventArgs) =>
        {
            if (eventArgs.ViewScope == viewScope)
            {
                handler(mouse, eventArgs);
            }
        });
    }

    public void SubscribeMotion(ViewScope viewScope, int priority, MouseMotionHandler handler)
    {
        _motionHandlers.Add(priority, (mouse, eventArgs) =>
        {
            if (eventArgs.ViewScope == viewScope)
            {
                handler(mouse, eventArgs);
            }
        });
    }

    public void SubscribeWheel(ViewScope viewScope, int priority, MouseWheelHandler handler)
    {
        _wheelHandlers.Add(priority, (mouse, eventArgs) =>
        {
            if (eventArgs.ViewScope == viewScope)
            {
                handler(mouse, eventArgs);
            }
        });
    }

    public void SubscribeWindowEnter(
        ViewScope viewScope,
        int priority,
        MouseWindowPresenceHandler handler)
    {
        _windowEnterHandlers.Add(priority, eventArgs =>
        {
            if (eventArgs.ViewScope == viewScope)
            {
                handler(eventArgs);
            }
        });
    }

    public void SubscribeWindowLeave(
        ViewScope viewScope,
        int priority,
        MouseWindowPresenceHandler handler)
    {
        _windowLeaveHandlers.Add(priority, eventArgs =>
        {
            if (eventArgs.ViewScope == viewScope)
            {
                handler(eventArgs);
            }
        });
    }

    internal void OnMouseWindowPresenceEvent(
        ViewScope viewScope,
        in SDL_WindowEvent windowEvent,
        bool isInWindow)
    {
        _windowPresenceEventArgs.ViewScope = viewScope;
        _windowPresenceEventArgs.IsInWindow = isInWindow;
        _windowPresenceEventArgs.Timestamp = windowEvent.timestamp;

        PriorityEventHandlers<MouseWindowPresenceHandler> handlers = isInWindow
            ? _windowEnterHandlers
            : _windowLeaveHandlers;

        foreach ((_, MouseWindowPresenceHandler handler) in handlers.GetSorted())
        {
            handler(_windowPresenceEventArgs);
        }
    }

    internal void OnMouseButtonEvent(
        ViewScope viewScope,
        in SDL_MouseButtonEvent mouseButtonEvent)
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

        _buttonEventArgs.ViewScope = viewScope;
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

                    if (_buttonEventArgs.Consumed)
                    {
                        break;
                    }
                }
            }
        }
        else
        {
            mouse.Unset(button);

            foreach ((_, MouseButtonReleasedHandler handler) in _buttonReleaseHandlers.GetSorted())
            {
                handler(mouse, _buttonEventArgs);

                if (_buttonEventArgs.Consumed)
                {
                    break;
                }
            }
        }
    }

    internal void OnMouseMotionEvent(
        ViewScope viewScope,
        in SDL_MouseMotionEvent mouseMotionEvent)
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

        _motionEventArgs.ViewScope = viewScope;
        _motionEventArgs.Position = position;
        _motionEventArgs.RelativeMotion = relativeMotion;
        _motionEventArgs.Timestamp = timestamp;
        _motionEventArgs.Consumed = false;

        foreach ((_, MouseMotionHandler handler) in _motionHandlers.GetSorted())
        {
            handler(mouse, _motionEventArgs);

            if (_motionEventArgs.Consumed)
            {
                break;
            }
        }
    }

    internal void OnMouseWheelEvent(
        ViewScope viewScope,
        in SDL_MouseWheelEvent mouseWheelEvent)
    {
        SDL_MouseID mouseId = mouseWheelEvent.which;
        Vector2 delta = new(mouseWheelEvent.x, mouseWheelEvent.y);
        Vector2 position = new(mouseWheelEvent.mouse_x, mouseWheelEvent.mouse_y);
        ulong timestamp = mouseWheelEvent.timestamp;

        ref Mouse? mouse = ref CollectionsMarshal.GetValueRefOrAddDefault(_mice, mouseId, out bool exists);

        if (!exists || mouse == null)
        {
            mouse = new Mouse(mouseId);
        }

        mouse.Position = position;

        _wheelEventArgs.ViewScope = viewScope;
        _wheelEventArgs.Delta = delta;
        _wheelEventArgs.Position = position;
        _wheelEventArgs.Timestamp = timestamp;
        _wheelEventArgs.Consumed = false;

        foreach ((_, MouseWheelHandler handler) in _wheelHandlers.GetSorted())
        {
            handler(mouse, _wheelEventArgs);

            if (_wheelEventArgs.Consumed)
            {
                break;
            }
        }
    }
}
