using System.Numerics;

namespace GameKit.Sprites;

internal record SpriteDto
{
    public required string Filename { get; init; }
    
    public required Vector2 TopLeft { get; init; }
    public required Vector2 BottomRight { get; init; }
    public Vector2 Pivot { get; init; }
}

internal record AnimatedSpriteDto
{
    public required double FrameDuration { get; init; }
    public required string Image { get; init; }
    public required List<AnimationFrameDto> Frames { get; init; }
    public bool Repeat { get; init; } = false;
}

internal readonly record struct AnimationFrameDto
{
    public required Vector2 TopLeft { get; init; }
    public required Vector2 BottomRight { get; init; }
    public Vector2 Pivot { get; init; }
}
