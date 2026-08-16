namespace GameKit.Input;

public interface ITextInputService
{
    bool IsActiveFor(Window window);
    void Start(Window window);
    void Stop(Window window);

    event TextInputHandler TextInput;
    event TextEditingHandler TextEditing;

    void SubscribeTextInput(int priority, TextInputHandler handler);
    void SubscribeTextEditing(int priority, TextEditingHandler handler);
}
