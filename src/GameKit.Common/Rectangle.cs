using System.Numerics;

namespace GameKit.Common;

public readonly record struct Rectangle(int X, int Y, int Width, int Height)
{
    public (int, int) GetXY() => (X, Y);
    public (int, int) GetSize() => (Width, Height);
}

public readonly record struct Rectangle<TType>(TType X, TType Y, TType Width, TType Height) where TType : unmanaged, INumberBase<TType>
{
    public (TType, TType) GetXY() => (X, Y);
    public (TType, TType) GetSize() => (Width, Height);

    public Size<TType> Size => new(Width, Height);
}

//[JsonConverter(typeof(SizeJsonConverter))]
public readonly record struct ShortRectangle(short X, short Y, short Width, short Height)
{
    public ShortRectangle(ShortVector2 position, ShortVector2 size) : this(position.X, position.Y, size.X, size.Y) { }
    public ShortVector2 Position => new ShortVector2(X, Y);
    public ShortVector2 Size => new ShortVector2(Width, Height);

    public bool Intersects(ShortVector2 point) => point.X >= X && point.X <= X + Width && point.Y >= Y && point.Y <= Y + Height;
}
