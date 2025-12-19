namespace GameKit.Input;

public interface IKeyboardService
{
    event KeyDownEventHandler? KeyDown;
    event KeyUpEventHandler? KeyUp;
}
