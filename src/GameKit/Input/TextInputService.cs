using System.Runtime.InteropServices;
using SDL;

namespace GameKit.Input;

public class TextInputEventArgs
{
    public ViewScope ViewScope { get; internal set; }
    public string Text { get; internal set; } = string.Empty;
    public ulong Timestamp { get; internal set; }
    public bool Consumed { get; internal set; }
    public void Consume() { Consumed = true; }
}

public class TextEditingEventArgs
{
    public ViewScope ViewScope { get; internal set; }
    public string Text { get; internal set; } = string.Empty;
    public int Start { get; internal set; }
    public int Length { get; internal set; }
    public ulong Timestamp { get; internal set; }
    public bool Consumed { get; internal set; }
    public void Consume() { Consumed = true; }
}

public delegate void TextInputHandler(TextInputEventArgs eventArgs);
public delegate void TextEditingHandler(TextEditingEventArgs eventArgs);

public class TextInputService : ITextInputService
{
    private readonly WindowRegistry _windowRegistry;

    private readonly TextInputEventArgs _textInputEventArgs = new();
    private readonly TextEditingEventArgs _textEditingEventArgs = new();
    private readonly PriorityEventHandlers<TextInputHandler> _textInputHandlers = new();
    private readonly PriorityEventHandlers<TextEditingHandler> _textEditingHandlers = new();

    public bool IsActiveFor(ViewScope viewScope = default)
    {
        Window window = _windowRegistry.GetWindow(viewScope);
        unsafe
        {
            return SDL3.SDL_TextInputActive(window.SdlWindow);
        }
    }

    public event TextInputHandler TextInput
    {
        add => _textInputHandlers.Add(0, value);
        remove => _textInputHandlers.Remove(value);
    }

    public event TextEditingHandler TextEditing
    {
        add => _textEditingHandlers.Add(0, value);
        remove => _textEditingHandlers.Remove(value);
    }

    public void SubscribeTextInput(int priority, TextInputHandler handler)
    {
        _textInputHandlers.Add(priority, handler);
    }

    public void SubscribeTextEditing(int priority, TextEditingHandler handler)
    {
        _textEditingHandlers.Add(priority, handler);
    }

    public void SubscribeTextInput(
        ViewScope viewScope,
        int priority,
        TextInputHandler handler)
    {
        _textInputHandlers.Add(priority, eventArgs =>
        {
            if (eventArgs.ViewScope == viewScope)
            {
                handler(eventArgs);
            }
        });
    }

    public void SubscribeTextEditing(
        ViewScope viewScope,
        int priority,
        TextEditingHandler handler)
    {
        _textEditingHandlers.Add(priority, eventArgs =>
        {
            if (eventArgs.ViewScope == viewScope)
            {
                handler(eventArgs);
            }
        });
    }

    public void Start(ViewScope viewScope = default)
    {
        Window window = _windowRegistry.GetWindow(viewScope);
        unsafe
        {
            SDL3.SDL_StartTextInput(window.SdlWindow);
        }
    }

    public void Stop(ViewScope viewScope = default)
    {
        Window window = _windowRegistry.GetWindow(viewScope);
        unsafe
        {
            SDL3.SDL_StopTextInput(window.SdlWindow);
        }
    }

    internal TextInputService(WindowRegistry windowRegistry)
    {
        _windowRegistry = windowRegistry;
    }

    internal void OnTextInputEvent(
        ViewScope viewScope,
        in SDL_TextInputEvent textInputEvent)
    {
        string text;
        unsafe
        {
            text = Marshal.PtrToStringUTF8((IntPtr)textInputEvent.text) ?? string.Empty;
        }

        _textInputEventArgs.ViewScope = viewScope;
        _textInputEventArgs.Text = text;
        _textInputEventArgs.Timestamp = textInputEvent.timestamp;
        _textInputEventArgs.Consumed = false;

        foreach ((_, TextInputHandler handler) in _textInputHandlers.GetSorted())
        {
            handler(_textInputEventArgs);

            if (_textInputEventArgs.Consumed)
            {
                break;
            }
        }
    }

    internal void OnTextEditingEvent(
        ViewScope viewScope,
        in SDL_TextEditingEvent textEditingEvent)
    {
        string text;
        unsafe
        {
            text = Marshal.PtrToStringUTF8((IntPtr)textEditingEvent.text) ?? string.Empty;
        }

        _textEditingEventArgs.ViewScope = viewScope;
        _textEditingEventArgs.Text = text;
        _textEditingEventArgs.Start = textEditingEvent.start;
        _textEditingEventArgs.Length = textEditingEvent.length;
        _textEditingEventArgs.Timestamp = textEditingEvent.timestamp;
        _textEditingEventArgs.Consumed = false;

        foreach ((_, TextEditingHandler handler) in _textEditingHandlers.GetSorted())
        {
            handler(_textEditingEventArgs);

            if (_textEditingEventArgs.Consumed)
            {
                break;
            }
        }
    }
}
