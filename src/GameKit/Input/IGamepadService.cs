namespace GameKit.Input;

public interface IGamepadService
{
    event GamepadMotionEventHandler? LeftStickMotion;
    event GamepadMotionEventHandler? RightStickMotion;
    event GamepadTriggerEventHandler? LeftTriggerMotion;
    event GamepadTriggerEventHandler? RightTriggerMotion;
    event GamepadButtonPressedHandler? ButtonPress;
    event GamepadButtonReleasedHandler? ButtonRelease;
    event GamepadConnectionEventHandler? GamepadConnected;
    event GamepadConnectionEventHandler? GamepadDisconnected;
}