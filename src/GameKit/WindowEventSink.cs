using GameKit.Input;
using SDL;

namespace GameKit;

internal interface IWindowEventSink
{
    void Process(in SDL_Event evt);
}

internal sealed class WindowEventSink<TWindow> : IWindowEventSink, IDisposable
    where TWindow : class
{
    private readonly KeyboardService<TWindow> _keyboardService;
    private readonly MouseService<TWindow> _mouseService;
    private readonly TextInputService<TWindow> _textInputService;
    private readonly Window<TWindow> _window;

    public WindowEventSink(
        Window<TWindow> window,
        KeyboardService<TWindow> keyboardService,
        MouseService<TWindow> mouseService,
        TextInputService<TWindow> textInputService)
    {
        _window = window;
        _keyboardService = keyboardService;
        _mouseService = mouseService;
        _textInputService = textInputService;
        window.AttachEventSink(this);
    }

    public void Process(in SDL_Event evt)
    {
        if (evt.Type == SDL_EventType.SDL_EVENT_KEY_DOWN ||
            evt.Type == SDL_EventType.SDL_EVENT_KEY_UP)
        {
            _keyboardService.OnKeyEvent(evt.key);
        }
        else if (evt.Type == SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN ||
                 evt.Type == SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP)
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
            _mouseService.OnMouseWindowPresenceEvent(evt.window, true);
        }
        else if (evt.Type == SDL_EventType.SDL_EVENT_WINDOW_MOUSE_LEAVE)
        {
            _mouseService.OnMouseWindowPresenceEvent(evt.window, false);
        }
        else if (evt.Type == SDL_EventType.SDL_EVENT_TEXT_INPUT)
        {
            _textInputService.OnTextInputEvent(evt.text);
        }
        else if (evt.Type == SDL_EventType.SDL_EVENT_TEXT_EDITING)
        {
            _textInputService.OnTextEditingEvent(evt.edit);
        }
    }

    public void Dispose()
    {
        _window.DetachEventSink(this);
    }
}
