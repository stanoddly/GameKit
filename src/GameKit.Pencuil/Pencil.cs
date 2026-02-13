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
    private readonly Pencil _context;
    private readonly ShortVector2 _previousPosition;
    private readonly ShortVector2 _previousSize;
    private readonly LayoutDirection _previousLayoutDirection;
    private readonly short _previousGap;

    internal GroupDisposer(
        Pencil context,
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
        _context.CurrentDirection = _previousLayoutDirection;
        _context.CurrentPosition = _previousPosition;
        _context.CurrentSize = _previousSize;
        _context.CurrentGap = _previousGap;
    }
}

public class Pencil
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

    public LayoutDirection CurrentDirection { get; set; } = LayoutDirection.Bottom;
    public ShortVector2 CurrentPosition { get; set; }
    public ShortVector2 CurrentSize { get; set; }
    public ShortVector2 CursorPosition { get; set; }
    public short CurrentGap { get; set; }

    public bool CursorJustReleased { get; set; }
    public bool CursorPressed { get; set; }

    public Pencil(IGuiPlatform guiPlatform, GuiStyle guiStyle)
    {
        _guiPlatform = guiPlatform;
        Style = guiStyle;
    }

    public Pencil(IGuiPlatform guiPlatform, GuiStyle guiStyle, AppConfig appConfig) : this(guiPlatform, guiStyle)
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

        if (CurrentDirection == LayoutDirection.Bottom)
        {
            return new ShortVector2(CurrentPosition.X, (short)(CurrentPosition.Y + CurrentSize.Y + gap));
        }

        if (CurrentDirection == LayoutDirection.Top)
        {
            return new ShortVector2(CurrentPosition.X, (short)(CurrentPosition.Y - size.Y - gap));
        }

        if (CurrentDirection == LayoutDirection.Left)
        {
            return new ShortVector2((short)(CurrentPosition.X - size.X - gap), CurrentPosition.Y);
        }

        if (CurrentDirection == LayoutDirection.Right)
        {
            return new ShortVector2((short)(CurrentPosition.X + CurrentSize.X + gap), CurrentPosition.Y);
        }

        return new ShortVector2(CurrentPosition.X, CurrentPosition.Y);
    }

    public void MoveTo(short x, short y)
    {
        CurrentPosition = new ShortVector2(x, y);
    }

    public void MoveTo(ShortVector2 position)
    {
        CurrentPosition = position;
    }

    public ShortVector2 Anchor(int count, short size, short gap, HAlign h, VAlign v, short margin = 0)
    {
        short totalExtent = (short)(count * size + (count - 1) * gap);
        return Anchor(totalExtent, size, h, v, margin);
    }

    public ShortVector2 Anchor(short width, short height, HAlign h, VAlign v, short margin = 0)
    {
        short x = h switch
        {
            HAlign.Start => margin,
            HAlign.Center => (short)((_viewportWidth - width) / 2),
            HAlign.End => (short)(_viewportWidth - width - margin),
            _ => (short)0
        };

        short y = v switch
        {
            VAlign.Start => margin,
            VAlign.Center => (short)((_viewportHeight - height) / 2),
            VAlign.End => (short)(_viewportHeight - height - margin),
            _ => (short)0
        };

        return new ShortVector2(x, y);
    }

    public GroupDisposer Direction(LayoutDirection direction, short gap = 0)
    {
        var disposer = new GroupDisposer(
            this,
            CurrentPosition,
            CurrentSize,
            CurrentDirection,
            CurrentGap);

        CurrentDirection = direction;
        CurrentSize = default;
        CurrentGap = gap;

        return disposer;
    }

    public ShortVector2 MeasureString(string text, ushort fontSize) => _guiPlatform.MeasureString(text, fontSize);

    internal void ClearInstructions()
    {
        _coloredRectangleInstructions.Clear();
        _textureRegionInstructions.Clear();
        _depth = 0;
    }
}

public static class PencilExtensions
{

    public static CursorState Panel(this Pencil pencil, short width, short height, Color color)
    {
        ShortVector2 size = new ShortVector2(width, height);
        ShortVector2 position = pencil.CurrentPosition;
        ShortRectangle area = new ShortRectangle(position, size);
        pencil.AddRectangle(area, color);
        pencil.CurrentSize = size;
        pencil.CurrentPosition = pencil.DetermineNextPosition(size);

        pencil.AddHoverTest(area);
        pencil.AddHoverInTest(area);
        pencil.AddHoverOutTest(area);
        pencil.AddClickTest(area);

        if (!area.Intersects(pencil.CursorPosition))
            return CursorState.None;

        return pencil.CursorJustReleased ? CursorState.Clicked : CursorState.Hovered;
    }

    public static CursorState Button(this Pencil pencil, string text)
    {
        // add render instructions

        GuiStyle style = pencil.Style;

        var size = pencil.MeasureString(text, style.TextSize);
        ShortVector2 padding = new ShortVector2(pencil.Style.TextPadding);

        ShortVector2 fullSize = size + padding + padding;
        ShortVector2 startPosition = pencil.DetermineNextPosition(fullSize);

        ShortVector2 thickness = new ShortVector2(style.BorderThickness);
        Color innerColor = style.Background;

        ShortRectangle area = new ShortRectangle(startPosition, fullSize);

        pencil.AddHoverTest(area);
        pencil.AddHoverInTest(area);
        pencil.AddHoverOutTest(area);
        pencil.AddClickTest(area);

        if (!area.Intersects(pencil.CursorPosition))
            return CursorState.None;

        innerColor = style.ActiveColor;
        return pencil.CursorJustReleased ? CursorState.Clicked : CursorState.Hovered;
    }

}
