using GameKit.Common;

namespace GameKit.Uiui;

public class FloatLayout : ContainerWidget
{
    protected internal override ShortVector2 Measure(ShortVector2 availableSize)
    {
        return availableSize;
    }

    protected internal override void Arrange(GuiContext guiContext, ShortRectangle bounds)
    {
        base.Arrange(guiContext, bounds);

        foreach (Widget widget in Children)
        {
            widget.Arrange(guiContext, bounds);
        }
    }
}
