namespace GameKit.Input;

public interface ITextInputService
{
    bool IsActiveFor(ViewScope viewScope);
    void Start(ViewScope viewScope);
    void Stop(ViewScope viewScope);

    event TextInputHandler TextInput;
    event TextEditingHandler TextEditing;

    void SubscribeTextInput(int priority, TextInputHandler handler);
    void SubscribeTextEditing(int priority, TextEditingHandler handler);
    void SubscribeTextInput(ViewScope viewScope, int priority, TextInputHandler handler);
    void SubscribeTextEditing(ViewScope viewScope, int priority, TextEditingHandler handler);
}
