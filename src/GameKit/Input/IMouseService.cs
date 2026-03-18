namespace GameKit.Input;

public interface IMouseService
{
    event MouseButtonPressedHandler ButtonPress;
    event MouseButtonReleasedHandler ButtonRelease;
    event MouseMotionHandler Motion;

    void SubscribeButtonPress(int priority, MouseButtonPressedHandler handler);
    void SubscribeButtonRelease(int priority, MouseButtonReleasedHandler handler);
    void SubscribeMotion(int priority, MouseMotionHandler handler);
}
