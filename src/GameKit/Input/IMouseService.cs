namespace GameKit.Input;

public interface IMouseService
{
    event InputEventHandler<Mouse, MouseButtonEventArgs> ButtonPress;
    event InputEventHandler<Mouse, MouseButtonEventArgs> ButtonRelease;
    event InputEventHandler<Mouse, MouseMotionEventArgs> Motion;
    event InputEventHandler<Mouse, MouseWheelEventArgs> Wheel;
    event InputEventHandler<IMouseService, MouseWindowPresenceEventArgs> WindowEnter;
    event InputEventHandler<IMouseService, MouseWindowPresenceEventArgs> WindowLeave;

    bool IsInWindow(ViewScope viewScope = default);

    MouseState GetGlobalState();

    void SubscribeButtonPress(int priority, InputEventHandler<Mouse, MouseButtonEventArgs> handler);
    void SubscribeButtonRelease(int priority, InputEventHandler<Mouse, MouseButtonEventArgs> handler);
    void SubscribeMotion(int priority, InputEventHandler<Mouse, MouseMotionEventArgs> handler);
    void SubscribeWheel(int priority, InputEventHandler<Mouse, MouseWheelEventArgs> handler);
    void SubscribeWindowEnter(
        int priority,
        InputEventHandler<IMouseService, MouseWindowPresenceEventArgs> handler);
    void SubscribeWindowLeave(
        int priority,
        InputEventHandler<IMouseService, MouseWindowPresenceEventArgs> handler);
    void SubscribeButtonPress(
        ViewScope viewScope,
        int priority,
        InputEventHandler<Mouse, MouseButtonEventArgs> handler);
    void SubscribeButtonRelease(
        ViewScope viewScope,
        int priority,
        InputEventHandler<Mouse, MouseButtonEventArgs> handler);
    void SubscribeMotion(
        ViewScope viewScope,
        int priority,
        InputEventHandler<Mouse, MouseMotionEventArgs> handler);
    void SubscribeWheel(
        ViewScope viewScope,
        int priority,
        InputEventHandler<Mouse, MouseWheelEventArgs> handler);
    void SubscribeWindowEnter(
        ViewScope viewScope,
        int priority,
        InputEventHandler<IMouseService, MouseWindowPresenceEventArgs> handler);
    void SubscribeWindowLeave(
        ViewScope viewScope,
        int priority,
        InputEventHandler<IMouseService, MouseWindowPresenceEventArgs> handler);
}
