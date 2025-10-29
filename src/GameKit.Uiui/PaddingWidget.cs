using GameKit.Common;

namespace GameKit.Uiui;

public readonly record struct Padding(short Top, short Bottom, short Left, short Right);

public class PaddingWidget: SingleChildContainerWidget
{
    private Padding _padding;

    public PaddingWidget(Padding padding)
    {
        _padding = padding;
    }

    protected internal override ShortVector2 Measure(ShortVector2 availableSize)
    {
        if (Child == null)
        {
            return new ShortVector2(0, 0);
        }

        int innerWidth = Math.Max(0, availableSize.X - _padding.Left - _padding.Right);
        int innerHeight = Math.Max(0, availableSize.Y - _padding.Top - _padding.Bottom);
        ShortVector2 childSize = Child.Measure(new ShortVector2(innerWidth, innerHeight));
        return new ShortVector2(
            childSize.X + _padding.Left + _padding.Right,
            childSize.Y + _padding.Top + _padding.Bottom
        );
    }

    protected internal override void Arrange(GuiContext guiContext, ShortRectangle bounds)
    {
        base.Arrange(guiContext, bounds);

        if (Child == null)
        {
            return;
        }
            
        ShortRectangle childRect = new ShortRectangle(
            (short)(bounds.X + _padding.Left),
            (short)(bounds.Y + _padding.Top),
            (short)Math.Max(0, bounds.Width - _padding.Left - _padding.Right),
            (short)Math.Max(0, bounds.Height - _padding.Top - _padding.Bottom)
        );
        Child.Arrange(guiContext, childRect);
    }

    public override void Render(GuiContext guiContext, GuiRenderer guiRenderer)
    {
        Child?.Render(guiContext, guiRenderer);
    }
}
