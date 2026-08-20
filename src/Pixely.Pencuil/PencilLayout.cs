using System.Numerics;
using Pixely.Gpu;

namespace Pixely.Pencuil;

public enum CrossAxisAlignment
{
    Start,
    Center,
    End,
    Stretch
}

public enum Alignment
{
    TopLeft,
    TopCenter,
    TopRight,
    CenterLeft,
    Center,
    CenterRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}

public readonly record struct Insets(int Left, int Top, int Right, int Bottom)
{
    public Insets(int value)
        : this(value, value, value, value)
    {
    }

    public Insets(int horizontal, int vertical)
        : this(horizontal, vertical, horizontal, vertical)
    {
    }
}

public readonly record struct AxisSize
{
    private readonly AxisSizeKind _kind;
    private readonly int _value;

    private AxisSize(AxisSizeKind kind, int value)
    {
        _kind = kind;
        _value = value;
    }

    public static AxisSize Content => default;
    public static AxisSize Fill { get; } = new(AxisSizeKind.Fill, 0);

    public static AxisSize Fixed(int value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        return new AxisSize(AxisSizeKind.Fixed, value);
    }

    public static implicit operator AxisSize(int value) => Fixed(value);

    internal AxisSizeKind Kind => _kind;
    internal int Value => _value;
}

public readonly ref struct LayoutScope
{
    private readonly Pencil _pencil;
    private readonly int _entryIndex;

    internal LayoutScope(Pencil pencil, int entryIndex)
    {
        _pencil = pencil;
        _entryIndex = entryIndex;
    }

    public void Dispose()
    {
        _pencil.EndLayoutScope(_entryIndex);
    }
}

internal enum AxisSizeKind : byte
{
    Content,
    Fill,
    Fixed
}

internal enum LayoutKind : byte
{
    Row,
    Column,
    Padding,
    Align,
    Overlay,
    Sized,
    Expanded,
    Leaf
}

internal enum LayoutVisualKind : byte
{
    None,
    Rectangle,
    Texture
}

internal struct LayoutEntry
{
    internal LayoutKind Kind;
    internal int Parent;
    internal int FirstChild;
    internal int LastChild;
    internal int NextSibling;
    internal int SubtreeLimit;
    internal int Gap;
    internal int Flex;
    internal AxisSize Width;
    internal AxisSize Height;
    internal CrossAxisAlignment CrossAxisAlignment;
    internal Alignment Alignment;
    internal Insets Insets;
    internal Vector2Int IntrinsicSize;
    internal Vector2Int MeasuredSize;
    internal Rectangle ArrangedBounds;
    internal Vector2Int RootPosition;
    internal LayoutVisualKind VisualKind;
    internal Color Color;
    internal Texture? Texture;
    internal Vector4 Uvs;
    internal FColor Tint;
    internal object? ControlView;
    internal int ControlSequence;
}

internal readonly record struct LayoutHitArea(
    object View,
    int ControlSequence,
    Rectangle Area);

public partial class Pencil
{
    private readonly List<LayoutEntry> _layoutEntries = new();
    private List<LayoutHitArea> _layoutHitAreas = new();
    private List<LayoutHitArea> _previousLayoutHitAreas = new();
    private int _currentLayoutEntry = -1;
    private object? _currentLayoutView;
    private int _nextLayoutControlSequence;

    internal bool IsLayoutActive => _currentLayoutEntry >= 0;

    public LayoutScope Row(
        int gap = 0,
        AxisSize width = default,
        AxisSize height = default,
        CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.Start)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(gap);
        return BeginLayoutScope(
            LayoutKind.Row,
            gap,
            width,
            height,
            crossAxisAlignment,
            default,
            default,
            0);
    }

    public LayoutScope Column(
        int gap = 0,
        AxisSize width = default,
        AxisSize height = default,
        CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.Start)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(gap);
        return BeginLayoutScope(
            LayoutKind.Column,
            gap,
            width,
            height,
            crossAxisAlignment,
            default,
            default,
            0);
    }

    public LayoutScope Padding(Insets insets)
    {
        if (insets.Left < 0 || insets.Top < 0 || insets.Right < 0 || insets.Bottom < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(insets));
        }

        return BeginLayoutScope(
            LayoutKind.Padding,
            0,
            default,
            default,
            default,
            default,
            insets,
            0);
    }

    public LayoutScope Padding(int value) => Padding(new Insets(value));

    public LayoutScope Align(Alignment alignment)
    {
        return BeginLayoutScope(
            LayoutKind.Align,
            0,
            default,
            default,
            default,
            alignment,
            default,
            0);
    }

    public LayoutScope Overlay(Alignment alignment = Alignment.TopLeft)
    {
        return BeginLayoutScope(
            LayoutKind.Overlay,
            0,
            default,
            default,
            default,
            alignment,
            default,
            0);
    }

    public LayoutScope Sized(AxisSize width = default, AxisSize height = default)
    {
        return BeginLayoutScope(
            LayoutKind.Sized,
            0,
            width,
            height,
            default,
            default,
            default,
            0);
    }

    public LayoutScope Expanded(int flex = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(flex);
        if (_currentLayoutEntry < 0)
        {
            throw new InvalidOperationException("Expanded must be a direct child of a Row or Column.");
        }

        LayoutKind parentKind = _layoutEntries[_currentLayoutEntry].Kind;
        if (parentKind != LayoutKind.Row && parentKind != LayoutKind.Column)
        {
            throw new InvalidOperationException("Expanded must be a direct child of a Row or Column.");
        }

        return BeginLayoutScope(
            LayoutKind.Expanded,
            0,
            default,
            default,
            default,
            default,
            default,
            flex);
    }

    internal void BeginLayoutView(object view)
    {
        if (_currentLayoutEntry >= 0)
        {
            throw new InvalidOperationException("A layout scope was left open by the previous view.");
        }

        _currentLayoutView = view;
        _nextLayoutControlSequence = 0;
    }

    internal void EndLayoutView(object view)
    {
        if (!ReferenceEquals(_currentLayoutView, view))
        {
            throw new InvalidOperationException("Pencuil views must end in the order they began.");
        }

        if (_currentLayoutEntry >= 0)
        {
            throw new InvalidOperationException("A layout scope was not disposed before the view completed.");
        }

        _currentLayoutView = null;
    }

    internal void ResetLayoutBuild()
    {
        if (_currentLayoutEntry >= 0)
        {
            throw new InvalidOperationException("A layout scope was left open by the previous build.");
        }

        _layoutEntries.Clear();
        _layoutHitAreas.Clear();
        _currentLayoutView = null;
        _nextLayoutControlSequence = 0;
    }

    internal void CompleteLayoutBuild()
    {
        if (_currentLayoutEntry >= 0)
        {
            throw new InvalidOperationException("A layout scope was not disposed before the build completed.");
        }

        (_layoutHitAreas, _previousLayoutHitAreas) =
            (_previousLayoutHitAreas, _layoutHitAreas);
        _layoutHitAreas.Clear();
        _layoutEntries.Clear();
    }

    internal bool IsOverLayoutInteractiveArea(Vector2Int position)
    {
        foreach (LayoutHitArea hitArea in _previousLayoutHitAreas)
        {
            if (hitArea.Area.Intersects(position))
            {
                return true;
            }
        }

        return false;
    }

    internal bool TryAddLayoutRectangle(
        Vector2Int size,
        Color color,
        bool interactive,
        out CursorState cursorState)
    {
        if (!IsLayoutActive)
        {
            cursorState = default;
            return false;
        }

        object? controlView = null;
        int controlSequence = -1;
        cursorState = default;
        if (interactive)
        {
            controlView = _currentLayoutView ?? this;
            controlSequence = _nextLayoutControlSequence++;
            cursorState = GetLayoutCursorState(controlView, controlSequence);
        }

        AddLayoutLeaf(
            size,
            LayoutVisualKind.Rectangle,
            color,
            null,
            default,
            default,
            controlView,
            controlSequence);
        return true;
    }

    internal bool TryAddLayoutTexture(
        Vector2Int size,
        Texture texture,
        Vector4 uvs,
        FColor tint)
    {
        if (!IsLayoutActive)
        {
            return false;
        }

        AddLayoutLeaf(
            size,
            LayoutVisualKind.Texture,
            default,
            texture,
            uvs,
            tint,
            null,
            -1);
        return true;
    }

    internal void EndLayoutScope(int entryIndex)
    {
        if (_currentLayoutEntry != entryIndex)
        {
            throw new InvalidOperationException("Layout scopes must be disposed in reverse order.");
        }

        LayoutEntry entry = _layoutEntries[entryIndex];
        ValidateChildCount(entry);
        entry.SubtreeLimit = _layoutEntries.Count;
        _layoutEntries[entryIndex] = entry;
        _currentLayoutEntry = entry.Parent;

        if (entry.Parent >= 0)
        {
            return;
        }

        Vector2Int availableSize = new(
            Math.Max(0, _viewportWidth - entry.RootPosition.X),
            Math.Max(0, _viewportHeight - entry.RootPosition.Y));
        Vector2Int measuredSize = Measure(entryIndex, availableSize);
        Arrange(
            entryIndex,
            new Rectangle(entry.RootPosition, measuredSize));
        Emit(entryIndex, entry.SubtreeLimit);

        CurrentPosition = entry.RootPosition;
        CurrentSize = measuredSize;
        CurrentPosition = DetermineNextPosition(measuredSize);
    }

    private LayoutScope BeginLayoutScope(
        LayoutKind kind,
        int gap,
        AxisSize width,
        AxisSize height,
        CrossAxisAlignment crossAxisAlignment,
        Alignment alignment,
        Insets insets,
        int flex)
    {
        int parent = _currentLayoutEntry;
        int entryIndex = _layoutEntries.Count;
        LayoutEntry entry = new()
        {
            Kind = kind,
            Parent = parent,
            FirstChild = -1,
            LastChild = -1,
            NextSibling = -1,
            SubtreeLimit = -1,
            Gap = gap,
            Flex = flex,
            Width = width,
            Height = height,
            CrossAxisAlignment = crossAxisAlignment,
            Alignment = alignment,
            Insets = insets,
            RootPosition = parent < 0 ? CurrentPosition : default,
            ControlSequence = -1
        };
        _layoutEntries.Add(entry);
        AddChild(parent, entryIndex);
        _currentLayoutEntry = entryIndex;
        return new LayoutScope(this, entryIndex);
    }

    private void AddLayoutLeaf(
        Vector2Int size,
        LayoutVisualKind visualKind,
        Color color,
        Texture? texture,
        Vector4 uvs,
        FColor tint,
        object? controlView,
        int controlSequence)
    {
        int entryIndex = _layoutEntries.Count;
        LayoutEntry entry = new()
        {
            Kind = LayoutKind.Leaf,
            Parent = _currentLayoutEntry,
            FirstChild = -1,
            LastChild = -1,
            NextSibling = -1,
            SubtreeLimit = entryIndex + 1,
            IntrinsicSize = size,
            VisualKind = visualKind,
            Color = color,
            Texture = texture,
            Uvs = uvs,
            Tint = tint,
            ControlView = controlView,
            ControlSequence = controlSequence
        };
        _layoutEntries.Add(entry);
        AddChild(_currentLayoutEntry, entryIndex);
    }

    private void AddChild(int parentIndex, int childIndex)
    {
        if (parentIndex < 0)
        {
            return;
        }

        LayoutEntry parent = _layoutEntries[parentIndex];
        if (parent.LastChild < 0)
        {
            parent.FirstChild = childIndex;
        }
        else
        {
            LayoutEntry sibling = _layoutEntries[parent.LastChild];
            sibling.NextSibling = childIndex;
            _layoutEntries[parent.LastChild] = sibling;
        }

        parent.LastChild = childIndex;
        _layoutEntries[parentIndex] = parent;
    }

    private void ValidateChildCount(LayoutEntry entry)
    {
        if (entry.Kind != LayoutKind.Padding
            && entry.Kind != LayoutKind.Align
            && entry.Kind != LayoutKind.Sized
            && entry.Kind != LayoutKind.Expanded)
        {
            return;
        }

        if (entry.FirstChild < 0 || entry.FirstChild != entry.LastChild)
        {
            throw new InvalidOperationException($"{entry.Kind} must contain exactly one direct child.");
        }
    }

    private CursorState GetLayoutCursorState(object view, int controlSequence)
    {
        for (int i = _previousLayoutHitAreas.Count - 1; i >= 0; i--)
        {
            LayoutHitArea hitArea = _previousLayoutHitAreas[i];
            if (ReferenceEquals(hitArea.View, view)
                && hitArea.ControlSequence == controlSequence
                && hitArea.Area.Intersects(CursorPosition))
            {
                return CursorJustReleased
                    ? CursorState.Clicked
                    : CursorState.Hovered;
            }
        }

        return CursorState.None;
    }

    private Vector2Int Measure(int entryIndex, Vector2Int availableSize)
    {
        LayoutEntry entry = _layoutEntries[entryIndex];
        Vector2Int naturalSize = entry.Kind switch
        {
            LayoutKind.Leaf => entry.IntrinsicSize,
            LayoutKind.Row => MeasureLinear(entryIndex, availableSize, true),
            LayoutKind.Column => MeasureLinear(entryIndex, availableSize, false),
            LayoutKind.Padding => MeasurePadding(entry, availableSize),
            LayoutKind.Align => MeasureSingleChild(entry, availableSize),
            LayoutKind.Overlay => MeasureOverlay(entry, availableSize),
            LayoutKind.Sized => MeasureSized(entry, availableSize),
            LayoutKind.Expanded => MeasureSingleChild(entry, availableSize),
            _ => throw new InvalidOperationException($"Unsupported layout kind {entry.Kind}.")
        };

        entry = _layoutEntries[entryIndex];
        entry.MeasuredSize = new Vector2Int(
            ResolveAxisSize(entry.Width, naturalSize.X, availableSize.X),
            ResolveAxisSize(entry.Height, naturalSize.Y, availableSize.Y));
        _layoutEntries[entryIndex] = entry;
        return entry.MeasuredSize;
    }

    private Vector2Int MeasureLinear(
        int entryIndex,
        Vector2Int availableSize,
        bool horizontal)
    {
        LayoutEntry entry = _layoutEntries[entryIndex];
        int availableMain = horizontal
            ? ResolveAvailableAxis(entry.Width, availableSize.X)
            : ResolveAvailableAxis(entry.Height, availableSize.Y);
        int availableCross = horizontal
            ? ResolveAvailableAxis(entry.Height, availableSize.Y)
            : ResolveAvailableAxis(entry.Width, availableSize.X);
        int childCount = 0;
        int totalFlex = 0;
        int fixedMain = 0;
        int maxCross = 0;

        for (int childIndex = entry.FirstChild; childIndex >= 0; childIndex = _layoutEntries[childIndex].NextSibling)
        {
            childCount++;
            LayoutEntry child = _layoutEntries[childIndex];
            if (child.Kind == LayoutKind.Expanded)
            {
                totalFlex += child.Flex;
                continue;
            }

            Vector2Int childAvailable = horizontal
                ? new Vector2Int(availableMain, availableCross)
                : new Vector2Int(availableCross, availableMain);
            Vector2Int childSize = Measure(childIndex, childAvailable);
            fixedMain += horizontal ? childSize.X : childSize.Y;
            maxCross = Math.Max(maxCross, horizontal ? childSize.Y : childSize.X);
        }

        int totalGap = Math.Max(0, childCount - 1) * entry.Gap;
        int remaining = Math.Max(0, availableMain - fixedMain - totalGap);
        int allocated = 0;
        int accumulatedFlex = 0;

        if (totalFlex > 0)
        {
            for (int childIndex = entry.FirstChild; childIndex >= 0; childIndex = _layoutEntries[childIndex].NextSibling)
            {
                LayoutEntry child = _layoutEntries[childIndex];
                if (child.Kind != LayoutKind.Expanded)
                {
                    continue;
                }

                accumulatedFlex += child.Flex;
                int allocationLimit = (int)((long)remaining * accumulatedFlex / totalFlex);
                int childMain = allocationLimit - allocated;
                allocated = allocationLimit;
                Vector2Int childAvailable = horizontal
                    ? new Vector2Int(childMain, availableCross)
                    : new Vector2Int(availableCross, childMain);
                Vector2Int childSize = Measure(childIndex, childAvailable);
                child = _layoutEntries[childIndex];
                child.MeasuredSize = horizontal
                    ? new Vector2Int(childMain, childSize.Y)
                    : new Vector2Int(childSize.X, childMain);
                _layoutEntries[childIndex] = child;
                maxCross = Math.Max(maxCross, horizontal ? childSize.Y : childSize.X);
            }
        }

        int main = fixedMain + totalGap + allocated;
        return horizontal
            ? new Vector2Int(main, maxCross)
            : new Vector2Int(maxCross, main);
    }

    private Vector2Int MeasurePadding(LayoutEntry entry, Vector2Int availableSize)
    {
        int horizontalInsets = entry.Insets.Left + entry.Insets.Right;
        int verticalInsets = entry.Insets.Top + entry.Insets.Bottom;
        Vector2Int childSize = Measure(
            entry.FirstChild,
            new Vector2Int(
                Math.Max(0, availableSize.X - horizontalInsets),
                Math.Max(0, availableSize.Y - verticalInsets)));
        return new Vector2Int(
            childSize.X + horizontalInsets,
            childSize.Y + verticalInsets);
    }

    private Vector2Int MeasureSingleChild(LayoutEntry entry, Vector2Int availableSize)
    {
        return Measure(entry.FirstChild, availableSize);
    }

    private Vector2Int MeasureOverlay(LayoutEntry entry, Vector2Int availableSize)
    {
        Vector2Int result = default;
        for (int childIndex = entry.FirstChild; childIndex >= 0; childIndex = _layoutEntries[childIndex].NextSibling)
        {
            Vector2Int childSize = Measure(childIndex, availableSize);
            result = new Vector2Int(
                Math.Max(result.X, childSize.X),
                Math.Max(result.Y, childSize.Y));
        }

        return result;
    }

    private Vector2Int MeasureSized(LayoutEntry entry, Vector2Int availableSize)
    {
        Vector2Int childAvailable = new(
            ResolveAvailableAxis(entry.Width, availableSize.X),
            ResolveAvailableAxis(entry.Height, availableSize.Y));
        return Measure(entry.FirstChild, childAvailable);
    }

    private void Arrange(int entryIndex, Rectangle bounds)
    {
        LayoutEntry entry = _layoutEntries[entryIndex];
        entry.ArrangedBounds = bounds;
        _layoutEntries[entryIndex] = entry;

        switch (entry.Kind)
        {
            case LayoutKind.Leaf:
                return;
            case LayoutKind.Row:
                ArrangeLinear(entry, bounds, true);
                return;
            case LayoutKind.Column:
                ArrangeLinear(entry, bounds, false);
                return;
            case LayoutKind.Padding:
                ArrangePadding(entry, bounds);
                return;
            case LayoutKind.Align:
                ArrangeAlignedChild(entry.FirstChild, bounds, entry.Alignment);
                return;
            case LayoutKind.Overlay:
                ArrangeOverlay(entry, bounds);
                return;
            case LayoutKind.Sized:
            case LayoutKind.Expanded:
                Arrange(entry.FirstChild, bounds);
                return;
            default:
                throw new InvalidOperationException($"Unsupported layout kind {entry.Kind}.");
        }
    }

    private void ArrangeLinear(LayoutEntry entry, Rectangle bounds, bool horizontal)
    {
        int childCount = 0;
        int totalFlex = 0;
        int fixedMain = 0;
        for (int childIndex = entry.FirstChild; childIndex >= 0; childIndex = _layoutEntries[childIndex].NextSibling)
        {
            childCount++;
            LayoutEntry child = _layoutEntries[childIndex];
            if (child.Kind == LayoutKind.Expanded)
            {
                totalFlex += child.Flex;
            }
            else
            {
                fixedMain += horizontal ? child.MeasuredSize.X : child.MeasuredSize.Y;
            }
        }

        int availableMain = horizontal ? bounds.Width : bounds.Height;
        int totalGap = Math.Max(0, childCount - 1) * entry.Gap;
        int remaining = Math.Max(0, availableMain - fixedMain - totalGap);
        int position = horizontal ? bounds.X : bounds.Y;
        int allocated = 0;
        int accumulatedFlex = 0;

        for (int childIndex = entry.FirstChild; childIndex >= 0; childIndex = _layoutEntries[childIndex].NextSibling)
        {
            LayoutEntry child = _layoutEntries[childIndex];
            int childMain;
            if (child.Kind == LayoutKind.Expanded)
            {
                accumulatedFlex += child.Flex;
                int allocationLimit = (int)((long)remaining * accumulatedFlex / totalFlex);
                childMain = allocationLimit - allocated;
                allocated = allocationLimit;
            }
            else
            {
                childMain = horizontal ? child.MeasuredSize.X : child.MeasuredSize.Y;
            }

            int measuredCross = horizontal ? child.MeasuredSize.Y : child.MeasuredSize.X;
            int availableCross = horizontal ? bounds.Height : bounds.Width;
            int childCross = entry.CrossAxisAlignment == CrossAxisAlignment.Stretch
                ? availableCross
                : measuredCross;
            int crossPosition = CalculateCrossPosition(
                horizontal ? bounds.Y : bounds.X,
                availableCross,
                childCross,
                entry.CrossAxisAlignment);
            Rectangle childBounds = horizontal
                ? new Rectangle(position, crossPosition, childMain, childCross)
                : new Rectangle(crossPosition, position, childCross, childMain);
            Arrange(childIndex, childBounds);
            position += childMain + entry.Gap;
        }
    }

    private void ArrangePadding(LayoutEntry entry, Rectangle bounds)
    {
        LayoutEntry child = _layoutEntries[entry.FirstChild];
        Arrange(
            entry.FirstChild,
            new Rectangle(
                bounds.X + entry.Insets.Left,
                bounds.Y + entry.Insets.Top,
                child.MeasuredSize.X,
                child.MeasuredSize.Y));
    }

    private void ArrangeOverlay(LayoutEntry entry, Rectangle bounds)
    {
        for (int childIndex = entry.FirstChild; childIndex >= 0; childIndex = _layoutEntries[childIndex].NextSibling)
        {
            ArrangeAlignedChild(childIndex, bounds, entry.Alignment);
        }
    }

    private void ArrangeAlignedChild(int childIndex, Rectangle bounds, Alignment alignment)
    {
        Vector2Int childSize = _layoutEntries[childIndex].MeasuredSize;
        int x = alignment switch
        {
            Alignment.TopCenter or Alignment.Center or Alignment.BottomCenter =>
                bounds.X + (bounds.Width - childSize.X) / 2,
            Alignment.TopRight or Alignment.CenterRight or Alignment.BottomRight =>
                bounds.X + bounds.Width - childSize.X,
            _ => bounds.X
        };
        int y = alignment switch
        {
            Alignment.CenterLeft or Alignment.Center or Alignment.CenterRight =>
                bounds.Y + (bounds.Height - childSize.Y) / 2,
            Alignment.BottomLeft or Alignment.BottomCenter or Alignment.BottomRight =>
                bounds.Y + bounds.Height - childSize.Y,
            _ => bounds.Y
        };
        Arrange(childIndex, new Rectangle(x, y, childSize.X, childSize.Y));
    }

    private void Emit(int rootIndex, int subtreeLimit)
    {
        for (int entryIndex = rootIndex; entryIndex < subtreeLimit; entryIndex++)
        {
            LayoutEntry entry = _layoutEntries[entryIndex];
            if (entry.VisualKind == LayoutVisualKind.Rectangle)
            {
                AddRectangle(entry.ArrangedBounds, entry.Color);
            }
            else if (entry.VisualKind == LayoutVisualKind.Texture)
            {
                AddTexture(
                    entry.Texture!,
                    entry.ArrangedBounds,
                    entry.Uvs,
                    entry.Tint);
            }

            if (entry.ControlView != null)
            {
                _layoutHitAreas.Add(new LayoutHitArea(
                    entry.ControlView,
                    entry.ControlSequence,
                    entry.ArrangedBounds));
            }
        }
    }

    private static int ResolveAxisSize(AxisSize size, int content, int available)
    {
        return size.Kind switch
        {
            AxisSizeKind.Content => content,
            AxisSizeKind.Fill => available,
            AxisSizeKind.Fixed => size.Value,
            _ => throw new InvalidOperationException($"Unsupported axis size {size.Kind}.")
        };
    }

    private static int ResolveAvailableAxis(AxisSize size, int available)
    {
        return size.Kind == AxisSizeKind.Fixed ? size.Value : available;
    }

    private static int CalculateCrossPosition(
        int start,
        int available,
        int child,
        CrossAxisAlignment alignment)
    {
        return alignment switch
        {
            CrossAxisAlignment.Center => start + (available - child) / 2,
            CrossAxisAlignment.End => start + available - child,
            _ => start
        };
    }
}
