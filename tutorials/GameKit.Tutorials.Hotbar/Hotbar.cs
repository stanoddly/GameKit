using GameKit.Common;
using GameKit.Gpu;
using GameKit.Input;
using GameKit.Pencuil;
using GameKit.Text;

namespace GameKit.Tutorials.Hotbar;

public class Hotbar : StatefulGuiCanvas
{
    private const int SlotCount = 9;
    private const int SlotSize = 48;
    private const int SlotGap = 4;
    private const int LabelGap = 4;

    private static readonly Color SlotColor = new(60, 60, 60, 255);
    private static readonly Color SelectedColor = new(200, 200, 200, 255);
    private static readonly Color HoverColor = new(100, 100, 100, 255);

    private static readonly string[] SlotNames =
        ["Sword", "Shield", "Bow", "Potion", "Scroll", "Torch", "Ring", "Gem", "Key"];

    private readonly Font _font;
    private readonly State<int> _selectedSlot;
    private int _hoveredSlot = -1;

    public Hotbar(IKeyboardService keyboardService, IFontSystem fontSystem)
    {
        _font = fontSystem.Load("fonts/GohuFont-Medium.ttf", 14);
        _selectedSlot = CreateState(0);

        keyboardService.KeyDown += (_, args) =>
        {
            int index = args.Scancode - Scancode.Number1;
            if (index >= 0 && index < SlotCount)
            {
                _selectedSlot.Value = index;
            }
        };
    }

    public override void Build(Pencil pencil)
    {
        int hoveredSlot = -1;
        IntVector2 hoveredPos = default;

        using (pencil.WithGap(SlotGap))
        using (pencil.WithDirection(LayoutDirection.Right))
        {
            int totalExtent = SlotCount * SlotSize + (SlotCount - 1) * SlotGap;
            IntVector2 anchor = pencil.BottomCenter;
            pencil.MoveTo(anchor.X - totalExtent / 2, anchor.Y - SlotSize - 16);

            for (int i = 0; i < SlotCount; i++)
            {
                IntVector2 slotPos = pencil.CurrentPosition;

                Color color = i == _selectedSlot.Value ? SelectedColor
                    : i == _hoveredSlot ? HoverColor
                    : SlotColor;

                CursorState state = pencil.Panel(SlotSize, SlotSize, color);

                if (state == CursorState.Clicked)
                {
                    _selectedSlot.Value = i;
                }
                if (state >= CursorState.Hovered)
                {
                    hoveredSlot = i;
                    hoveredPos = slotPos;
                }
            }
        }

        if (hoveredSlot >= 0)
        {
            string label = SlotNames[hoveredSlot];
            IntVector2 textSize = pencil.MeasureText(label, _font);
            pencil.MoveTo(
                hoveredPos.X + (SlotSize - textSize.X) / 2,
                hoveredPos.Y - textSize.Y - LabelGap);
            pencil.Text(label, _font, Colors.White);
        }

        _hoveredSlot = hoveredSlot;
    }
}
