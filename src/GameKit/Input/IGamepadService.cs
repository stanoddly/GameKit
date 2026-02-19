namespace GameKit.Input;

public interface IGamepadService
{
    IReadOnlyCollection<Gamepad> Gamepads { get; }

    event GamepadMotionEventHandler? LeftStickMotion;
    event GamepadMotionEventHandler? RightStickMotion;
    event GamepadTriggerEventHandler? LeftTriggerMotion;
    event GamepadTriggerEventHandler? RightTriggerMotion;
    event GamepadButtonPressedHandler? ButtonPress;
    event GamepadButtonReleasedHandler? ButtonRelease;
    event GamepadConnectionEventHandler? GamepadConnected;
    event GamepadConnectionEventHandler? GamepadDisconnected;
}