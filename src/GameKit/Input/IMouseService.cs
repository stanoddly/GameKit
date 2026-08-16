namespace GameKit.Input;

public interface IMouseService
{
    event MouseButtonPressedHandler ButtonPress;
    event MouseButtonReleasedHandler ButtonRelease;
    event MouseMotionHandler Motion;
    event MouseWheelHandler Wheel;
    event MouseWindowPresenceHandler WindowEnter;
    event MouseWindowPresenceHandler WindowLeave;

    bool IsInWindow(ViewScope viewScope);

    MouseState GetGlobalState();

    void SubscribeButtonPress(int priority, MouseButtonPressedHandler handler);
    void SubscribeButtonRelease(int priority, MouseButtonReleasedHandler handler);
    void SubscribeMotion(int priority, MouseMotionHandler handler);
    void SubscribeWheel(int priority, MouseWheelHandler handler);
    void SubscribeWindowEnter(int priority, MouseWindowPresenceHandler handler);
    void SubscribeWindowLeave(int priority, MouseWindowPresenceHandler handler);
    void SubscribeButtonPress(ViewScope viewScope, int priority, MouseButtonPressedHandler handler);
    void SubscribeButtonRelease(ViewScope viewScope, int priority, MouseButtonReleasedHandler handler);
    void SubscribeMotion(ViewScope viewScope, int priority, MouseMotionHandler handler);
    void SubscribeWheel(ViewScope viewScope, int priority, MouseWheelHandler handler);
    void SubscribeWindowEnter(ViewScope viewScope, int priority, MouseWindowPresenceHandler handler);
    void SubscribeWindowLeave(ViewScope viewScope, int priority, MouseWindowPresenceHandler handler);
}
