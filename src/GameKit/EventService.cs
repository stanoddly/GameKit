using GameKit.Input;
using SDL;

namespace GameKit;

public class EventService
{
    private readonly KeyboardService _keyboardService;
    private readonly GamepadService _gamepadService;
    private readonly MouseService _mouseService;
    private readonly Window _window;
    private readonly AppControl _appControl;

    internal EventService(KeyboardService keyboardService, GamepadService gamepadService, MouseService mouseService, Window window, AppControl appControl)
    {
        _keyboardService = keyboardService;
        _gamepadService = gamepadService;
        _mouseService = mouseService;
        _window = window;
        _appControl = appControl;
    }

    public void Process()
    {
        unsafe
        {
            SDL_Event evt;
            while (SDL3.SDL_PollEvent(&evt) == true)
            {
                if (evt.Type == SDL_EventType.SDL_EVENT_KEY_DOWN)
                {
                    _keyboardService.OnKeyEvent(evt.key);
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_KEY_UP)
                {
                    _keyboardService.OnKeyEvent(evt.key);
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_GAMEPAD_ADDED)
                {
                    _gamepadService.OnGamepadAdded(evt.gdevice.which);
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_GAMEPAD_REMOVED)
                {
                    _gamepadService.OnGamepadRemoved(evt.gdevice.which);
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_GAMEPAD_AXIS_MOTION)
                {
                    _gamepadService.OnGamepadStickMotion(in evt.gaxis);
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_GAMEPAD_BUTTON_DOWN)
                {
                    _gamepadService.OnGamepadButtonPressed(evt.gbutton);
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_GAMEPAD_BUTTON_UP)
                {
                    _gamepadService.OnGamepadButtonReleased(evt.gbutton);
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN)
                {
                    _mouseService.OnMouseButtonEvent(evt.button);
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP)
                {
                    _mouseService.OnMouseButtonEvent(evt.button);
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_MOUSE_MOTION)
                {
                    _mouseService.OnMouseMotionEvent(evt.motion);
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_MOUSE_WHEEL)
                {
                    _mouseService.OnMouseWheelEvent(evt.wheel);
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_WINDOW_PIXEL_SIZE_CHANGED)
                {
                    _window.OnPixelSizeChanged(evt.window.timestamp);
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_QUIT)
                {
                    _appControl.Quit();
                }
            }
        }
    }
}
