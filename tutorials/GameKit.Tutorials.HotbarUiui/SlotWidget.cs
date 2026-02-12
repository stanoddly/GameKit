using GameKit.Common;
using GameKit.Gpu;
using GameKit.Uiui;

namespace GameKit.Tutorials.HotbarUiui;

public class SlotWidget : Widget
{
    private FColor _color;

    public SlotWidget(FColor color)
    {
        _color = color;
    }

    public FColor Color
    {
        get => _color;
        set
        {
            _color = value;
            Invalid = true;
        }
    }

    protected override ShortVector2 Measure(ShortVector2 availableSize)
    {
        return new ShortVector2(48, 48);
    }

    public override void Render(GuiContext guiContext, GuiRenderer guiRenderer)
    {
        guiRenderer.DrawFilledRectangle(Bounds, _color);
    }
}
