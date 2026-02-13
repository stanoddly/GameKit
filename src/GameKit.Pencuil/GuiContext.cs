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

public class NullGuiPlatform : IGuiPlatform
{
    public ShortVector2 MeasureString(string text, ushort fontSize) => default;
    public void DrawRectangle(ShortRectangle rectangle, Color color) { }
    public void DrawText(string text, ushort size, Color color) { }
    public void DrawTexture(Texture texture, ShortRectangle region) { }
}

public enum LayoutDirection
{
    None, Bottom, Top, Left, Right
}

public enum HAlign : byte { None, Start, Center, End }
public enum VAlign : byte { None, Start, Center, End }
public enum CursorState : byte { None, Hovered, Clicked }

public readonly struct GroupDisposer : IDisposable
{
    private readonly GuiContext _context;
    private readonly ShortVector2 _previousPosition;
    private readonly ShortVector2 _previousSize;
    private readonly LayoutDirection _previousLayoutDirection;
    private readonly short _previousGap;

    internal GroupDisposer(
        GuiContext context,
        ShortVector2 previousPosition,
        ShortVector2 previousSize,
        LayoutDirection previousLayoutDirection,
        short previousGap)
    {
        _context = context;
        _previousPosition = previousPosition;
        _previousSize = previousSize;
        _previousLayoutDirection = previousLayoutDirection;
        _previousGap = previousGap;
    }

    public void Dispose()
    {
        _context.Direction = _previousLayoutDirection;
        _context.CurrentPosition = _previousPosition;
        _context.CurrentSize = _previousSize;
        _context.CurrentGap = _previousGap;
    }
}

public class GuiContext
{
    private readonly IGuiPlatform _guiPlatform;
    public GuiStyle Style { get; }
    internal int _depth = 0;

    internal readonly List<ColoredRectangleInstruction> _coloredRectangleInstructions = new();
    internal readonly List<TextureRegionInstruction> _textureRegionInstructions = new();

    private readonly List<ShortRectangle> _hoverTests = new();
    private readonly List<ShortRectangle> _hoverInTests = new();
    private readonly List<ShortRectangle> _hoverOutTests = new();
    private readonly List<ShortRectangle> _clickTests = new();

    internal readonly short _viewportWidth;
    internal readonly short _viewportHeight;

    public bool NeedsUpdate { get; private set; } = true;
    public void Invalidate() => NeedsUpdate = true;

    public void UpdateCursor(ShortVector2 position, bool pressed)
    {

    }

    public LayoutDirection Direction { get; set; } = LayoutDirection.Bottom;
    public ShortVector2 CurrentPosition { get; set; }
    public ShortVector2 CurrentSize { get; set; }
    public ShortVector2 CursorPosition { get; set; }
    public short CurrentGap { get; set; }

    public bool CursorJustReleased { get; set; }
    public bool CursorPressed { get; set; }

    public GuiContext(IGuiPlatform guiPlatform, GuiStyle guiStyle)
    {
        _guiPlatform = guiPlatform;
        Style = guiStyle;
    }

    public GuiContext(IGuiPlatform guiPlatform, GuiStyle guiStyle, AppConfig appConfig) : this(guiPlatform, guiStyle)
    {
        if (appConfig.Size is { } size)
        {
            _viewportWidth = (short)size.Width;
            _viewportHeight = (short)size.Height;
        }
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
        short gap = CurrentSize != default ? CurrentGap : (short)0;

        if (Direction == LayoutDirection.Bottom)
        {
            return new ShortVector2(CurrentPosition.X, (short)(CurrentPosition.Y + CurrentSize.Y + gap));
        }

        if (Direction == LayoutDirection.Top)
        {
            return new ShortVector2(CurrentPosition.X, (short)(CurrentPosition.Y - size.Y - gap));
        }

        if (Direction == LayoutDirection.Left)
        {
            return new ShortVector2((short)(CurrentPosition.X - size.X - gap), CurrentPosition.Y);
        }

        if (Direction == LayoutDirection.Right)
        {
            return new ShortVector2((short)(CurrentPosition.X + CurrentSize.X + gap), CurrentPosition.Y);
        }

        return new ShortVector2(CurrentPosition.X, CurrentPosition.Y);
    }

    public GroupDisposer Group(
        LayoutDirection layoutDirection = LayoutDirection.Bottom,
        HAlign hAlign = HAlign.None, VAlign vAlign = VAlign.None,
        short gap = 0, short padding = 0)
    {
        var groupDisposer = new GroupDisposer(
            this,
            CurrentPosition,
            CurrentSize,
            Direction,
            CurrentGap);

        Direction = layoutDirection;
        CurrentSize = default;
        CurrentGap = gap;

        return groupDisposer;
    }

    public ShortVector2 MeasureString(string text, ushort fontSize) => _guiPlatform.MeasureString(text, fontSize);

    internal void ClearInstructions()
    {
        _coloredRectangleInstructions.Clear();
        _textureRegionInstructions.Clear();
        _depth = 0;
    }
}

public static class GuiContextExtensions
{

    public static CursorState Panel(this GuiContext guiContext, short width, short height, Color color)
    {
        ShortVector2 size = new ShortVector2(width, height);
        ShortVector2 position = guiContext.DetermineNextPosition(size);
        ShortRectangle area = new ShortRectangle(position, size);
        guiContext.AddRectangle(area, color);
        guiContext.CurrentPosition = position;
        guiContext.CurrentSize = size;

        guiContext.AddHoverTest(area);
        guiContext.AddHoverInTest(area);
        guiContext.AddHoverOutTest(area);
        guiContext.AddClickTest(area);

        if (!area.Intersects(guiContext.CursorPosition))
            return CursorState.None;

        return guiContext.CursorJustReleased ? CursorState.Clicked : CursorState.Hovered;
    }

    public static CursorState Button(this GuiContext guiContext, string text)
    {
        // add render instructions

        GuiStyle style = guiContext.Style;

        var size = guiContext.MeasureString(text, style.TextSize);
        ShortVector2 padding = new ShortVector2(guiContext.Style.TextPadding);

        ShortVector2 fullSize = size + padding + padding;
        ShortVector2 startPosition = guiContext.DetermineNextPosition(fullSize);

        ShortVector2 thickness = new ShortVector2(style.BorderThickness);
        Color innerColor = style.Background;

        ShortRectangle area = new ShortRectangle(startPosition, fullSize);

        guiContext.AddHoverTest(area);
        guiContext.AddHoverInTest(area);
        guiContext.AddHoverOutTest(area);
        guiContext.AddClickTest(area);

        if (!area.Intersects(guiContext.CursorPosition))
            return CursorState.None;

        innerColor = style.ActiveColor;
        return guiContext.CursorJustReleased ? CursorState.Clicked : CursorState.Hovered;
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
