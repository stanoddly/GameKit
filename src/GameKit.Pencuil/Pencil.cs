using System.Numerics;
using GameKit.Common;
using GameKit.Gpu;
using GameKit.Text;

namespace GameKit.Pencuil;

public enum LayoutDirection
{
    None, Bottom, Top, Left, Right
}

public enum CursorState : byte { None, Hovered, Clicked }

public readonly struct DirectionDisposer : IDisposable
{
    private readonly Pencil _context;
    private readonly IntVector2 _previousPosition;
    private readonly IntVector2 _previousSize;
    private readonly LayoutDirection _previousLayoutDirection;

    internal DirectionDisposer(
        Pencil context,
        IntVector2 previousPosition,
        IntVector2 previousSize,
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
    private readonly int _previousGap;

    internal GapDisposer(Pencil context, int previousGap)
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

    private readonly List<Rectangle> _hoverTests = new();
    private readonly List<Rectangle> _hoverInTests = new();
    private readonly List<Rectangle> _hoverOutTests = new();
    private readonly List<Rectangle> _clickTests = new();

    internal readonly int _viewportWidth;
    internal readonly int _viewportHeight;

    public bool NeedsUpdate { get; private set; } = true;
    public void Invalidate() => NeedsUpdate = true;

    public void UpdateCursor(IntVector2 position, bool pressed)
    {

    }

    public LayoutDirection CurrentDirection { get; set; } = LayoutDirection.Bottom;
    public IntVector2 CurrentPosition { get; set; }
    public IntVector2 CurrentSize { get; set; }
    public IntVector2 CursorPosition { get; set; }
    public int CurrentGap { get; set; }

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
            _viewportWidth = (int)size.Width;
            _viewportHeight = (int)size.Height;
        }
    }

    public void AddHoverTest(Rectangle test)
    {
        _hoverTests.Add(test);
    }

    public void AddHoverInTest(Rectangle test)
    {
        _hoverInTests.Add(test);
    }

    public void AddHoverOutTest(Rectangle test)
    {
        _hoverOutTests.Add(test);
    }

    public void AddClickTest(Rectangle test)
    {
        _clickTests.Add(test);
    }

    public void AddRectangle(Rectangle rectangle, Color color)
    {
        _coloredRectangleInstructions.Add(new ColoredRectangleInstruction(_depth++, rectangle, color));
    }

    public void AddTexture(Texture texture, Rectangle area, Vector4 uvs, FColor tint)
    {
        _textureRegionInstructions.Add(new TextureRegionInstruction(_depth++, texture, area, uvs, tint));
    }

    public IntVector2 DetermineNextPosition(IntVector2 size)
    {
        int gap = CurrentSize != default ? CurrentGap : 0;

        if (CurrentDirection == LayoutDirection.Bottom)
        {
            return new IntVector2(CurrentPosition.X, CurrentPosition.Y + CurrentSize.Y + gap);
        }

        if (CurrentDirection == LayoutDirection.Top)
        {
            return new IntVector2(CurrentPosition.X, CurrentPosition.Y - size.Y - gap);
        }

        if (CurrentDirection == LayoutDirection.Left)
        {
            return new IntVector2(CurrentPosition.X - size.X - gap, CurrentPosition.Y);
        }

        if (CurrentDirection == LayoutDirection.Right)
        {
            return new IntVector2(CurrentPosition.X + CurrentSize.X + gap, CurrentPosition.Y);
        }

        return new IntVector2(CurrentPosition.X, CurrentPosition.Y);
    }

    public void MoveTo(int x, int y)
    {
        CurrentPosition = new IntVector2(x, y);
    }

    public void MoveTo(IntVector2 position)
    {
        CurrentPosition = position;
    }

    public IntVector2 TopLeft => new IntVector2(0, 0);
    public IntVector2 TopCenter => new IntVector2(_viewportWidth / 2, 0);
    public IntVector2 TopRight => new IntVector2(_viewportWidth, 0);
    public IntVector2 CenterLeft => new IntVector2(0, _viewportHeight / 2);
    public IntVector2 Center => new IntVector2(_viewportWidth / 2, _viewportHeight / 2);
    public IntVector2 CenterRight => new IntVector2(_viewportWidth, _viewportHeight / 2);
    public IntVector2 BottomLeft => new IntVector2(0, _viewportHeight);
    public IntVector2 BottomCenter => new IntVector2(_viewportWidth / 2, _viewportHeight);
    public IntVector2 BottomRight => new IntVector2(_viewportWidth, _viewportHeight);

    public DirectionDisposer WithDirection(LayoutDirection direction)
    {
        DirectionDisposer disposer = new DirectionDisposer(
            this,
            CurrentPosition,
            CurrentSize,
            CurrentDirection);

        CurrentDirection = direction;
        CurrentSize = default;

        return disposer;
    }

    public GapDisposer WithGap(int gap)
    {
        GapDisposer disposer = new GapDisposer(this, CurrentGap);
        CurrentGap = gap;
        return disposer;
    }

    public void Text(string text, Font font, Color color)
    {
        TextSpriteAsset sprite = _fontSystem.CreateTextSprite(text, font);
        Vector4 uvs = sprite.CalculateTextureRegionUVs();
        IntVector2 size = new IntVector2(sprite.Size.X, sprite.Size.Y);
        IntVector2 position = CurrentPosition;
        Rectangle area = new Rectangle(position, size);

        AddTexture(sprite.Texture, area, uvs, (FColor)color);

        CurrentSize = size;
        CurrentPosition = DetermineNextPosition(size);
    }

    public IntVector2 MeasureText(string text, Font font)
    {
        ShortSize size = _fontSystem.MeasureTextSprite(text, font);
        return new IntVector2(size.Width, size.Height);
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

    public static CursorState Panel(this Pencil pencil, int width, int height, Color color)
    {
        IntVector2 size = new IntVector2(width, height);
        IntVector2 position = pencil.CurrentPosition;
        Rectangle area = new Rectangle(position, size);
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
        GuiStyle style = pencil.Style;

        IntVector2 size = pencil.MeasureText(text, font);
        IntVector2 padding = new IntVector2(pencil.Style.TextPadding);

        IntVector2 fullSize = size + padding + padding;
        IntVector2 startPosition = pencil.DetermineNextPosition(fullSize);

        IntVector2 thickness = new IntVector2(style.BorderThickness);
        Color innerColor = style.Background;

        Rectangle area = new Rectangle(startPosition, fullSize);

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
