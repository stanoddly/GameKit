namespace GameKit.Input;

public interface IMouseService
{
    event MouseButtonPressedHandler? ButtonPress;
    event MouseButtonReleasedHandler? ButtonRelease;
    event MouseMotionHandler? Motion;
}
