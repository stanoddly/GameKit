using System.Numerics;

namespace GameKit.Common;

public readonly record struct IntVector2(int X, int Y)
{
    public IntVector2(int value) : this(value, value)
    {
    }

    public static implicit operator Vector2(IntVector2 iv) => new Vector2(iv.X, iv.Y);

    public static implicit operator IntVector2((int x, int y) position) => new IntVector2(position.x, position.y);

    public static explicit operator IntVector2(Vector2 v) => new IntVector2((int)v.X, (int)v.Y);

    public static IntVector2 operator +(IntVector2 left, IntVector2 right) => new IntVector2(left.X + right.X, left.Y + right.Y);

    public static IntVector2 operator -(IntVector2 left, IntVector2 right) => new IntVector2(left.X - right.X, left.Y - right.Y);

    public static IntVector2 operator *(IntVector2 left, int right) => new IntVector2(left.X * right, left.Y * right);

    public static Vector2 Zero { get; } = default;

    public override string ToString() => $"({X}, {Y})";
}
