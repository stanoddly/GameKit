using GameKit.Common;
using GameKit.Gpu;

namespace GameKit.Pencuil;

public interface IGuiPlatform
{
    ShortVector2 MeasureString(string text, ushort fontSize);
    void DrawRectangle(ShortRectangle rectangle, Color color);
    void DrawText(string text, ushort size, Color color);
    void DrawTexture(Texture texture, ShortRectangle region);
}

public enum LayoutDirection
{
    None, Bottom, Top, Left, Right
}

public readonly struct GroupDisposer(GuiContext _context, ShortVector2 _previousPosition, LayoutDirection _previousLayoutDirection): IDisposable
{
    public void Dispose()
    {
        _context.Direction = _previousLayoutDirection;
        _context.CurrentPosition = _previousPosition;
        // TODO: handle area too
    }
}

public class GuiContext
{
    private readonly IGuiPlatform _guiPlatform;
    public GuiStyle Style { get; }
    private int _depth = 0;

    private List<ColoredRectangleInstruction> _coloredRectangleInstructions = new();
    private List<TextureRegionInstruction> _textureRegionInstructions = new();

    private readonly List<ShortRectangle> _hoverTests = new();
    private readonly List<ShortRectangle> _hoverInTests = new();
    private readonly List<ShortRectangle> _hoverOutTests = new();
    private readonly List<ShortRectangle> _clickTests = new();

    public bool NeedsUpdate { get; private set; } = true;
    public void Invalidate() => NeedsUpdate = true;

    public void UpdateCursor(ShortVector2 position, bool pressed)
    {

    }

    public LayoutDirection Direction { get; set; } = LayoutDirection.Bottom;
    public ShortVector2 CurrentPosition { get; set; }
    public ShortVector2 CurrentSize { get; set; }
    public ShortVector2 CursorPosition { get; set; }

    public bool CursorJustReleased { get; set; }
    public bool CursorPressed { get; set; }

    public GuiContext(IGuiPlatform guiPlatform, GuiStyle guiStyle)
    {
        _guiPlatform = guiPlatform;
        Style = guiStyle;
    }

    public void AddHoverTest(ShortRectangle test)
    {
        _hoverTests.Add(test);
    }

    public void AddHoverInTest(ShortRectangle test)
    {
        _hoverInTests.Add(test);
    }

    public void AddHoverOutTest(ShortRectangle test)
    {
        _hoverOutTests.Add(test);
    }

    public void AddClickTest(ShortRectangle test)
    {
        _clickTests.Add(test);
    }

    public void AddRectangle(ShortRectangle rectangle, Color color)
    {
        _coloredRectangleInstructions.Add(new ColoredRectangleInstruction(_depth++, rectangle, color));
    }

    public void AddTexture(Texture texture, ShortRectangle region)
    {
        _textureRegionInstructions.Add(new TextureRegionInstruction(_depth++, texture, region));
    }

    public ShortVector2 DetermineNextPosition(ShortVector2 size)
    {
        if (Direction == LayoutDirection.Bottom)
        {
            return new ShortVector2(CurrentPosition.X, (short)(CurrentPosition.Y + CurrentSize.Y));
        }

        if (Direction == LayoutDirection.Top)
        {
            return new ShortVector2(CurrentPosition.X, (short)(CurrentPosition.Y - size.Y));
        }

        if (Direction == LayoutDirection.Left)
        {
            return new ShortVector2((short)(CurrentPosition.X - size.X), CurrentPosition.Y);
        }

        if (Direction == LayoutDirection.Right)
        {
            return new ShortVector2((short)(CurrentPosition.X + size.X + CurrentSize.X), CurrentPosition.Y);
        }

        return new ShortVector2(CurrentPosition.X, CurrentPosition.Y);
    }

    public GroupDisposer Group(LayoutDirection layoutDirection = LayoutDirection.Bottom)
    {
        var groupDisposer = new GroupDisposer(this, CurrentPosition, layoutDirection);

        Direction = layoutDirection;

        return groupDisposer;
    }

    public ShortVector2 MeasureString(string text, ushort fontSize) => _guiPlatform.MeasureString(text, fontSize);

    public void Draw()
    {
        foreach (var instruction in _coloredRectangleInstructions)
            _guiPlatform.DrawRectangle(instruction.Area, instruction.Color);

        foreach (var instruction in _textureRegionInstructions)
            _guiPlatform.DrawTexture(instruction.Texture, instruction.Area);

        _coloredRectangleInstructions.Clear();
        _textureRegionInstructions.Clear();
        _depth = 0;
    }
}

public static class GuiContextExtensions
{

    public static void Panel(this GuiContext guiContext, short width, short height, Color color)
    {
        ShortVector2 size = new ShortVector2(width, height);
        ShortVector2 position = guiContext.DetermineNextPosition(size);
        guiContext.AddRectangle(new ShortRectangle(position, size), color);
        guiContext.CurrentPosition = position;
        guiContext.CurrentSize = size;
    }

    public static bool Button(this GuiContext guiContext, string text)
    {
        // add render instructions

        bool isClicked = false;

        GuiStyle style = guiContext.Style;

        var size = guiContext.MeasureString(text, style.TextSize);
        ShortVector2 padding = new ShortVector2(guiContext.Style.TextPadding);

        ShortVector2 fullSize = size + padding + padding;
        ShortVector2 startPosition = guiContext.DetermineNextPosition(fullSize);

        ShortVector2 thickness = new ShortVector2(style.BorderThickness);
        Color innerColor = style.Background;

        ShortRectangle area = new ShortRectangle(startPosition, fullSize);
        if (area.Intersects(guiContext.CursorPosition))
        {
            innerColor = style.ActiveColor;

            isClicked = guiContext.CursorJustReleased;
        }

        guiContext.AddClickTest(area);
        guiContext.AddHoverInTest(area);
        guiContext.AddHoverOutTest(area);

        return isClicked;
    }

    public static GroupDisposer BottomGroup(this GuiContext guiContext)
    {
        return guiContext.Group(LayoutDirection.Bottom);
    }

    public static GroupDisposer TopGroup(this GuiContext guiContext)
    {
        return guiContext.Group(LayoutDirection.Top);
    }

    public static void VerticalSpace(this GuiContext guiContext, short size)
    {
        ShortVector2 startPosition = guiContext.DetermineNextPosition(new ShortVector2((short)0, size));
        guiContext.CurrentPosition = startPosition;
    }

    public static void HorizontalSpace(this GuiContext guiContext, short size)
    {
        if (guiContext.Direction == LayoutDirection.Left)
        {
            size = (short)-size;
        }
        guiContext.CurrentPosition += new ShortVector2(size, (short)0);
    }

    public static void DirectionSpace(this GuiContext guiContext, short size)
    {
        LayoutDirection direction = guiContext.Direction;
        ShortVector2 vector = default;

        if (direction == LayoutDirection.Bottom)
        {
            vector = new ShortVector2(size, (short)0);
        }

        if (direction == LayoutDirection.Top)
        {
            vector = new ShortVector2((short)-size, (short)0);
        }

        if (direction == LayoutDirection.Left)
        {
            vector = new ShortVector2((short)-size, (short)0);
        }

        if (direction == LayoutDirection.Right)
        {
            vector = new ShortVector2(size, (short)0);
        }

        guiContext.CurrentPosition += vector;
    }
}
