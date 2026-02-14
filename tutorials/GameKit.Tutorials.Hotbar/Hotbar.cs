using GameKit.Common;
using GameKit.Gpu;
using GameKit.Input;
using GameKit.Pencuil;
using GameKit.Text;

namespace GameKit.Tutorials.Hotbar;

public record HotbarState(int SelectedSlot);

public class Hotbar : StatefulGuiCanvas<HotbarState>
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
    private int _hoveredSlot = -1;

    public static Hotbar Create(IKeyboardService keyboardService, IFontSystem fontSystem)
    {
        Font font = fontSystem.Load("fonts/GohuFont-Medium.ttf", 14);
        return new Hotbar(keyboardService, font);
    }

    public Hotbar(IKeyboardService keyboardService, Font font) : base(new HotbarState(0))
    {
        _font = font;

        keyboardService.KeyDown += (_, args) =>
        {
            int index = args.Scancode - Scancode.Number1;
            if (index >= 0 && index < SlotCount)
            {
                State = State with { SelectedSlot = index };
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

                Color color = i == State.SelectedSlot ? SelectedColor
                    : i == _hoveredSlot ? HoverColor
                    : SlotColor;

                CursorState state = pencil.Panel(SlotSize, SlotSize, color);

                if (state == CursorState.Clicked)
                {
                    State = State with { SelectedSlot = i };
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
