using GameKit.Common;
using GameKit.Gpu;
using GameKit.Input;
using GameKit.Uiui;

namespace GameKit.Tutorials.HotbarUiui;

public class HotbarWidget : BoxLayout<SlotWidget>
{
    private const int SlotCount = 9;

    private static readonly FColor SlotColor = new(0.235f, 0.235f, 0.235f, 1.0f);
    private static readonly FColor SelectedColor = new(0.784f, 0.784f, 0.784f, 1.0f);
    private static readonly FColor HoverColor = new(0.392f, 0.392f, 0.392f, 1.0f);

    private readonly GuiResolutionProvider _guiResolutionProvider;
    private int _selectedSlot;
    private int _hoveredSlot = -1;

    public HotbarWidget(IMouseService mouseService, GuiResolutionProvider guiResolutionProvider)
        : base(Orientation.Horizontal, spacing: 4)
    {
        _guiResolutionProvider = guiResolutionProvider;

        for (int i = 0; i < SlotCount; i++)
        {
            Add(new SlotWidget(i == 0 ? SelectedColor : SlotColor));
        }

        mouseService.Motion += (_, args) =>
        {
            ShortVector2 cursor = ScreenToBase(args.Position);
            int newHover = HitTest(cursor);
            if (newHover != _hoveredSlot)
            {
                _hoveredSlot = newHover;
                UpdateSlotColors();
            }
        };

        mouseService.ButtonRelease += (_, args) =>
        {
            if (args.Button != MouseButton.Left) return;

            ShortVector2 cursor = ScreenToBase(args.Position);
            int hit = HitTest(cursor);
            if (hit >= 0)
            {
                _selectedSlot = hit;
                UpdateSlotColors();
            }
        };
    }

    public override bool OnKeyDown(Keyboard keyboard, KeyEventArgs keyEventArgs)
    {
        int index = keyEventArgs.Scancode - Scancode.Number1;
        if (index >= 0 && index < SlotCount)
        {
            _selectedSlot = index;
            UpdateSlotColors();
            return true;
        }

        return base.OnKeyDown(keyboard, keyEventArgs);
    }

    private ShortVector2 ScreenToBase(System.Numerics.Vector2 screenPos)
    {
        ushort scale = _guiResolutionProvider.ResolutionInfo.ScaleFactor;
        return new ShortVector2((short)(screenPos.X / scale), (short)(screenPos.Y / scale));
    }

    private int HitTest(ShortVector2 cursor)
    {
        for (int i = 0; i < Children.Count; i++)
        {
            if (Children[i].Bounds.Intersects(cursor))
                return i;
        }
        return -1;
    }

    private void UpdateSlotColors()
    {
        for (int i = 0; i < Children.Count; i++)
        {
            FColor color = i == _selectedSlot ? SelectedColor
                : i == _hoveredSlot ? HoverColor
                : SlotColor;
            Children[i].Color = color;
        }
    }
}
