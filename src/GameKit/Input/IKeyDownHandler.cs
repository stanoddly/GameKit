namespace GameKit.Input;

public interface IKeyDownHandler
{
    int Order => 0;
    void OnKeyDown(Keyboard keyboard, KeyInputEvent inputEvent);
}
