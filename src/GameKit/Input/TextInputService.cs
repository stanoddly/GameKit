using System.Runtime.InteropServices;
using SDL;

namespace GameKit.Input;

public class TextInputEventArgs
{
    public string Text { get; internal set; } = string.Empty;
    public ulong Timestamp { get; internal set; }
    public bool Consumed { get; internal set; }
    public void Consume() { Consumed = true; }
}

public class TextEditingEventArgs
{
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
    private readonly ViewScopedPriorityEventHandlers<TextInputHandler> _textInputHandlers = new();
    private readonly ViewScopedPriorityEventHandlers<TextEditingHandler> _textEditingHandlers = new();

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
        add => _textInputHandlers.Add(default, 0, value);
        remove => _textInputHandlers.Remove(default, value);
    }

    public event TextEditingHandler TextEditing
    {
        add => _textEditingHandlers.Add(default, 0, value);
        remove => _textEditingHandlers.Remove(default, value);
    }

    public void SubscribeTextInput(int priority, TextInputHandler handler)
    {
        _textInputHandlers.Add(default, priority, handler);
    }

    public void SubscribeTextEditing(int priority, TextEditingHandler handler)
    {
        _textEditingHandlers.Add(default, priority, handler);
    }

    public void SubscribeTextInput(
        ViewScope viewScope,
        int priority,
        TextInputHandler handler)
    {
        _textInputHandlers.Add(viewScope, priority, handler);
    }

    public void SubscribeTextEditing(
        ViewScope viewScope,
        int priority,
        TextEditingHandler handler)
    {
        _textEditingHandlers.Add(viewScope, priority, handler);
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

        _textInputEventArgs.Text = text;
        _textInputEventArgs.Timestamp = textInputEvent.timestamp;
        _textInputEventArgs.Consumed = false;

        foreach ((_, TextInputHandler handler) in _textInputHandlers.GetSorted(viewScope))
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

        _textEditingEventArgs.Text = text;
        _textEditingEventArgs.Start = textEditingEvent.start;
        _textEditingEventArgs.Length = textEditingEvent.length;
        _textEditingEventArgs.Timestamp = textEditingEvent.timestamp;
        _textEditingEventArgs.Consumed = false;

        foreach ((_, TextEditingHandler handler) in _textEditingHandlers.GetSorted(viewScope))
        {
            handler(_textEditingEventArgs);

            if (_textEditingEventArgs.Consumed)
            {
                break;
            }
        }
    }
}
