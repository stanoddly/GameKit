namespace GameKit.Input;

public interface IKeyboardService
{
    event KeyDownEventHandler KeyDown;
    event KeyUpEventHandler KeyUp;

    void SubscribeKeyDown(int priority, KeyDownEventHandler handler);
    void SubscribeKeyUp(int priority, KeyUpEventHandler handler);
    void SubscribeKeyDown(ViewScope viewScope, int priority, KeyDownEventHandler handler);
    void SubscribeKeyUp(ViewScope viewScope, int priority, KeyUpEventHandler handler);
}
