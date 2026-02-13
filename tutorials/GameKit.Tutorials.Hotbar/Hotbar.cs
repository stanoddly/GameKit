using GameKit.Gpu;
using GameKit.Input;
using GameKit.Pencuil;

namespace GameKit.Tutorials.Hotbar;

public class Hotbar : GuiCanvas
{
    private const int SlotCount = 9;
    private const short SlotSize = 48;
    private const short SlotGap = 4;

    private static readonly Color SlotColor = new(60, 60, 60, 255);
    private static readonly Color SelectedColor = new(200, 200, 200, 255);
    private static readonly Color HoverColor = new(100, 100, 100, 255);

    private int _selectedSlot = 0;
    private int _hoveredSlot = -1;

    public Hotbar(IKeyboardService keyboardService)
    {
        keyboardService.KeyDown += (_, args) =>
        {
            int index = args.Scancode - Scancode.Number1;
            if (index >= 0 && index < SlotCount)
                _selectedSlot = index;
        };
    }

    public override void Build(Pencil pencil)
    {
        int hoveredSlot = -1;

        pencil.MoveTo(pencil.Anchor(SlotCount, SlotSize, SlotGap, HAlign.Center, VAlign.End, margin: 16));

        using (pencil.Direction(LayoutDirection.Right, gap: SlotGap))
        {
            for (int i = 0; i < SlotCount; i++)
            {
                Color color = i == _selectedSlot ? SelectedColor
                    : i == _hoveredSlot ? HoverColor
                    : SlotColor;

                CursorState state = pencil.Panel(SlotSize, SlotSize, color);

                if (state == CursorState.Clicked)
                    _selectedSlot = i;
                if (state >= CursorState.Hovered)
                    hoveredSlot = i;
            }
        }

        _hoveredSlot = hoveredSlot;
    }
}
