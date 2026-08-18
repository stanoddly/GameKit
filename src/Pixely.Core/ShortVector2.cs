using System.Numerics;

namespace Pixely;

/// <summary>
/// Represents a 2D vector using short integers for X and Y coordinates.
/// Useful for compact position representations or when coordinate values
/// are known to be within the short integer range.
/// </summary>
public readonly record struct ShortVector2(short X, short Y)
{
    public ShortVector2(short value) : this(value, value)
    {
    }

    public ShortVector2(int x, int y) : this((short)x, (short)y)
    {
    }

    // Implicit conversion from ShortVector2 to Vector2
    public static implicit operator Vector2(ShortVector2 sv) => new Vector2(sv.X, sv.Y);
    
    public static implicit operator ShortVector2((short x, short y) position) => new ShortVector2(position.x, position.y);

    // Explicit conversion from Vector2 to ShortVector2 (could cause overflow)
    public static explicit operator ShortVector2(Vector2 v) => new ShortVector2((short)v.X, (short)v.Y);

    public static ShortVector2 operator +(ShortVector2 left, ShortVector2 right) => 
        new((short)(left.X + right.X), (short)(left.Y + right.Y));
    
    public static ShortVector2 operator -(ShortVector2 left, ShortVector2 right) => 
        new((short)(left.X - right.X), (short)(left.Y - right.Y));

    public static ShortVector2 operator *(ShortVector2 left, short right) => 
        new((short)(left.X * right), (short)(left.Y * right));

    public static Vector2 Zero { get; } = default;

    public override string ToString() => $"({X}, {Y})";
}
