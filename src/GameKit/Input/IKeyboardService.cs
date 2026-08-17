namespace GameKit.Input;

public interface IKeyboardService
{
    event InputEventHandler<KeyEventArgs> KeyDown;
    event InputEventHandler<KeyEventArgs> KeyUp;

    void SubscribeKeyDown(int priority, InputEventHandler<KeyEventArgs> handler);
    void SubscribeKeyUp(int priority, InputEventHandler<KeyEventArgs> handler);
    void SubscribeKeyDown(ViewScope viewScope, int priority, InputEventHandler<KeyEventArgs> handler);
    void SubscribeKeyUp(ViewScope viewScope, int priority, InputEventHandler<KeyEventArgs> handler);
}
