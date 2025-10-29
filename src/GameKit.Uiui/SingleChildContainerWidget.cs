using System.Collections;
using System.Numerics;
using GameKit.Input;
using GameKit;

namespace GameKit.Uiui;

public abstract class SingleChildContainerWidget: Widget, IEnumerable<Widget>
{
    protected Widget? Child { get; private set; }

    public void Add(Widget child)
    {
        if (Child != null)
            throw new InvalidOperationException("AnchorLayout can only contain one child.");

        Child = child;
        Invalid = true;
    }

    public void Remove()
    {
        if (Child == null)
            return;

        Child = null;
        Invalid = true;
    }

    public override bool Update(FrameContext context)
    {
        bool hasInvalidChild = Invalid;
        Invalid = false;
        
        if (Child != null && Child.Update(context))
        {
            hasInvalidChild = true;
        }
        
        return hasInvalidChild;
    }

    public override void Render(GuiContext guiContext, GuiRenderer guiRenderer)
    {
        Child?.Render(guiContext, guiRenderer);
    }

    public override bool OnKeyDown(Keyboard keyboard, KeyEventArgs keyEventArgs)
    {
        return Child?.OnKeyDown(keyboard, keyEventArgs) ?? false;
    }

    public override bool OnKeyUp(Keyboard keyboard, KeyEventArgs keyEventArgs)
    {
        return Child?.OnKeyUp(keyboard, keyEventArgs) ?? false;
    }

    public override bool OnGamepadButtonPress(Gamepad gamepad, GamepadButton button)
    {
        return Child?.OnGamepadButtonPress(gamepad, button) ?? false;
    }

    public override bool OnGamepadButtonRelease(Gamepad gamepad, GamepadButton button)
    {
        return Child?.OnGamepadButtonRelease(gamepad, button) ?? false;
    }

    public override bool OnGamepadStickMotion(Gamepad gamepad, Vector2 motion)
    {
        return Child?.OnGamepadStickMotion(gamepad, motion) ?? false;
    }

    public IEnumerator<Widget> GetEnumerator()
    {
        if (Child != null)
        {
            yield return Child;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
