using System.Numerics;
using GameKit.Common;
using GameKit.Gpu;
using GameKit.Text;

namespace GameKit.Pencuil;

public enum LayoutDirection
{
    None, Bottom, Top, Left, Right
}

public enum HAlign : byte { None, Start, Center, End }
public enum VAlign : byte { None, Start, Center, End }
public enum CursorState : byte { None, Hovered, Clicked }

public readonly struct DirectionDisposer : IDisposable
{
    private readonly Pencil _context;
    private readonly ShortVector2 _previousPosition;
    private readonly ShortVector2 _previousSize;
    private readonly LayoutDirection _previousLayoutDirection;

    internal DirectionDisposer(
        Pencil context,
        ShortVector2 previousPosition,
        ShortVector2 previousSize,
        LayoutDirection previousLayoutDirection)
    {
        _context = context;
        _previousPosition = previousPosition;
        _previousSize = previousSize;
        _previousLayoutDirection = previousLayoutDirection;
    }

    public void Dispose()
    {
        _context.CurrentDirection = _previousLayoutDirection;
        _context.CurrentPosition = _previousPosition;
        _context.CurrentSize = _previousSize;
    }
}

public readonly struct GapDisposer : IDisposable
{
    private readonly Pencil _context;
    private readonly short _previousGap;

    internal GapDisposer(Pencil context, short previousGap)
    {
        _context = context;
        _previousGap = previousGap;
    }

    public void Dispose() => _context.CurrentGap = _previousGap;
}

public class Pencil
{
    private readonly IFontSystem _fontSystem;
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

    public Pencil(IFontSystem fontSystem, GuiStyle guiStyle)
    {
        _fontSystem = fontSystem;
        Style = guiStyle;
    }

    public Pencil(IFontSystem fontSystem, GuiStyle guiStyle, AppConfig appConfig) : this(fontSystem, guiStyle)
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

    public void AddTexture(Texture texture, ShortRectangle area, Vector4 uvs, FColor tint)
    {
        _textureRegionInstructions.Add(new TextureRegionInstruction(_depth++, texture, area, uvs, tint));
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

    public void Align(HAlign h, VAlign v, short size, int count = 1, short margin = 0)
    {
        short totalExtent = (short)(count * size + (count - 1) * CurrentGap);
        short x = h switch
        {
            HAlign.Start => margin,
            HAlign.Center => (short)((_viewportWidth - totalExtent) / 2),
            HAlign.End => (short)(_viewportWidth - totalExtent - margin),
            _ => (short)0
        };

        short y = v switch
        {
            VAlign.Start => margin,
            VAlign.Center => (short)((_viewportHeight - size) / 2),
            VAlign.End => (short)(_viewportHeight - size - margin),
            _ => (short)0
        };

        CurrentPosition = new ShortVector2(x, y);
    }

    public void AlignBottomCenter(short size, int count = 1, short margin = 0)
        => Align(HAlign.Center, VAlign.End, size, count, margin);

    public void AlignTopLeft(short size, int count = 1, short margin = 0)
        => Align(HAlign.Start, VAlign.Start, size, count, margin);

    public void AlignTopRight(short size, int count = 1, short margin = 0)
        => Align(HAlign.End, VAlign.Start, size, count, margin);

    public void AlignCenter(short size, int count = 1, short margin = 0)
        => Align(HAlign.Center, VAlign.Center, size, count, margin);

    public DirectionDisposer WithDirection(LayoutDirection direction)
    {
        var disposer = new DirectionDisposer(
            this,
            CurrentPosition,
            CurrentSize,
            CurrentDirection);

        CurrentDirection = direction;
        CurrentSize = default;

        return disposer;
    }

    public GapDisposer WithGap(short gap)
    {
        var disposer = new GapDisposer(this, CurrentGap);
        CurrentGap = gap;
        return disposer;
    }

    public void Text(string text, Font font, Color color)
    {
        TextSpriteAsset sprite = _fontSystem.CreateTextSprite(text, font);
        Vector4 uvs = sprite.CalculateTextureRegionUVs();
        ShortVector2 size = sprite.Size;
        ShortVector2 position = CurrentPosition;
        ShortRectangle area = new ShortRectangle(position, size);

        AddTexture(sprite.Texture, area, uvs, (FColor)color);

        CurrentSize = size;
        CurrentPosition = DetermineNextPosition(size);
    }

    public ShortVector2 MeasureText(string text, Font font)
    {
        ShortSize size = _fontSystem.MeasureTextSprite(text, font);
        return new ShortVector2((short)size.Width, (short)size.Height);
    }

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

    public static CursorState Button(this Pencil pencil, string text, Font font)
    {
        // add render instructions

        GuiStyle style = pencil.Style;

        var size = pencil.MeasureText(text, font);
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
