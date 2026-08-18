namespace Pixely.Input;

public interface IGamepadService
{
    IReadOnlyCollection<Gamepad> Gamepads { get; }

    event InputEventHandler<GamepadStickEventArgs> LeftStickMotion;
    event InputEventHandler<GamepadStickEventArgs> RightStickMotion;
    event InputEventHandler<GamepadTriggerEventArgs> LeftTriggerMotion;
    event InputEventHandler<GamepadTriggerEventArgs> RightTriggerMotion;
    event InputEventHandler<GamepadButtonEventArgs> ButtonPress;
    event InputEventHandler<GamepadButtonEventArgs> ButtonRelease;
    event GamepadConnectionEventHandler? GamepadConnected;
    event GamepadConnectionEventHandler? GamepadDisconnected;

    void SubscribeLeftStickMotion(int priority, InputEventHandler<GamepadStickEventArgs> handler);
    void SubscribeRightStickMotion(int priority, InputEventHandler<GamepadStickEventArgs> handler);
    void SubscribeLeftTriggerMotion(int priority, InputEventHandler<GamepadTriggerEventArgs> handler);
    void SubscribeRightTriggerMotion(int priority, InputEventHandler<GamepadTriggerEventArgs> handler);
    void SubscribeButtonPress(int priority, InputEventHandler<GamepadButtonEventArgs> handler);
    void SubscribeButtonRelease(int priority, InputEventHandler<GamepadButtonEventArgs> handler);
}
