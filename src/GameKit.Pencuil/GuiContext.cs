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

public readonly struct GroupDisposer : IDisposable
{
    private readonly GuiContext _context;
    private readonly ShortVector2 _previousPosition;
    private readonly ShortVector2 _previousSize;
    private readonly LayoutDirection _previousLayoutDirection;
    private readonly short _previousGap;
    private readonly int _colorStartIndex;
    private readonly int _textureStartIndex;
    private readonly int _hoverStartIndex;
    private readonly int _hoverInStartIndex;
    private readonly int _hoverOutStartIndex;
    private readonly int _clickStartIndex;
    private readonly HAlign _hAlign;
    private readonly VAlign _vAlign;
    private readonly short _padding;

    internal GroupDisposer(
        GuiContext context,
        ShortVector2 previousPosition,
        ShortVector2 previousSize,
        LayoutDirection previousLayoutDirection,
        short previousGap,
        int colorStartIndex,
        int textureStartIndex,
        int hoverStartIndex,
        int hoverInStartIndex,
        int hoverOutStartIndex,
        int clickStartIndex,
        HAlign hAlign,
        VAlign vAlign,
        short padding)
    {
        _context = context;
        _previousPosition = previousPosition;
        _previousSize = previousSize;
        _previousLayoutDirection = previousLayoutDirection;
        _previousGap = previousGap;
        _colorStartIndex = colorStartIndex;
        _textureStartIndex = textureStartIndex;
        _hoverStartIndex = hoverStartIndex;
        _hoverInStartIndex = hoverInStartIndex;
        _hoverOutStartIndex = hoverOutStartIndex;
        _clickStartIndex = clickStartIndex;
        _hAlign = hAlign;
        _vAlign = vAlign;
        _padding = padding;
    }

    public void Dispose()
    {
        if (_hAlign != HAlign.None || _vAlign != VAlign.None)
            _context.PatchGroupAlignment(
                _colorStartIndex, _textureStartIndex,
                _hoverStartIndex, _hoverInStartIndex, _hoverOutStartIndex, _clickStartIndex,
                _hAlign, _vAlign, _padding);

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
    private int _depth = 0;

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
            CurrentGap,
            _coloredRectangleInstructions.Count,
            _textureRegionInstructions.Count,
            _hoverTests.Count,
            _hoverInTests.Count,
            _hoverOutTests.Count,
            _clickTests.Count,
            hAlign,
            vAlign,
            padding);

        Direction = layoutDirection;
        CurrentSize = default;
        CurrentGap = gap;

        return groupDisposer;
    }

    internal void PatchGroupAlignment(
        int colorStart, int textureStart,
        int hoverStart, int hoverInStart, int hoverOutStart, int clickStart,
        HAlign hAlign, VAlign vAlign, short padding)
    {
        int colorEnd = _coloredRectangleInstructions.Count;
        int textureEnd = _textureRegionInstructions.Count;

        if (colorStart == colorEnd && textureStart == textureEnd)
            return;

        short minX = short.MaxValue, minY = short.MaxValue;
        short maxX = short.MinValue, maxY = short.MinValue;

        for (int i = colorStart; i < colorEnd; i++)
        {
            var area = _coloredRectangleInstructions[i].Area;
            if (area.X < minX) minX = area.X;
            if (area.Y < minY) minY = area.Y;
            short right = (short)(area.X + area.Width);
            short bottom = (short)(area.Y + area.Height);
            if (right > maxX) maxX = right;
            if (bottom > maxY) maxY = bottom;
        }

        for (int i = textureStart; i < textureEnd; i++)
        {
            var area = _textureRegionInstructions[i].Area;
            if (area.X < minX) minX = area.X;
            if (area.Y < minY) minY = area.Y;
            short right = (short)(area.X + area.Width);
            short bottom = (short)(area.Y + area.Height);
            if (right > maxX) maxX = right;
            if (bottom > maxY) maxY = bottom;
        }

        short groupWidth = (short)(maxX - minX);
        short groupHeight = (short)(maxY - minY);

        short offsetX = 0;
        short offsetY = 0;

        switch (hAlign)
        {
            case HAlign.Start:
                offsetX = (short)(padding - minX);
                break;
            case HAlign.Center:
                offsetX = (short)((_viewportWidth - groupWidth) / 2 - minX);
                break;
            case HAlign.End:
                offsetX = (short)(_viewportWidth - groupWidth - padding - minX);
                break;
        }

        switch (vAlign)
        {
            case VAlign.Start:
                offsetY = (short)(padding - minY);
                break;
            case VAlign.Center:
                offsetY = (short)((_viewportHeight - groupHeight) / 2 - minY);
                break;
            case VAlign.End:
                offsetY = (short)(_viewportHeight - groupHeight - padding - minY);
                break;
        }

        ShortVector2 offset = new(offsetX, offsetY);

        for (int i = colorStart; i < colorEnd; i++)
        {
            var inst = _coloredRectangleInstructions[i];
            _coloredRectangleInstructions[i] = new ColoredRectangleInstruction(inst.Depth, inst.Area.Offset(offset), inst.Color);
        }

        for (int i = textureStart; i < textureEnd; i++)
        {
            var inst = _textureRegionInstructions[i];
            _textureRegionInstructions[i] = new TextureRegionInstruction(inst.Depth, inst.Texture, inst.Area.Offset(offset));
        }

        PatchInputTests(_hoverTests, hoverStart, offset);
        PatchInputTests(_hoverInTests, hoverInStart, offset);
        PatchInputTests(_hoverOutTests, hoverOutStart, offset);
        PatchInputTests(_clickTests, clickStart, offset);
    }

    private static void PatchInputTests(List<ShortRectangle> tests, int start, ShortVector2 offset)
    {
        for (int i = start; i < tests.Count; i++)
        {
            tests[i] = tests[i].Offset(offset);
        }
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
