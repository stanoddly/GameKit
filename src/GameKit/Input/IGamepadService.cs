namespace GameKit.Input;

public interface IGamepadService
{
    IReadOnlyCollection<Gamepad> Gamepads { get; }

    event GamepadMotionEventHandler LeftStickMotion;
    event GamepadMotionEventHandler RightStickMotion;
    event GamepadTriggerEventHandler LeftTriggerMotion;
    event GamepadTriggerEventHandler RightTriggerMotion;
    event GamepadButtonPressedHandler ButtonPress;
    event GamepadButtonReleasedHandler ButtonRelease;
    event GamepadConnectionEventHandler? GamepadConnected;
    event GamepadConnectionEventHandler? GamepadDisconnected;

    void SubscribeLeftStickMotion(int priority, GamepadMotionEventHandler handler);
    void SubscribeRightStickMotion(int priority, GamepadMotionEventHandler handler);
    void SubscribeLeftTriggerMotion(int priority, GamepadTriggerEventHandler handler);
    void SubscribeRightTriggerMotion(int priority, GamepadTriggerEventHandler handler);
    void SubscribeButtonPress(int priority, GamepadButtonPressedHandler handler);
    void SubscribeButtonRelease(int priority, GamepadButtonReleasedHandler handler);
}
