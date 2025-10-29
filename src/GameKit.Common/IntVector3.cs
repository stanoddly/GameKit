using System.Numerics;

namespace GameKit.Common;

public readonly record struct IntVector3(int X, int Y, int Z)
{
    public IntVector3(int value) : this(value, value, value) { }

    public static implicit operator IntVector3((int x, int y, int z) position) => new IntVector3(position.x, position.y, position.z);
    
    public static explicit operator Vector3(IntVector3 iv) => new Vector3(iv.X, iv.Y, iv.Z);

    public static explicit operator IntVector3(Vector3 v) => new IntVector3((int)v.X, (int)v.Y, (int)v.Z);

    public static IntVector3 operator +(IntVector3 left, IntVector3 right) => new IntVector3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    public static IntVector3 operator -(IntVector3 left, IntVector3 right) => new IntVector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    public static IntVector3 operator *(IntVector3 left, int right) => new IntVector3(left.X * right, left.Y * right, left.Z * right);

    public static Vector3 Zero { get; } = default;

    public override string ToString() => $"({X}, {Y}, {Z})";
}
