namespace GameKit.Input;

public interface IKeyboardService
{
    event KeyDownEventHandler KeyDown;
    event KeyUpEventHandler KeyUp;

    void SubscribeKeyDown(int priority, KeyDownEventHandler handler);
    void SubscribeKeyUp(int priority, KeyUpEventHandler handler);
}

public interface IKeyboardService<TWindow> : IKeyboardService
    where TWindow : Window
{
}
