using GameKit.Input;
using SDL;

namespace GameKit;

public class EventService
{
    private readonly AppControl _appControl;
    private readonly GamepadService _gamepadService;
    private readonly WindowManager _windowManager;

    internal EventService(
        GamepadService gamepadService,
        WindowManager windowManager,
        AppControl appControl)
    {
        _gamepadService = gamepadService;
        _windowManager = windowManager;
        _appControl = appControl;
    }

    public void Process()
    {
        unsafe
        {
            SDL_Event evt;
            while (SDL3.SDL_PollEvent(&evt))
            {
                Process(in evt);
            }
        }
    }

    internal void Process(in SDL_Event evt)
    {
        if (evt.Type == SDL_EventType.SDL_EVENT_GAMEPAD_ADDED)
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
        else if (evt.Type == SDL_EventType.SDL_EVENT_QUIT)
        {
            _appControl.Quit();
        }
        else
        {
            uint windowId = GetWindowId(in evt);
            if (windowId != 0 && _windowManager.TryGetWindow(windowId, out Window window))
            {
                if (evt.Type == SDL_EventType.SDL_EVENT_WINDOW_PIXEL_SIZE_CHANGED)
                {
                    window.OnPixelSizeChanged(evt.window.timestamp);
                }
                else if (evt.Type == SDL_EventType.SDL_EVENT_WINDOW_CLOSE_REQUESTED)
                {
                    if (window.StopGameOnClose)
                    {
                        _appControl.Quit();
                    }
                    else
                    {
                        _windowManager.DestroyWindow(window);
                    }
                }
                else
                {
                    window.ProcessEvent(in evt);
                }
            }
        }
    }

    private static uint GetWindowId(in SDL_Event evt)
    {
        return evt.Type switch
        {
            SDL_EventType.SDL_EVENT_KEY_DOWN or SDL_EventType.SDL_EVENT_KEY_UP =>
                (uint)evt.key.windowID,
            SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN or SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP =>
                (uint)evt.button.windowID,
            SDL_EventType.SDL_EVENT_MOUSE_MOTION => (uint)evt.motion.windowID,
            SDL_EventType.SDL_EVENT_MOUSE_WHEEL => (uint)evt.wheel.windowID,
            SDL_EventType.SDL_EVENT_TEXT_INPUT => (uint)evt.text.windowID,
            SDL_EventType.SDL_EVENT_TEXT_EDITING => (uint)evt.edit.windowID,
            >= SDL_EventType.SDL_EVENT_WINDOW_FIRST and <= SDL_EventType.SDL_EVENT_WINDOW_LAST =>
                (uint)evt.window.windowID,
            _ => 0
        };
    }
}
