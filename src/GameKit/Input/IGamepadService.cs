namespace GameKit.Input;

public interface IGamepadService
{
    IReadOnlyCollection<Gamepad> Gamepads { get; }

    event InputEventHandler<Gamepad, GamepadStickEventArgs> LeftStickMotion;
    event InputEventHandler<Gamepad, GamepadStickEventArgs> RightStickMotion;
    event InputEventHandler<Gamepad, GamepadTriggerEventArgs> LeftTriggerMotion;
    event InputEventHandler<Gamepad, GamepadTriggerEventArgs> RightTriggerMotion;
    event InputEventHandler<Gamepad, GamepadButtonEventArgs> ButtonPress;
    event InputEventHandler<Gamepad, GamepadButtonEventArgs> ButtonRelease;
    event GamepadConnectionEventHandler? GamepadConnected;
    event GamepadConnectionEventHandler? GamepadDisconnected;

    void SubscribeLeftStickMotion(
        int priority,
        InputEventHandler<Gamepad, GamepadStickEventArgs> handler);
    void SubscribeRightStickMotion(
        int priority,
        InputEventHandler<Gamepad, GamepadStickEventArgs> handler);
    void SubscribeLeftTriggerMotion(
        int priority,
        InputEventHandler<Gamepad, GamepadTriggerEventArgs> handler);
    void SubscribeRightTriggerMotion(
        int priority,
        InputEventHandler<Gamepad, GamepadTriggerEventArgs> handler);
    void SubscribeButtonPress(
        int priority,
        InputEventHandler<Gamepad, GamepadButtonEventArgs> handler);
    void SubscribeButtonRelease(
        int priority,
        InputEventHandler<Gamepad, GamepadButtonEventArgs> handler);
}
