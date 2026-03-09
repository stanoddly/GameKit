namespace GameKit.Common;

[Flags]
public enum SpriteFlip
{
    None = 0,
    Horizontal = 1,
    Vertical = 2,
    Both = Horizontal | Vertical
}
