using System.Collections;
using System.Numerics;
using GameKit.Input;
using GameKit;

namespace GameKit.Uiui;

public abstract class ContainerWidget<TWidget> : Widget, IEnumerable<TWidget> where TWidget : Widget
{
    protected List<TWidget> Children { get; } = new();

    public TWidget this[int index] => Children[index];

    public override bool Update(FrameContext context)
    {
        bool hasInvalidChild = Invalid;
        Invalid = false;
        
        for (int i = 0; i < Children.Count; i++)
        {
            if (Children[i].Update(context))
            {
                hasInvalidChild = true;
            }
        }

        return hasInvalidChild;
    }

    public virtual void Add(TWidget child)
    {
        Children.Add(child);
        Invalid = true;
    }

    public virtual bool Remove(TWidget child)
    {
        bool removed = Children.Remove(child);
        if (removed)
        {
            Invalid = true;
        }

        return removed;
    }

    public virtual void RemoveAll()
    {
        Children.Clear();
        Invalid = true;
    }

    public override void Render(GuiContext guiContext, GuiRenderer guiRenderer)
    {
        foreach (var child in Children)
        {
            child.Render(guiContext, guiRenderer);
        }
    }

    public override bool OnKeyDown(Keyboard keyboard, KeyEventArgs keyEventArgs)
    {
        foreach (var child in Children)
        {
            if (child.OnKeyDown(keyboard, keyEventArgs))
            {
                return true;
            }
        }
        return false;
    }

    public override bool OnKeyUp(Keyboard keyboard, KeyEventArgs keyEventArgs)
    {
        foreach (var child in Children)
        {
            if (child.OnKeyUp(keyboard, keyEventArgs))
            {
                return true;
            }
        }
        return false;
    }

    public override bool OnGamepadButtonPress(Gamepad gamepad, GamepadButton button)
    {
        foreach (var child in Children)
        {
            if (child.OnGamepadButtonPress(gamepad, button))
            {
                return true;
            }
        }
        return false;
    }

    public override bool OnGamepadButtonRelease(Gamepad gamepad, GamepadButton button)
    {
        foreach (var child in Children)
        {
            if (child.OnGamepadButtonRelease(gamepad, button))
            {
                return true;
            }
        }
        return false;
    }

    public override bool OnGamepadStickMotion(Gamepad gamepad, Vector2 motion)
    {
        foreach (var child in Children)
        {
            if (child.OnGamepadStickMotion(gamepad, motion))
            {
                return true;
            }
        }
        return false;
    }

    public IEnumerator<TWidget> GetEnumerator()
    {
        return Children.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Children).GetEnumerator();
    }
}

public abstract class ContainerWidget : ContainerWidget<Widget>
{
}

