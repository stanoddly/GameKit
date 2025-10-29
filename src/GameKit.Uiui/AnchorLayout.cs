using System.Collections;
using GameKit.Common;
using GameKit;

namespace GameKit.Uiui;

public enum HorizontalAnchor
{
    Left,
    Center,
    Right
}

public enum VerticalAnchor
{
    Top,
    Middle,
    Bottom
}

public class AnchorLayout : Widget, IEnumerable<Widget>
{
    private Widget? _child;

    public HorizontalAnchor HorizontalAnchor { get; }
    public VerticalAnchor VerticalAnchor { get; }

    public AnchorLayout(HorizontalAnchor horizontalAnchor = HorizontalAnchor.Center, 
                       VerticalAnchor verticalAnchor = VerticalAnchor.Middle)
    {
        HorizontalAnchor = horizontalAnchor;
        VerticalAnchor = verticalAnchor;
    }

    public void Add(Widget child)
    {
        if (_child != null)
            throw new InvalidOperationException("AnchorLayout can only contain one child.");

        _child = child;
        Invalid = true;
    }

    public void Remove()
    {
        if (_child == null)
            return;

        _child = null;
        Invalid = true;
    }

    public override bool Update(FrameContext context)
    {
        bool hasInvalidChild = Invalid;
        Invalid = false;
        
        if (_child != null && _child.Update(context))
        {
            hasInvalidChild = true;
        }
        
        return hasInvalidChild;
    }

    protected internal override ShortVector2 Measure(ShortVector2 availableSize)
    {
        if (_child == null)
            return new ShortVector2(0, 0);

        return _child.Measure(availableSize);
    }

    protected internal override void Arrange(GuiContext guiContext, ShortRectangle bounds)
    {
        base.Arrange(guiContext, bounds);

        if (_child == null)
        {
            return;
        }

        var childSize = _child.Measure(new ShortVector2(bounds.Width, bounds.Height));

        short x = HorizontalAnchor switch
        {
            HorizontalAnchor.Left => bounds.X,
            HorizontalAnchor.Center => (short)(bounds.X + (bounds.Width - childSize.X) / 2),
            HorizontalAnchor.Right => (short)(bounds.X + bounds.Width - childSize.X),
            _ => bounds.X
        };

        short y = VerticalAnchor switch
        {
            VerticalAnchor.Top => bounds.Y,
            VerticalAnchor.Middle => (short)(bounds.Y + (bounds.Height - childSize.Y) / 2),
            VerticalAnchor.Bottom => (short)(bounds.Y + bounds.Height - childSize.Y),
            _ => bounds.Y
        };

        _child.Arrange(guiContext, new ShortRectangle(x, y, childSize.X, childSize.Y));
    }

    public override void Render(GuiContext guiContext, GuiRenderer guiRenderer)
    {
        _child?.Render(guiContext, guiRenderer);
    }

    public IEnumerator<Widget> GetEnumerator()
    {
        if (_child != null)
        {
            yield return _child;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}