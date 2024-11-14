using System.Collections.Immutable;
using System.Numerics;

namespace GameKit.Sprites;

internal record SpriteDto
{
    public required string Image { get; init; }
    public required Rectangle SourceRectangle { get; init; } 
}

internal record AnimatedSpriteDto
{
    public required double FrameDuration { get; init; }
    public required string Image { get; init; }
    public required ImmutableArray<Rectangle> Frames { get; init; }
    public bool Repeat { get; init; } = false;
}
