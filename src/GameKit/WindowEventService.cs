using GameKit.Input;
using SDL;

namespace GameKit;

internal sealed class WindowEventService : IDisposable
{
    private readonly KeyboardService _keyboard;
    private readonly MouseService _mouse;
    private readonly TextInputService _textInput;
    private readonly AppControl _appControl;
    private readonly EventService _eventService;
    private readonly WindowManager _windowManager;

    internal WindowEventService(
        Window window,
        KeyboardService keyboard,
        MouseService mouse,
        TextInputService textInput,
        WindowManager windowManager,
        AppControl appControl,
        EventService eventService)
    {
        Window = window;
        _keyboard = keyboard;
        _mouse = mouse;
        _textInput = textInput;
        _windowManager = windowManager;
        _appControl = appControl;
        _eventService = eventService;
    }

    internal Window Window { get; }

    internal static WindowEventService Create(
        Window window,
        KeyboardService keyboard,
        MouseService mouse,
        TextInputService textInput,
        WindowManager windowManager,
        AppControl appControl,
        EventService eventService)
    {
        WindowEventService windowEvents = new(
            window,
            keyboard,
            mouse,
            textInput,
            windowManager,
            appControl,
            eventService);
        eventService.Attach(windowEvents);
        return windowEvents;
    }

    internal void Process(in SDL_Event evt)
    {
        if (evt.Type == SDL_EventType.SDL_EVENT_KEY_DOWN ||
            evt.Type == SDL_EventType.SDL_EVENT_KEY_UP)
        {
            _keyboard.OnKeyEvent(evt.key);
        }
        else if (evt.Type == SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN ||
                 evt.Type == SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP)
        {
            _mouse.OnMouseButtonEvent(evt.button);
        }
        else if (evt.Type == SDL_EventType.SDL_EVENT_MOUSE_MOTION)
        {
            _mouse.OnMouseMotionEvent(evt.motion);
        }
        else if (evt.Type == SDL_EventType.SDL_EVENT_MOUSE_WHEEL)
        {
            _mouse.OnMouseWheelEvent(evt.wheel);
        }
        else if (evt.Type == SDL_EventType.SDL_EVENT_WINDOW_MOUSE_ENTER)
        {
            _mouse.OnMouseWindowPresenceEvent(evt.window, true);
        }
        else if (evt.Type == SDL_EventType.SDL_EVENT_WINDOW_MOUSE_LEAVE)
        {
            _mouse.OnMouseWindowPresenceEvent(evt.window, false);
        }
        else if (evt.Type == SDL_EventType.SDL_EVENT_TEXT_INPUT)
        {
            _textInput.OnTextInputEvent(evt.text);
        }
        else if (evt.Type == SDL_EventType.SDL_EVENT_TEXT_EDITING)
        {
            _textInput.OnTextEditingEvent(evt.edit);
        }
        else if (evt.Type == SDL_EventType.SDL_EVENT_WINDOW_PIXEL_SIZE_CHANGED)
        {
            Window.OnPixelSizeChanged(evt.window.timestamp);
        }
        else if (evt.Type == SDL_EventType.SDL_EVENT_WINDOW_CLOSE_REQUESTED)
        {
            if (Window.StopGameOnClose)
            {
                _appControl.Quit();
            }
            else
            {
                _windowManager.DestroyWindow(Window);
            }
        }
    }

    public void Dispose()
    {
        _eventService.Detach(this);
    }
}
