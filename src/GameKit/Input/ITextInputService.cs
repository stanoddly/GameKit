namespace GameKit.Input;

public interface ITextInputService
{
    bool IsActive { get; }

    void Start();
    void Stop();

    event TextInputHandler TextInput;
    event TextEditingHandler TextEditing;

    void SubscribeTextInput(int priority, TextInputHandler handler);
    void SubscribeTextEditing(int priority, TextEditingHandler handler);
}
