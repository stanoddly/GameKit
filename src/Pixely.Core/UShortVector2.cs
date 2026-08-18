using System.Numerics;

namespace Pixely;

public readonly record struct UShortVector2(ushort X, ushort Y)
{
    public UShortVector2(ushort value) : this(value, value)
    {
    }

    public UShortVector2(int x, int y) : this((ushort)x, (ushort)y)
    {
    }

    public static implicit operator Vector2(UShortVector2 sv) => new Vector2(sv.X, sv.Y);

    public static implicit operator UShortVector2((ushort x, ushort y) position) => new UShortVector2(position.x, position.y);

    public static explicit operator UShortVector2(Vector2 v) => new UShortVector2((ushort)v.X, (ushort)v.Y);

    public static UShortVector2 operator +(UShortVector2 left, UShortVector2 right) =>
        new((ushort)(left.X + right.X), (ushort)(left.Y + right.Y));

    public static UShortVector2 operator -(UShortVector2 left, UShortVector2 right) =>
        new((ushort)(left.X - right.X), (ushort)(left.Y - right.Y));

    public static UShortVector2 operator *(UShortVector2 left, ushort right) =>
        new((ushort)(left.X * right), (ushort)(left.Y * right));

    public static Vector2 Zero { get; } = default;

    public override string ToString() => $"({X}, {Y})";
}
