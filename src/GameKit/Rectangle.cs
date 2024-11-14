namespace GameKit;

public readonly record struct Rectangle(int X, int Y, int Width, int Height)
{
    public (int, int) GetXY() => (X, Y);
    public (int, int) GetSize() => (Width, Height);
}
