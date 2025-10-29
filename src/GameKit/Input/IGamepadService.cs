namespace GameKit.Input;

public interface IGamepadService
{
    event GamepadMotionEventHandler? LeftStickMotion;
    event GamepadMotionEventHandler? RightStickMotion;
    event GamepadButtonPressedHandler? ButtonPress;
    event GamepadButtonReleasedHandler? ButtonRelease;
}