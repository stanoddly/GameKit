namespace GameKit.Pencuil;

public readonly record struct Color(byte Red, byte Green, byte Blue, byte Alpha)
{
    public Color Invert()
    {
        return new Color((byte)(255 - Red), (byte)(255 - Green), (byte)(255 - Blue), Alpha);
    }
}

public static class Colors
{
    public static readonly Color White = new Color(255, 255, 255, 255);
    public static readonly Color Black = new Color(0, 0, 0, 255);
    public static readonly Color Red = new Color(255, 0, 0, 255);
    public static readonly Color Green = new Color(0, 255, 0, 255);
    public static readonly Color Blue = new Color(0, 0, 255, 255);
    public static readonly Color Silver = new Color(192, 192, 192, 255);
    public static readonly Color Gray = new Color(128, 128, 128, 255);
    public static readonly Color Transparent = new Color(0, 0, 0, 0);
}
