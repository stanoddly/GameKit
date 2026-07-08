using GameKit.Input;
using SDL;

namespace GameKit;

public class EventService
{
    private readonly KeyboardService _keyboardService;
    private readonly GamepadService _gamepadService;
    private readonly MouseService _mouseService;
    private readonly TextInputService _textInputService;
    private readonly WindowManager _windowManager;
    private readonly AppControl _appControl;

    internal EventService(KeyboardService keyboardService, GamepadService gamepadService, MouseService mouseService, TextInputService textInputService, WindowManager windowManager, AppControl appControl)
    {
        _keyboardService = keyboardService;
        _gamepadService = gamepadService;
        _mouseService = mouseService;
        _textInputService = textInputService;
        _windowManager = windowManager;
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
                else if (evt.Type == SDL_EventType.SDL_EVENT_WINDOW_MOUSE_ENTER)
                {
                    if ((uint)evt.window.windowID == _windowManager.PrimaryWindow.Id)
                    {
                        _mouseService.OnMouseWindowPresenceEvent(evt.window, true);
                    }
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_WINDOW_MOUSE_LEAVE)
                {
                    if ((uint)evt.window.windowID == _windowManager.PrimaryWindow.Id)
                    {
                        _mouseService.OnMouseWindowPresenceEvent(evt.window, false);
                    }
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_TEXT_INPUT)
                {
                    _textInputService.OnTextInputEvent(evt.text);
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_TEXT_EDITING)
                {
                    _textInputService.OnTextEditingEvent(evt.edit);
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_WINDOW_PIXEL_SIZE_CHANGED)
                {
                    if (_windowManager.TryGetWindow((uint)evt.window.windowID, out Window pixelSizeWindow))
                    {
                        pixelSizeWindow.OnPixelSizeChanged(evt.window.timestamp);
                    }
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_WINDOW_CLOSE_REQUESTED)
                {
                    if (_windowManager.TryGetWindow((uint)evt.window.windowID, out Window closedWindow))
                    {
                        if (closedWindow == _windowManager.PrimaryWindow)
                        {
                            _appControl.Quit();
                        }
                        else
                        {
                            _windowManager.DestroyWindow(closedWindow);
                        }
                    }
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_QUIT)
                {
                    _appControl.Quit();
                }
            }
        }
    }
}
