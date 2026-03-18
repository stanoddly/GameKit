namespace GameKit.Input;

public interface IMouseButtonPressHandler
{
    int Order => 0;
    void OnButtonPress(Mouse mouse, MouseButtonInputEvent inputEvent);
}
