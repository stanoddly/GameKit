using GameKit.Common;
using GameKit.Sprites;

namespace GameKit.Uiui;

public class SpriteWidget : Widget
{
    private readonly SpriteAsset _sprite;

    public SpriteWidget(SpriteAsset sprite)
    {
        _sprite = sprite;
    }

    protected internal override ShortVector2 Measure(ShortVector2 availableSize)
    {
        return _sprite.Size;
    }

    protected internal override void Arrange(GuiContext guiContext, ShortRectangle finalRect)
    {
        base.Arrange(guiContext, finalRect);
        
        Bounds = finalRect;
    }

    public override void Render(GuiContext guiContext, GuiRenderer guiRenderer)
    {
        guiRenderer.DrawSprite(Bounds, _sprite);
    }
}