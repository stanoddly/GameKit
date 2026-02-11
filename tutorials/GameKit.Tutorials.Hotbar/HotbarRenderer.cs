using GameKit.Common;
using GameKit.Gpu;
using GameKit.Pencuil;
using GameKit.RenderOrchestration;
namespace GameKit.Tutorials.Hotbar;

public class HotbarRenderer : IRenderPhase<DefaultRenderContext>
{
    private const int SlotCount = 9;
    private const short SlotSize = 48;
    private const short SlotGap = 4;

    private static readonly Color SlotColor = new(60, 60, 60, 255);
    private static readonly Color SelectedColor = new(200, 200, 200, 255);

    private readonly GuiContext _guiContext;
    private int _selectedSlot = 0;

    public HotbarRenderer(GuiContext guiContext)
    {
        _guiContext = guiContext;
    }

    public void Render(DefaultRenderContext renderContext)
    {
        short totalWidth = (short)(SlotCount * SlotSize + (SlotCount - 1) * SlotGap);
        short startX = (short)((1280 - totalWidth) / 2);
        short startY = (short)(720 - SlotSize - 16);

        _guiContext.CurrentPosition = new ShortVector2(startX, startY);
        _guiContext.CurrentSize = default;

        using (_guiContext.Group(LayoutDirection.Right))
        {
            for (int i = 0; i < SlotCount; i++)
            {
                Color color = i == _selectedSlot ? SelectedColor : SlotColor;
                _guiContext.Panel(SlotSize, SlotSize, color);
                if (i < SlotCount - 1)
                    _guiContext.DirectionSpace(SlotGap);
            }
        }

        _guiContext.Draw();

        using IRenderPass renderPass = new RenderPassBuilder(renderContext.CommandBuffer)
            .AddColorTarget(renderContext.SwapchainTexture)
            .SetSharedColorTargetSettings(ColorTargetSettings.Clear)
            .Build();
    }
}
