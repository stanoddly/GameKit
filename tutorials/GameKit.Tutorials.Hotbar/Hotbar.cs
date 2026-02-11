using GameKit.Common;
using GameKit.Gpu;
using GameKit.Pencuil;

namespace GameKit.Tutorials.Hotbar;

public class Hotbar : GuiCanvas
{
    private const int SlotCount = 9;
    private const short SlotSize = 48;
    private const short SlotGap = 4;

    private static readonly Color SlotColor = new(60, 60, 60, 255);
    private static readonly Color SelectedColor = new(200, 200, 200, 255);

    private int _selectedSlot = 0;

    public override void Build(GuiContext guiContext)
    {
        short totalWidth = (short)(SlotCount * SlotSize + (SlotCount - 1) * SlotGap);
        short startX = (short)((1280 - totalWidth) / 2);
        short startY = (short)(720 - SlotSize - 16);

        guiContext.CurrentPosition = new ShortVector2(startX, startY);
        guiContext.CurrentSize = default;

        using (guiContext.Group(LayoutDirection.Right))
        {
            for (int i = 0; i < SlotCount; i++)
            {
                Color color = i == _selectedSlot ? SelectedColor : SlotColor;
                guiContext.Panel(SlotSize, SlotSize, color);
                if (i < SlotCount - 1)
                    guiContext.DirectionSpace(SlotGap);
            }
        }
    }
}
