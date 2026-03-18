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

public delegate void MouseButtonPressedHandler(Mouse mouse, MouseButtonInputEvent inputEvent);
public delegate void MouseButtonReleasedHandler(Mouse mouse, MouseButtonInputEvent inputEvent);
public delegate void MouseMotionHandler(Mouse mouse, MouseMotionInputEvent inputEvent);

public class MouseService : IMouseService
{
    private readonly Dictionary<SDL_MouseID, Mouse> _mice = new();
    private readonly IMouseButtonPressHandler[] _pressHandlers;
    private readonly IMouseButtonReleaseHandler[] _releaseHandlers;
    private readonly IMouseMotionHandler[] _motionHandlers;

    public event MouseButtonPressedHandler? ButtonPress;
    public event MouseButtonReleasedHandler? ButtonRelease;
    public event MouseMotionHandler? Motion;

    public MouseService(
        IEnumerable<IMouseButtonPressHandler> pressHandlers,
        IEnumerable<IMouseButtonReleaseHandler> releaseHandlers,
        IEnumerable<IMouseMotionHandler> motionHandlers)
    {
        _pressHandlers = pressHandlers.OrderBy(h => h.Order).ToArray();
        _releaseHandlers = releaseHandlers.OrderBy(h => h.Order).ToArray();
        _motionHandlers = motionHandlers.OrderBy(h => h.Order).ToArray();
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

        MouseButtonInputEvent inputEvent = new(button, position, timestamp);

        if (mouseButtonEvent.down)
        {
            if (mouse.Set(button))
            {
                foreach (IMouseButtonPressHandler handler in _pressHandlers)
                {
                    handler.OnButtonPress(mouse, inputEvent);
                }

                if (!inputEvent.Consumed)
                {
                    ButtonPress?.Invoke(mouse, inputEvent);
                }
            }
        }
        else
        {
            mouse.Unset(button);

            foreach (IMouseButtonReleaseHandler handler in _releaseHandlers)
            {
                handler.OnButtonRelease(mouse, inputEvent);
            }

            if (!inputEvent.Consumed)
            {
                ButtonRelease?.Invoke(mouse, inputEvent);
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

        MouseMotionInputEvent inputEvent = new(position, relativeMotion, timestamp);

        foreach (IMouseMotionHandler handler in _motionHandlers)
        {
            handler.OnMotion(mouse, inputEvent);
        }

        if (!inputEvent.Consumed)
        {
            Motion?.Invoke(mouse, inputEvent);
        }
    }
}
