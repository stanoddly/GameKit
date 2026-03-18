namespace GameKit.Input;

public interface IKeyUpHandler
{
    int Order => 0;
    void OnKeyUp(Keyboard keyboard, KeyInputEvent inputEvent);
}
