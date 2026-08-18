namespace Pixely.Input;

public interface IMouseService
{
    event InputEventHandler<MouseButtonEventArgs> ButtonPress;
    event InputEventHandler<MouseButtonEventArgs> ButtonRelease;
    event InputEventHandler<MouseMotionEventArgs> Motion;
    event InputEventHandler<MouseWheelEventArgs> Wheel;
    event InputEventHandler<MouseWindowPresenceEventArgs> WindowEnter;
    event InputEventHandler<MouseWindowPresenceEventArgs> WindowLeave;

    bool IsInWindow(ViewScope viewScope = default);

    MouseState GetGlobalState();

    void SubscribeButtonPress(int priority, InputEventHandler<MouseButtonEventArgs> handler);
    void SubscribeButtonRelease(int priority, InputEventHandler<MouseButtonEventArgs> handler);
    void SubscribeMotion(int priority, InputEventHandler<MouseMotionEventArgs> handler);
    void SubscribeWheel(int priority, InputEventHandler<MouseWheelEventArgs> handler);
    void SubscribeWindowEnter(int priority, InputEventHandler<MouseWindowPresenceEventArgs> handler);
    void SubscribeWindowLeave(int priority, InputEventHandler<MouseWindowPresenceEventArgs> handler);
    void SubscribeButtonPress(ViewScope viewScope, int priority, InputEventHandler<MouseButtonEventArgs> handler);
    void SubscribeButtonRelease(ViewScope viewScope, int priority, InputEventHandler<MouseButtonEventArgs> handler);
    void SubscribeMotion(ViewScope viewScope, int priority, InputEventHandler<MouseMotionEventArgs> handler);
    void SubscribeWheel(ViewScope viewScope, int priority, InputEventHandler<MouseWheelEventArgs> handler);
    void SubscribeWindowEnter(ViewScope viewScope, int priority, InputEventHandler<MouseWindowPresenceEventArgs> handler);
    void SubscribeWindowLeave(ViewScope viewScope, int priority, InputEventHandler<MouseWindowPresenceEventArgs> handler);
}
