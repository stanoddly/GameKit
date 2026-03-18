namespace GameKit.Input;

public interface IMouseButtonReleaseHandler
{
    int Order => 0;
    void OnButtonRelease(Mouse mouse, MouseButtonInputEvent inputEvent);
}
