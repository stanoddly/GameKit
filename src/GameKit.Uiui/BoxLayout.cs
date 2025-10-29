using GameKit.Common;

namespace GameKit.Uiui;

public class BoxLayout<TWidget> : ContainerWidget<TWidget> where TWidget : Widget
{
    public Orientation Orientation { get; }
    public int Spacing { get; }

    public BoxLayout(Orientation orientation = Orientation.Vertical, int spacing = 0)
    {
        Orientation = orientation;
        Spacing = spacing;
    }

    protected internal override ShortVector2 Measure(ShortVector2 availableSize)
    {
        if (Children.Count == 0)
        {
            return new ShortVector2(0, 0);
        }

        short totalWidth = 0;
        short totalHeight = 0;
        short maxWidth = 0;
        short maxHeight = 0;

        foreach (var child in Children)
        {
            ShortVector2 childSize = child.Measure(availableSize);

            if (Orientation == Orientation.Horizontal)
            {
                totalWidth += childSize.X;
                maxHeight = Math.Max(maxHeight, childSize.Y);
            }
            else
            {
                totalHeight += childSize.Y;
                maxWidth = Math.Max(maxWidth, childSize.X);
            }
        }

        if (Children.Count > 1)
        {
            int spacingCount = Children.Count - 1;
            if (Orientation == Orientation.Horizontal)
                totalWidth += (short)(Spacing * spacingCount);
            else
                totalHeight += (short)(Spacing * spacingCount);
        }

        return new ShortVector2(
            Orientation == Orientation.Horizontal ? totalWidth : maxWidth,
            Orientation == Orientation.Horizontal ? maxHeight : totalHeight
        );
    }

    protected internal override void Arrange(GuiContext guiContext, ShortRectangle bounds)
    {
        base.Arrange(guiContext, bounds);

        if (Children.Count == 0)
        {
            return;
        }

        short currentX = Bounds.X;
        short currentY = Bounds.Y;

        foreach (var child in Children)
        {
            ShortVector2 childSize = child.Measure(new ShortVector2(Bounds.Width, Bounds.Height));

            ShortRectangle childRect;
            if (Orientation == Orientation.Horizontal)
            {
                childRect = new ShortRectangle(
                    currentX,
                    Bounds.Y,
                    childSize.X,
                    Bounds.Height
                );
                currentX += (short)(childSize.X + Spacing);
            }
            else
            {
                childRect = new ShortRectangle(
                    Bounds.X,
                    currentY,
                    Bounds.Width,
                    childSize.Y
                );
                currentY += (short)(childSize.Y + Spacing);
            }

            child.Arrange(guiContext, childRect);
        }
    }
}

public class BoxLayout : BoxLayout<Widget>;
