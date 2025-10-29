using GameKit.Common;
using GameKit.Input;
using GameKit;
using System.Numerics;

namespace GameKit.Uiui;

public abstract class Widget
{
    public ShortRectangle Bounds { get; protected set; }

    protected bool Invalid { get; set; } = true;

    public virtual bool Update(FrameContext context)
    {
        bool wasInvalid = Invalid;
        Invalid = false;
        return wasInvalid;
    }

    protected internal abstract ShortVector2 Measure(ShortVector2 availableSize);

    protected internal virtual void Arrange(GuiContext guiContext, ShortRectangle bounds)
    {
        Invalid = false;
        Bounds = bounds;
    }

    public abstract void Render(GuiContext guiContext, GuiRenderer guiRenderer);

    public virtual bool OnKeyDown(Keyboard keyboard, KeyEventArgs keyEventArgs)
    {
        return false;
    }

    public virtual bool OnKeyUp(Keyboard keyboard, KeyEventArgs keyEventArgs)
    {
        return false;
    }

    public virtual bool OnGamepadButtonPress(Gamepad gamepad, GamepadButton button)
    {
        return false;
    }

    public virtual bool OnGamepadButtonRelease(Gamepad gamepad, GamepadButton button)
    {
        return false;
    }

    public virtual bool OnGamepadStickMotion(Gamepad gamepad, Vector2 motion)
    {
        return false;
    }
}

public enum Orientation
{
    Horizontal,
    Vertical
}
