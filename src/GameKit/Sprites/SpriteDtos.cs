using System.Collections.Immutable;
using GameKit.Common;

namespace GameKit.Sprites;

internal record SpriteDto
{
    public required string Texture { get; init; }
    public required ShortRectangle TextureRegion { get; init; } 
}

internal record AnimatedSpriteDto
{
    public required double FrameDuration { get; init; }
    public required string Texture { get; init; }
    public required ImmutableArray<ShortRectangle> Frames { get; init; }
    public bool Repeat { get; init; } = false;
    public float[]? AnchorOffset { get; init; }
}
