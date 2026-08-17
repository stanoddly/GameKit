namespace GameKit.Input;

public interface IKeyboardService
{
    event InputEventHandler<Keyboard, KeyEventArgs> KeyDown;
    event InputEventHandler<Keyboard, KeyEventArgs> KeyUp;

    void SubscribeKeyDown(int priority, InputEventHandler<Keyboard, KeyEventArgs> handler);
    void SubscribeKeyUp(int priority, InputEventHandler<Keyboard, KeyEventArgs> handler);
    void SubscribeKeyDown(
        ViewScope viewScope,
        int priority,
        InputEventHandler<Keyboard, KeyEventArgs> handler);
    void SubscribeKeyUp(
        ViewScope viewScope,
        int priority,
        InputEventHandler<Keyboard, KeyEventArgs> handler);
}
