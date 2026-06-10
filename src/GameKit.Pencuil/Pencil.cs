using System.Numerics;
using System.Runtime.InteropServices;
using GameKit.Common;
using GameKit.Gpu;
using GameKit.Input;
using GameKit.Sprites;
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
    private readonly Vector2Int _previousPosition;
    private readonly Vector2Int _previousSize;
    private readonly LayoutDirection _previousLayoutDirection;

    internal DirectionDisposer(
        Pencil context,
        Vector2Int previousPosition,
        Vector2Int previousSize,
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
    private readonly IClipboardService _clipboardService;
    public GuiStyle Style { get; }
    internal int _depth = 0;

    internal List<ColoredRectangleInstruction> _coloredRectangleInstructions = new();
    internal List<TextureRegionInstruction> _textureRegionInstructions = new();

    private List<ColoredRectangleInstruction> _previousColoredRectangleInstructions = new();
    private List<TextureRegionInstruction> _previousTextureRegionInstructions = new();

    private readonly List<Rectangle> _hoverTests = new();
    private readonly List<Rectangle> _hoverInTests = new();
    private readonly List<Rectangle> _hoverOutTests = new();
    private readonly List<Rectangle> _clickTests = new();

    internal int _viewportWidth;
    internal int _viewportHeight;

    public bool NeedsUpdate { get; set; } = true;
    public void Invalidate() => NeedsUpdate = true;

    internal void UpdateViewport(int width, int height)
    {
        _viewportWidth = width;
        _viewportHeight = height;
        Invalidate();
    }

    public void UpdateCursor(Vector2Int position, bool pressed)
    {

    }

    public LayoutDirection CurrentDirection { get; set; } = LayoutDirection.Bottom;
    public Vector2Int CurrentPosition { get; set; }
    public Vector2Int CurrentSize { get; set; }
    public Vector2Int CursorPosition { get; set; }
    public int CurrentGap { get; set; }

    public bool CursorJustReleased { get; set; }
    public bool CursorPressed { get; set; }

    public int? FocusedControlId { get; private set; }
    public bool HasFocus => FocusedControlId != null;
    internal bool FocusClaimedThisFrame;
    internal TextFieldEditingState? EditingState;

    public Pencil(IFontSystem fontSystem, IClipboardService clipboardService, GuiStyle guiStyle, AppConfig appConfig)
    {
        _fontSystem = fontSystem;
        _clipboardService = clipboardService;
        Style = guiStyle;
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

    public bool IsOverInteractiveArea(Vector2Int position)
    {
        foreach (Rectangle area in _clickTests)
        {
            if (area.Intersects(position))
            {
                return true;
            }
        }

        return false;
    }

    public void AddRectangle(Rectangle rectangle, Color color)
    {
        _coloredRectangleInstructions.Add(new ColoredRectangleInstruction(_depth++, rectangle, color));
    }

    public void AddTexture(Texture texture, Rectangle area, Vector4 uvs, FColor tint)
    {
        _textureRegionInstructions.Add(new TextureRegionInstruction(_depth++, texture, area, uvs, tint));
    }

    public Vector2Int DetermineNextPosition(Vector2Int size)
    {
        int gap = CurrentSize != default ? CurrentGap : 0;

        if (CurrentDirection == LayoutDirection.Bottom)
        {
            return new Vector2Int(CurrentPosition.X, CurrentPosition.Y + CurrentSize.Y + gap);
        }

        if (CurrentDirection == LayoutDirection.Top)
        {
            return new Vector2Int(CurrentPosition.X, CurrentPosition.Y - size.Y - gap);
        }

        if (CurrentDirection == LayoutDirection.Left)
        {
            return new Vector2Int(CurrentPosition.X - size.X - gap, CurrentPosition.Y);
        }

        if (CurrentDirection == LayoutDirection.Right)
        {
            return new Vector2Int(CurrentPosition.X + CurrentSize.X + gap, CurrentPosition.Y);
        }

        return new Vector2Int(CurrentPosition.X, CurrentPosition.Y);
    }

    public void MoveTo(int x, int y)
    {
        CurrentPosition = new Vector2Int(x, y);
    }

    public void MoveTo(Vector2Int position)
    {
        CurrentPosition = position;
    }

    public Vector2Int TopLeft => new Vector2Int(0, 0);
    public Vector2Int TopCenter => new Vector2Int(_viewportWidth / 2, 0);
    public Vector2Int TopRight => new Vector2Int(_viewportWidth, 0);
    public Vector2Int CenterLeft => new Vector2Int(0, _viewportHeight / 2);
    public Vector2Int Center => new Vector2Int(_viewportWidth / 2, _viewportHeight / 2);
    public Vector2Int CenterRight => new Vector2Int(_viewportWidth, _viewportHeight / 2);
    public Vector2Int BottomLeft => new Vector2Int(0, _viewportHeight);
    public Vector2Int BottomCenter => new Vector2Int(_viewportWidth / 2, _viewportHeight);
    public Vector2Int BottomRight => new Vector2Int(_viewportWidth, _viewportHeight);

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
        if (text.Length == 0)
        {
            return;
        }

        TextSpriteAsset sprite = _fontSystem.CreateTextSprite(text, font);
        Vector4 uvs = sprite.CalculateTextureRegionUVs();
        Vector2Int size = new Vector2Int(sprite.Size.X, sprite.Size.Y);
        Vector2Int position = CurrentPosition;
        Rectangle area = new Rectangle(position, size);

        AddTexture(sprite.Texture, area, uvs, (FColor)color);

        CurrentSize = size;
        CurrentPosition = DetermineNextPosition(size);
    }

    public Vector2Int MeasureText(string text, Font font)
    {
        if (text.Length == 0)
        {
            return default;
        }

        ShortSize size = _fontSystem.MeasureTextSprite(text, font);
        return new Vector2Int(size.Width, size.Height);
    }

    public bool IsFocused(int id) => FocusedControlId == id;

    internal void Focus(int id, string initialValue)
    {
        FocusedControlId = id;
        FocusClaimedThisFrame = true;
        EditingState = new TextFieldEditingState(initialValue);
        Invalidate();
    }

    internal void Blur()
    {
        FocusedControlId = null;
        EditingState = null;
        Invalidate();
    }

    internal void InsertText(string text)
    {
        if (EditingState == null)
        {
            return;
        }

        if (EditingState.HasSelection)
        {
            EditingState.DeleteSelection();
        }

        EditingState.Buffer = EditingState.Buffer.Insert(EditingState.CursorPosition, text);
        EditingState.CursorPosition += text.Length;
        Invalidate();
    }

    internal bool HandleEditingKeyDown(Scancode scancode, bool shift, bool ctrl)
    {
        if (EditingState == null)
        {
            return false;
        }

        switch (scancode)
        {
            case Scancode.Backspace:
                if (EditingState.HasSelection)
                {
                    EditingState.DeleteSelection();
                }
                else if (ctrl)
                {
                    int target = FindWordBoundaryLeft(EditingState.Buffer, EditingState.CursorPosition);
                    EditingState.Buffer = EditingState.Buffer.Remove(target, EditingState.CursorPosition - target);
                    EditingState.CursorPosition = target;
                }
                else if (EditingState.CursorPosition > 0)
                {
                    EditingState.Buffer = EditingState.Buffer.Remove(EditingState.CursorPosition - 1, 1);
                    EditingState.CursorPosition--;
                }
                break;
            case Scancode.Delete:
                if (EditingState.HasSelection)
                {
                    EditingState.DeleteSelection();
                }
                else if (ctrl)
                {
                    int target = FindWordBoundaryRight(EditingState.Buffer, EditingState.CursorPosition);
                    EditingState.Buffer = EditingState.Buffer.Remove(EditingState.CursorPosition, target - EditingState.CursorPosition);
                }
                else if (EditingState.CursorPosition < EditingState.Buffer.Length)
                {
                    EditingState.Buffer = EditingState.Buffer.Remove(EditingState.CursorPosition, 1);
                }
                break;
            case Scancode.Left:
                if (shift)
                {
                    EditingState.SelectionAnchor ??= EditingState.CursorPosition;
                    EditingState.CursorPosition = ctrl
                        ? FindWordBoundaryLeft(EditingState.Buffer, EditingState.CursorPosition)
                        : Math.Max(0, EditingState.CursorPosition - 1);
                }
                else if (EditingState.HasSelection && !ctrl)
                {
                    (int start, _) = EditingState.GetSelectionRange();
                    EditingState.CursorPosition = start;
                    EditingState.SelectionAnchor = null;
                }
                else
                {
                    EditingState.SelectionAnchor = null;
                    EditingState.CursorPosition = ctrl
                        ? FindWordBoundaryLeft(EditingState.Buffer, EditingState.CursorPosition)
                        : Math.Max(0, EditingState.CursorPosition - 1);
                }
                break;
            case Scancode.Right:
                if (shift)
                {
                    EditingState.SelectionAnchor ??= EditingState.CursorPosition;
                    EditingState.CursorPosition = ctrl
                        ? FindWordBoundaryRight(EditingState.Buffer, EditingState.CursorPosition)
                        : Math.Min(EditingState.Buffer.Length, EditingState.CursorPosition + 1);
                }
                else if (EditingState.HasSelection && !ctrl)
                {
                    (int start, int length) = EditingState.GetSelectionRange();
                    EditingState.CursorPosition = start + length;
                    EditingState.SelectionAnchor = null;
                }
                else
                {
                    EditingState.SelectionAnchor = null;
                    EditingState.CursorPosition = ctrl
                        ? FindWordBoundaryRight(EditingState.Buffer, EditingState.CursorPosition)
                        : Math.Min(EditingState.Buffer.Length, EditingState.CursorPosition + 1);
                }
                break;
            case Scancode.Home:
                if (shift)
                {
                    EditingState.SelectionAnchor ??= EditingState.CursorPosition;
                }
                else
                {
                    EditingState.SelectionAnchor = null;
                }
                EditingState.CursorPosition = 0;
                break;
            case Scancode.End:
                if (shift)
                {
                    EditingState.SelectionAnchor ??= EditingState.CursorPosition;
                }
                else
                {
                    EditingState.SelectionAnchor = null;
                }
                EditingState.CursorPosition = EditingState.Buffer.Length;
                break;
            case Scancode.A:
                if (ctrl)
                {
                    EditingState.SelectionAnchor = 0;
                    EditingState.CursorPosition = EditingState.Buffer.Length;
                }
                else
                {
                    return false;
                }
                break;
            case Scancode.C:
                if (ctrl && EditingState.HasSelection)
                {
                    _clipboardService.SetText(EditingState.GetSelectedText());
                }
                else if (!ctrl)
                {
                    return false;
                }
                break;
            case Scancode.X:
                if (ctrl && EditingState.HasSelection)
                {
                    _clipboardService.SetText(EditingState.GetSelectedText());
                    EditingState.DeleteSelection();
                }
                else if (!ctrl)
                {
                    return false;
                }
                break;
            case Scancode.V:
                if (ctrl)
                {
                    string? clipboardText = _clipboardService.GetText();
                    if (clipboardText != null)
                    {
                        if (EditingState.HasSelection)
                        {
                            EditingState.DeleteSelection();
                        }
                        EditingState.Buffer = EditingState.Buffer.Insert(EditingState.CursorPosition, clipboardText);
                        EditingState.CursorPosition += clipboardText.Length;
                    }
                }
                else
                {
                    return false;
                }
                break;
            case Scancode.Return:
            case Scancode.Return2:
            case Scancode.KeypadEnter:
                EditingState.Committed = true;
                break;
            case Scancode.Escape:
                EditingState.Canceled = true;
                break;
            default:
                return false;
        }

        Invalidate();
        return true;
    }

    private static int FindWordBoundaryLeft(string text, int position)
    {
        if (position <= 0)
        {
            return 0;
        }

        int i = position - 1;
        while (i > 0 && char.IsWhiteSpace(text[i]))
        {
            i--;
        }
        while (i > 0 && !char.IsWhiteSpace(text[i - 1]))
        {
            i--;
        }
        return i;
    }

    private static int FindWordBoundaryRight(string text, int position)
    {
        if (position >= text.Length)
        {
            return text.Length;
        }

        int i = position;
        while (i < text.Length && !char.IsWhiteSpace(text[i]))
        {
            i++;
        }
        while (i < text.Length && char.IsWhiteSpace(text[i]))
        {
            i++;
        }
        return i;
    }

    internal bool HaveInstructionsChanged()
    {
        return
            !CollectionsMarshal.AsSpan(_coloredRectangleInstructions).SequenceEqual(CollectionsMarshal.AsSpan(_previousColoredRectangleInstructions)) ||
            !CollectionsMarshal.AsSpan(_textureRegionInstructions).SequenceEqual(CollectionsMarshal.AsSpan(_previousTextureRegionInstructions));
    }

    internal void CycleInstructions()
    {
        (_coloredRectangleInstructions, _previousColoredRectangleInstructions) =
            (_previousColoredRectangleInstructions, _coloredRectangleInstructions);
        (_textureRegionInstructions, _previousTextureRegionInstructions) =
            (_previousTextureRegionInstructions, _textureRegionInstructions);

        _coloredRectangleInstructions.Clear();
        _textureRegionInstructions.Clear();
        _depth = 0;
    }
}

public static class PencilExtensions
{
    public static void Image(this Pencil pencil, SpriteAsset sprite, Color tint)
    {
        Vector2Int size = new Vector2Int(sprite.Size.X, sprite.Size.Y);
        Vector2Int position = pencil.CurrentPosition;
        Rectangle area = new Rectangle(position, size);
        pencil.AddTexture(sprite.Texture, area, sprite.CalculateTextureRegionUVs(), (FColor)tint);
        pencil.CurrentSize = size;
        pencil.CurrentPosition = pencil.DetermineNextPosition(size);
    }

    public static void Image(this Pencil pencil, SpriteAsset sprite, int width, int height, Color tint)
    {
        Vector2Int size = new Vector2Int(width, height);
        Vector2Int position = pencil.CurrentPosition;
        Rectangle area = new Rectangle(position, size);
        pencil.AddTexture(sprite.Texture, area, sprite.CalculateTextureRegionUVs(), (FColor)tint);
        pencil.CurrentSize = size;
        pencil.CurrentPosition = pencil.DetermineNextPosition(size);
    }

    public static CursorState Panel(this Pencil pencil, int width, int height, Color color)
    {
        Vector2Int size = new Vector2Int(width, height);
        Vector2Int position = pencil.CurrentPosition;
        Rectangle area = new Rectangle(position, size);
        pencil.AddRectangle(area, color);
        pencil.CurrentSize = size;
        pencil.CurrentPosition = pencil.DetermineNextPosition(size);

        pencil.AddHoverTest(area);
        pencil.AddHoverInTest(area);
        pencil.AddHoverOutTest(area);
        pencil.AddClickTest(area);

        if (!area.Intersects(pencil.CursorPosition))
        {
            return CursorState.None;
        }

        return pencil.CursorJustReleased ? CursorState.Clicked : CursorState.Hovered;
    }

    public static CursorState Button(this Pencil pencil, string text, Font font)
    {
        GuiStyle style = pencil.Style;

        Vector2Int size = pencil.MeasureText(text, font);
        Vector2Int padding = new Vector2Int(pencil.Style.TextPadding);

        Vector2Int fullSize = size + padding + padding;
        Vector2Int startPosition = pencil.DetermineNextPosition(fullSize);

        Vector2Int thickness = new Vector2Int(style.BorderThickness);
        Color innerColor = style.Background;

        Rectangle area = new Rectangle(startPosition, fullSize);

        pencil.AddHoverTest(area);
        pencil.AddHoverInTest(area);
        pencil.AddHoverOutTest(area);
        pencil.AddClickTest(area);

        if (!area.Intersects(pencil.CursorPosition))
        {
            return CursorState.None;
        }

        innerColor = style.ActiveColor;
        return pencil.CursorJustReleased ? CursorState.Clicked : CursorState.Hovered;
    }

    public static bool TextField(this Pencil pencil, int id, ref string value, Font font, int width)
    {
        GuiStyle style = pencil.Style;
        int padding = style.TextPadding;
        Vector2Int textSize = pencil.MeasureText("Ay", font);
        int height = textSize.Y + padding * 2;
        Vector2Int size = new Vector2Int(width, height);
        Vector2Int position = pencil.CurrentPosition;
        Rectangle area = new Rectangle(position, size);

        bool isFocused = pencil.IsFocused(id);
        bool committed = false;

        if (isFocused && pencil.EditingState != null)
        {
            if (pencil.EditingState.Committed)
            {
                value = pencil.EditingState.Buffer;
                committed = true;
                pencil.Blur();
                isFocused = false;
            }
            else if (pencil.EditingState.Canceled)
            {
                pencil.Blur();
                isFocused = false;
            }
        }

        if (pencil.CursorJustReleased && area.Intersects(pencil.CursorPosition))
        {
            if (!isFocused)
            {
                pencil.Focus(id, value);
                isFocused = true;
            }
        }

        Color bgColor = isFocused ? style.ActiveColor : style.Background;
        pencil.AddRectangle(area, bgColor);

        string displayText = isFocused && pencil.EditingState != null
            ? pencil.EditingState.Buffer
            : value;

        Vector2Int textPosition = new Vector2Int(position.X + padding, position.Y + padding);

        if (isFocused && pencil.EditingState != null && pencil.EditingState.HasSelection)
        {
            (int selStart, int selLength) = pencil.EditingState.GetSelectionRange();
            int selStartX = textPosition.X;
            if (selStart > 0)
            {
                Vector2Int beforeSelSize = pencil.MeasureText(displayText[..selStart], font);
                selStartX += beforeSelSize.X;
            }
            Vector2Int selTextSize = pencil.MeasureText(displayText.Substring(selStart, selLength), font);
            Rectangle selRect = new Rectangle(selStartX, position.Y + padding, selTextSize.X, textSize.Y);
            pencil.AddRectangle(selRect, style.SelectionColor);
        }

        if (displayText.Length > 0)
        {
            Vector2Int savedPosition = pencil.CurrentPosition;
            pencil.CurrentPosition = textPosition;
            pencil.Text(displayText, font, style.TextColor);
            pencil.CurrentPosition = savedPosition;
        }

        if (isFocused && pencil.EditingState != null)
        {
            int cursorX;
            if (pencil.EditingState.CursorPosition > 0 && displayText.Length > 0)
            {
                string beforeCursor = displayText[..pencil.EditingState.CursorPosition];
                Vector2Int beforeSize = pencil.MeasureText(beforeCursor, font);
                cursorX = textPosition.X + beforeSize.X;
            }
            else
            {
                cursorX = textPosition.X;
            }

            Rectangle cursorRect = new Rectangle(cursorX, position.Y + padding, 1, textSize.Y);
            pencil.AddRectangle(cursorRect, style.TextColor);
        }

        pencil.AddClickTest(area);

        pencil.CurrentSize = size;
        pencil.CurrentPosition = pencil.DetermineNextPosition(size);

        return committed;
    }
}

internal class TextFieldEditingState
{
    public string Buffer;
    public int CursorPosition;
    public int? SelectionAnchor;
    public bool Committed;
    public bool Canceled;

    public TextFieldEditingState(string initialValue)
    {
        Buffer = initialValue;
        CursorPosition = initialValue.Length;
    }

    public bool HasSelection => SelectionAnchor != null && SelectionAnchor.Value != CursorPosition;

    public (int Start, int Length) GetSelectionRange()
    {
        if (SelectionAnchor == null)
        {
            return (CursorPosition, 0);
        }

        int start = Math.Min(SelectionAnchor.Value, CursorPosition);
        int end = Math.Max(SelectionAnchor.Value, CursorPosition);
        return (start, end - start);
    }

    public string GetSelectedText()
    {
        (int start, int length) = GetSelectionRange();
        if (length == 0)
        {
            return string.Empty;
        }

        return Buffer.Substring(start, length);
    }

    public void DeleteSelection()
    {
        (int start, int length) = GetSelectionRange();
        if (length == 0)
        {
            return;
        }

        Buffer = Buffer.Remove(start, length);
        CursorPosition = start;
        SelectionAnchor = null;
    }
}
