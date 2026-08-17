namespace GameKit.Input;

public interface ITextInputService
{
    bool IsActiveFor(ViewScope viewScope = default);
    void Start(ViewScope viewScope = default);
    void Stop(ViewScope viewScope = default);

    event InputEventHandler<TextInputEventArgs> TextInput;
    event InputEventHandler<TextEditingEventArgs> TextEditing;

    void SubscribeTextInput(int priority, InputEventHandler<TextInputEventArgs> handler);
    void SubscribeTextEditing(int priority, InputEventHandler<TextEditingEventArgs> handler);
    void SubscribeTextInput(ViewScope viewScope, int priority, InputEventHandler<TextInputEventArgs> handler);
    void SubscribeTextEditing(ViewScope viewScope, int priority, InputEventHandler<TextEditingEventArgs> handler);
}
