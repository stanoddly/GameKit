using System.Collections.Immutable;
using System.Numerics;
using GameKit.Common;

namespace GameKit.Sprites;

internal record SpriteDto
{
    public required string Texture { get; init; }
    public required ShortRectangle TextureRegion { get; init; }
    public Vector2 AnchorOffset { get; init; } = Vector2.Zero;
    public SpriteFlip Flip { get; init; } = SpriteFlip.None;
}

internal record AnimatedSpriteDto
{
    public required double FrameDuration { get; init; }
    public required string Texture { get; init; }
    public required ImmutableArray<ShortRectangle> Frames { get; init; }
    public Vector2 AnchorOffset { get; init; } = Vector2.Zero;
    public SpriteFlip Flip { get; init; } = SpriteFlip.None;
}
