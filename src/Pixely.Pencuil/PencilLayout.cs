using System.Numerics;

namespace Pixely.Pencuil;

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

public readonly ref struct LayoutScope
{
    private readonly Pencil _pencil;
    private readonly int _frameIndex;

    internal LayoutScope(Pencil pencil, int frameIndex)
    {
        _pencil = pencil;
        _frameIndex = frameIndex;
    }

    public void Dispose()
    {
        _pencil.EndLayoutScope(_frameIndex);
    }
}

internal enum LayoutKind : byte
{
    Row,
    Column,
    Padding,
    Overlay,
    Sized
}

internal struct LayoutFrame
{
    internal LayoutKind Kind;
    internal Vector2Int Origin;
    internal Vector2Int PreviousPosition;
    internal Vector2Int PreviousSize;
    internal LayoutDirection PreviousDirection;
    internal int PreviousGap;
    internal int Gap;
    internal Insets Insets;
    internal Alignment Alignment;
    internal Vector2Int FixedSize;
    internal bool HasAvailableSize;
    internal Vector2Int AvailableSize;
    internal int ChildCount;
    internal Vector2Int ContentSize;
}

public partial class Pencil
{
    private readonly List<LayoutFrame> _layoutFrames = new();

    internal bool IsLayoutActive => _layoutFrames.Count > 0;

    public LayoutScope Row(int gap = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(gap);
        return BeginLayoutScope(LayoutKind.Row, gap, default, default, default);
    }

    public LayoutScope Column(int gap = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(gap);
        return BeginLayoutScope(LayoutKind.Column, gap, default, default, default);
    }

    public LayoutScope Padding(Insets insets)
    {
        if (insets.Left < 0 || insets.Top < 0 || insets.Right < 0 || insets.Bottom < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(insets));
        }

        return BeginLayoutScope(LayoutKind.Padding, 0, insets, default, default);
    }

    public LayoutScope Padding(int value)
    {
        return Padding(new Insets(value));
    }

    public LayoutScope Overlay(Alignment alignment = Alignment.TopLeft)
    {
        return BeginLayoutScope(LayoutKind.Overlay, 0, default, alignment, default);
    }

    public LayoutScope Sized(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(width);
        ArgumentOutOfRangeException.ThrowIfNegative(height);
        return BeginLayoutScope(LayoutKind.Sized, 0, default, default, new Vector2Int(width, height));
    }

    internal Vector2Int BeginLayoutElement(Vector2Int size)
    {
        if (_layoutFrames.Count == 0)
        {
            return CurrentPosition;
        }

        LayoutFrame frame = _layoutFrames[^1];
        if (frame.Kind != LayoutKind.Overlay)
        {
            return CurrentPosition;
        }

        return AlignWithin(frame.Origin, frame.AvailableSize, size, frame.Alignment);
    }

    internal void EndLayoutElement(Vector2Int position, Vector2Int size)
    {
        if (_layoutFrames.Count == 0)
        {
            CurrentSize = size;
            CurrentPosition = DetermineNextPosition(size);
            return;
        }

        int frameIndex = _layoutFrames.Count - 1;
        LayoutFrame frame = _layoutFrames[frameIndex];
        frame.ChildCount++;
        frame.ContentSize = frame.Kind switch
        {
            LayoutKind.Row => new Vector2Int(position.X - frame.Origin.X + size.X, Math.Max(frame.ContentSize.Y, size.Y)),
            LayoutKind.Column => new Vector2Int(Math.Max(frame.ContentSize.X, size.X), position.Y - frame.Origin.Y + size.Y),
            LayoutKind.Padding or LayoutKind.Sized => size,
            LayoutKind.Overlay => new Vector2Int(
                Math.Max(frame.ContentSize.X, position.X - frame.Origin.X + size.X),
                Math.Max(frame.ContentSize.Y, position.Y - frame.Origin.Y + size.Y)),
            _ => throw new InvalidOperationException($"Unsupported layout kind {frame.Kind}.")
        };
        _layoutFrames[frameIndex] = frame;

        CurrentSize = size;
        CurrentPosition = frame.Kind switch
        {
            LayoutKind.Row => new Vector2Int(frame.Origin.X + frame.ContentSize.X + frame.Gap, frame.Origin.Y),
            LayoutKind.Column => new Vector2Int(frame.Origin.X, frame.Origin.Y + frame.ContentSize.Y + frame.Gap),
            LayoutKind.Padding => new Vector2Int(frame.Origin.X + frame.Insets.Left, frame.Origin.Y + frame.Insets.Top),
            _ => frame.Origin
        };
    }

    internal void EndLayoutScope(int frameIndex)
    {
        if (frameIndex != _layoutFrames.Count - 1)
        {
            throw new InvalidOperationException("Layout scopes must be disposed in reverse order.");
        }

        LayoutFrame frame = _layoutFrames[frameIndex];
        ValidateChildCount(frame);
        Vector2Int size = frame.Kind switch
        {
            LayoutKind.Padding => new Vector2Int(frame.ContentSize.X + frame.Insets.Left + frame.Insets.Right, frame.ContentSize.Y + frame.Insets.Top + frame.Insets.Bottom),
            LayoutKind.Sized => frame.FixedSize,
            LayoutKind.Overlay when frame.HasAvailableSize => frame.AvailableSize,
            _ => frame.ContentSize
        };

        _layoutFrames.RemoveAt(frameIndex);
        CurrentPosition = frame.PreviousPosition;
        CurrentSize = frame.PreviousSize;
        CurrentDirection = frame.PreviousDirection;
        CurrentGap = frame.PreviousGap;
        EndLayoutElement(frame.Origin, size);
    }

    internal void ValidateLayoutBuild()
    {
        if (_layoutFrames.Count > 0)
        {
            throw new InvalidOperationException("A layout scope was not disposed before the build completed.");
        }
    }

    private LayoutScope BeginLayoutScope(LayoutKind kind, int gap, Insets insets, Alignment alignment, Vector2Int fixedSize)
    {
        bool hasParentAvailableSize = TryGetAvailableSize(out Vector2Int parentAvailableSize);
        bool hasAvailableSize = kind == LayoutKind.Sized || hasParentAvailableSize && (kind == LayoutKind.Padding || kind == LayoutKind.Overlay);
        Vector2Int availableSize = kind switch
        {
            LayoutKind.Sized => fixedSize,
            LayoutKind.Padding when hasParentAvailableSize => new Vector2Int(
                Math.Max(0, parentAvailableSize.X - insets.Left - insets.Right),
                Math.Max(0, parentAvailableSize.Y - insets.Top - insets.Bottom)),
            LayoutKind.Overlay when hasParentAvailableSize => parentAvailableSize,
            _ => default
        };

        if (kind == LayoutKind.Overlay && alignment != Alignment.TopLeft && !hasAvailableSize)
        {
            throw new InvalidOperationException("An aligned Overlay requires bounds from a Sized or Padding scope.");
        }

        bool hasKnownSize = kind == LayoutKind.Sized || kind == LayoutKind.Overlay && hasAvailableSize;
        Vector2Int knownSize = kind == LayoutKind.Sized ? fixedSize : availableSize;
        EnsureParentCanPositionScope(hasKnownSize);
        Vector2Int origin = hasKnownSize ? BeginLayoutElement(knownSize) : CurrentPosition;
        int frameIndex = _layoutFrames.Count;
        LayoutFrame frame = new()
        {
            Kind = kind,
            Origin = origin,
            PreviousPosition = CurrentPosition,
            PreviousSize = CurrentSize,
            PreviousDirection = CurrentDirection,
            PreviousGap = CurrentGap,
            Gap = gap,
            Insets = insets,
            Alignment = alignment,
            FixedSize = fixedSize,
            HasAvailableSize = hasAvailableSize,
            AvailableSize = availableSize
        };
        _layoutFrames.Add(frame);
        CurrentPosition = kind == LayoutKind.Padding ? origin + new Vector2Int(insets.Left, insets.Top) : origin;
        CurrentSize = default;
        return new LayoutScope(this, frameIndex);
    }

    private bool TryGetAvailableSize(out Vector2Int availableSize)
    {
        if (_layoutFrames.Count > 0)
        {
            LayoutFrame parent = _layoutFrames[^1];
            if (parent.Kind == LayoutKind.Sized || parent.HasAvailableSize && (parent.Kind == LayoutKind.Padding || parent.Kind == LayoutKind.Overlay))
            {
                availableSize = parent.Kind == LayoutKind.Sized ? parent.FixedSize : parent.AvailableSize;
                return true;
            }
        }

        availableSize = default;
        return false;
    }

    private void EnsureParentCanPositionScope(bool hasKnownSize)
    {
        if (_layoutFrames.Count == 0)
        {
            return;
        }

        LayoutFrame parent = _layoutFrames[^1];
        if (parent.Kind == LayoutKind.Overlay && parent.Alignment != Alignment.TopLeft && !hasKnownSize)
        {
            throw new InvalidOperationException("An aligned Overlay can contain only controls or fixed-size scopes.");
        }
    }

    private static void ValidateChildCount(LayoutFrame frame)
    {
        if (frame.Kind != LayoutKind.Padding && frame.Kind != LayoutKind.Sized)
        {
            return;
        }

        if (frame.ChildCount != 1)
        {
            throw new InvalidOperationException($"{frame.Kind} must contain exactly one direct child.");
        }
    }

    private static Vector2Int AlignWithin(Vector2Int origin, Vector2Int availableSize, Vector2Int size, Alignment alignment)
    {
        int x = alignment switch
        {
            Alignment.TopCenter or Alignment.Center or Alignment.BottomCenter => origin.X + (availableSize.X - size.X) / 2,
            Alignment.TopRight or Alignment.CenterRight or Alignment.BottomRight => origin.X + availableSize.X - size.X,
            _ => origin.X
        };
        int y = alignment switch
        {
            Alignment.CenterLeft or Alignment.Center or Alignment.CenterRight => origin.Y + (availableSize.Y - size.Y) / 2,
            Alignment.BottomLeft or Alignment.BottomCenter or Alignment.BottomRight => origin.Y + availableSize.Y - size.Y,
            _ => origin.Y
        };
        return new Vector2Int(x, y);
    }
}
