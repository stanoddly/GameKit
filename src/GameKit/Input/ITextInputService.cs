namespace GameKit.Input;

public interface ITextInputService
{
    bool IsActiveFor(ViewScope viewScope = default);
    void Start(ViewScope viewScope = default);
    void Stop(ViewScope viewScope = default);

    event InputEventHandler<ITextInputService, TextInputEventArgs> TextInput;
    event InputEventHandler<ITextInputService, TextEditingEventArgs> TextEditing;

    void SubscribeTextInput(int priority, InputEventHandler<ITextInputService, TextInputEventArgs> handler);
    void SubscribeTextEditing(int priority, InputEventHandler<ITextInputService, TextEditingEventArgs> handler);
    void SubscribeTextInput(ViewScope viewScope, int priority, InputEventHandler<ITextInputService, TextInputEventArgs> handler);
    void SubscribeTextEditing(ViewScope viewScope, int priority, InputEventHandler<ITextInputService, TextEditingEventArgs> handler);
}
