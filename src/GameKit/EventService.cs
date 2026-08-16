using GameKit.Input;
using SDL;

namespace GameKit;

public class EventService
{
    private readonly KeyboardService _keyboardService;
    private readonly GamepadService _gamepadService;
    private readonly MouseService _mouseService;
    private readonly TextInputService _textInputService;
    private readonly WindowRegistry _windowRegistry;
    private readonly AppControl _appControl;

    internal EventService(
        KeyboardService keyboardService,
        GamepadService gamepadService,
        MouseService mouseService,
        TextInputService textInputService,
        WindowRegistry windowRegistry,
        AppControl appControl)
    {
        _keyboardService = keyboardService;
        _gamepadService = gamepadService;
        _mouseService = mouseService;
        _textInputService = textInputService;
        _windowRegistry = windowRegistry;
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
                    if (_windowRegistry.TryGetWindow((uint)evt.key.windowID, out Window keyDownWindow))
                    {
                        _keyboardService.OnKeyEvent(keyDownWindow, evt.key);
                    }
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_KEY_UP)
                {
                    if (_windowRegistry.TryGetWindow((uint)evt.key.windowID, out Window keyUpWindow))
                    {
                        _keyboardService.OnKeyEvent(keyUpWindow, evt.key);
                    }
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
                    if (_windowRegistry.TryGetWindow((uint)evt.button.windowID, out Window buttonDownWindow))
                    {
                        _mouseService.OnMouseButtonEvent(buttonDownWindow, evt.button);
                    }
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP)
                {
                    if (_windowRegistry.TryGetWindow((uint)evt.button.windowID, out Window buttonUpWindow))
                    {
                        _mouseService.OnMouseButtonEvent(buttonUpWindow, evt.button);
                    }
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_MOUSE_MOTION)
                {
                    if (_windowRegistry.TryGetWindow((uint)evt.motion.windowID, out Window motionWindow))
                    {
                        _mouseService.OnMouseMotionEvent(motionWindow, evt.motion);
                    }
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_MOUSE_WHEEL)
                {
                    if (_windowRegistry.TryGetWindow((uint)evt.wheel.windowID, out Window wheelWindow))
                    {
                        _mouseService.OnMouseWheelEvent(wheelWindow, evt.wheel);
                    }
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_WINDOW_MOUSE_ENTER)
                {
                    if (_windowRegistry.TryGetWindow((uint)evt.window.windowID, out Window enteredWindow))
                    {
                        _mouseService.OnMouseWindowPresenceEvent(enteredWindow, evt.window, true);
                    }
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_WINDOW_MOUSE_LEAVE)
                {
                    if (_windowRegistry.TryGetWindow((uint)evt.window.windowID, out Window leftWindow))
                    {
                        _mouseService.OnMouseWindowPresenceEvent(leftWindow, evt.window, false);
                    }
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_TEXT_INPUT)
                {
                    if (_windowRegistry.TryGetWindow((uint)evt.text.windowID, out Window textInputWindow))
                    {
                        _textInputService.OnTextInputEvent(textInputWindow, evt.text);
                    }
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_TEXT_EDITING)
                {
                    if (_windowRegistry.TryGetWindow((uint)evt.edit.windowID, out Window textEditingWindow))
                    {
                        _textInputService.OnTextEditingEvent(textEditingWindow, evt.edit);
                    }
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_WINDOW_PIXEL_SIZE_CHANGED)
                {
                    if (_windowRegistry.TryGetWindow((uint)evt.window.windowID, out Window pixelSizeWindow))
                    {
                        pixelSizeWindow.OnPixelSizeChanged(evt.window.timestamp);
                    }
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_WINDOW_CLOSE_REQUESTED)
                {
                    if (_windowRegistry.TryGetWindow((uint)evt.window.windowID, out Window closedWindow) &&
                        closedWindow.CloseBehavior == WindowCloseBehavior.QuitApplication)
                    {
                        _appControl.Quit();
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
