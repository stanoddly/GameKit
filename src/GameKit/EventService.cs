using GameKit.Input;
using SDL;

namespace GameKit;

public sealed class EventService
{
    private readonly GamepadService _gamepadService;
    private readonly AppControl _appControl;
    private (uint WindowId, WindowEventService Service)[] _windowEventServices = [];

    internal EventService(
        GamepadService gamepadService,
        AppControl appControl)
    {
        _gamepadService = gamepadService;
        _appControl = appControl;
    }

    public void Process()
    {
        unsafe
        {
            SDL_Event evt;
            while (SDL3.SDL_PollEvent(&evt))
            {
                Process(&evt);
            }
        }
    }

    private unsafe void Process(SDL_Event* eventPointer)
    {
        ref SDL_Event evt = ref *eventPointer;

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
            SDL_Window* sdlWindow = SDL3.SDL_GetWindowFromEvent(eventPointer);
            if (sdlWindow != null &&
                TryGetWindowEventService(
                    (uint)SDL3.SDL_GetWindowID(sdlWindow),
                    out WindowEventService windowEvents))
            {
                windowEvents.Process(in evt);
            }
        }
    }

    internal void Attach(WindowEventService windowEvents)
    {
        uint windowId = windowEvents.Window.RequireActivation().Id;
        for (int i = 0; i < _windowEventServices.Length; i++)
        {
            if (_windowEventServices[i].WindowId == windowId)
            {
                throw new InvalidOperationException($"SDL window ID {windowId} is already attached.");
            }
        }

        _windowEventServices = [.. _windowEventServices, (windowId, windowEvents)];
    }

    internal void Detach(WindowEventService windowEvents)
    {
        int index = -1;
        for (int i = 0; i < _windowEventServices.Length; i++)
        {
            if (ReferenceEquals(_windowEventServices[i].Service, windowEvents))
            {
                index = i;
                break;
            }
        }

        if (index < 0)
        {
            return;
        }

        int itemsToMove = _windowEventServices.Length - index - 1;
        if (itemsToMove > 0)
        {
            Array.Copy(_windowEventServices, index + 1, _windowEventServices, index, itemsToMove);
        }

        Array.Resize(ref _windowEventServices, _windowEventServices.Length - 1);
    }

    internal bool TryGetWindowEventService(uint windowId, out WindowEventService windowEvents)
    {
        for (int i = 0; i < _windowEventServices.Length; i++)
        {
            if (_windowEventServices[i].WindowId == windowId)
            {
                windowEvents = _windowEventServices[i].Service;
                return true;
            }
        }

        windowEvents = null!;
        return false;
    }
}
