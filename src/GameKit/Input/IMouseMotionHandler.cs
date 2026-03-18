namespace GameKit.Input;

public interface IMouseMotionHandler
{
    int Order => 0;
    void OnMotion(Mouse mouse, MouseMotionInputEvent inputEvent);
}
