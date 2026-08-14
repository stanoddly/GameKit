namespace GameKit.Input;

public interface ITextInputService
{
    bool IsActive { get; }

    void Start();
    void Stop();

    bool IsActiveFor(Window window);
    void Start(Window window);
    void Stop(Window window);

    event TextInputHandler TextInput;
    event TextEditingHandler TextEditing;

    void SubscribeTextInput(int priority, TextInputHandler handler);
    void SubscribeTextEditing(int priority, TextEditingHandler handler);
}

public interface ITextInputService<TWindow> : ITextInputService
    where TWindow : class
{
}
