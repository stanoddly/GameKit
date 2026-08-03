using System.Numerics;

namespace GameKit.Input;

public readonly record struct MouseState(Vector2 Position, int ButtonFlags)
{
    public bool IsPressed(MouseButton button)
    {
        int mask = 1 << ((int)button - 1);
        return (ButtonFlags & mask) != 0;
    }
}
