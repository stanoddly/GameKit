using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SDL;

namespace Pixely.Gpu;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct Color(byte Red, byte Green, byte Blue, byte Alpha)
{
    public static implicit operator SDL_Color(Color color) => Unsafe.As<Color, SDL_Color>(ref color);
    public static implicit operator Color(SDL_Color color) => Unsafe.As<SDL_Color, Color>(ref color);

    public static Color FromRgb(uint value)
    {
        return new Color(
            (byte)((value >> 16) & 0xFF),
            (byte)((value >> 8) & 0xFF),
            (byte)(value & 0xFF),
            255
        );
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct FColor(float Red, float Green, float Blue, float Alpha)
{
    public float R => Red;
    public float G => Green;
    public float B => Blue;
    public float A => Alpha;
    
    public static implicit operator SDL_FColor(FColor color) => Unsafe.As<FColor, SDL_FColor>(ref color);
    public static implicit operator FColor(SDL_FColor color) => Unsafe.As<SDL_FColor, FColor>(ref color);
    
    public static explicit operator FColor(Color color) => new FColor(color.Red / 255f, color.Green / 255f, color.Blue / 255f, color.Alpha / 255f);
}

public static class Colors
{
    public static readonly Color AliceBlue = new(240, 248, 255, 255);
    public static readonly Color AntiqueWhite = new(250, 235, 215, 255);
    public static readonly Color Aqua = new(0, 255, 255, 255);
    public static readonly Color Aquamarine = new(127, 255, 212, 255);
    public static readonly Color Azure = new(240, 255, 255, 255);
    public static readonly Color Beige = new(245, 245, 220, 255);
    public static readonly Color Black = new(0, 0, 0, 255);
    public static readonly Color Blue = new(0, 0, 255, 255);
    public static readonly Color Brown = new(165, 42, 42, 255);
    public static readonly Color Coral = new(255, 127, 80, 255);
    public static readonly Color Crimson = new(220, 20, 60, 255);
    public static readonly Color Cyan = new(0, 255, 255, 255);
    public static readonly Color DarkBlue = new(0, 0, 139, 255);
    public static readonly Color DarkGrey = new(169, 169, 169, 255);
    public static readonly Color DarkGreen = new(0, 100, 0, 255);
    public static readonly Color DarkRed = new(139, 0, 0, 255);
    public static readonly Color DeepPink = new(255, 20, 147, 255);
    public static readonly Color ForestGreen = new(34, 139, 34, 255);
    public static readonly Color Gold = new(255, 215, 0, 255);
    public static readonly Color Grey = new(128, 128, 128, 255);
    public static readonly Color Green = new(0, 255, 0, 255);
    public static readonly Color HotPink = new(255, 105, 180, 255);
    public static readonly Color Indigo = new(75, 0, 130, 255);
    public static readonly Color Ivory = new(255, 255, 240, 255);
    public static readonly Color LightBlue = new(173, 216, 230, 255);
    public static readonly Color LightGrey = new(211, 211, 211, 255);
    public static readonly Color LightGreen = new(144, 238, 144, 255);
    public static readonly Color LightPink = new(255, 182, 193, 255);
    public static readonly Color Lime = new(0, 255, 0, 255);
    public static readonly Color Magenta = new(255, 0, 255, 255);
    public static readonly Color Maroon = new(128, 0, 0, 255);
    public static readonly Color Navy = new(0, 0, 128, 255);
    public static readonly Color Olive = new(128, 128, 0, 255);
    public static readonly Color Orange = new(255, 165, 0, 255);
    public static readonly Color Pink = new(255, 192, 203, 255);
    public static readonly Color Purple = new(128, 0, 128, 255);
    public static readonly Color Red = new(255, 0, 0, 255);
    public static readonly Color RoyalBlue = new(65, 105, 225, 255);
    public static readonly Color Silver = new(192, 192, 192, 255);
    public static readonly Color SkyBlue = new(135, 206, 235, 255);
    public static readonly Color Snow = new(255, 250, 250, 255);
    public static readonly Color Teal = new(0, 128, 128, 255);
    public static readonly Color Transparent = new(0, 0, 0, 0);
    public static readonly Color Turquoise = new(64, 224, 208, 255);
    public static readonly Color Violet = new(238, 130, 238, 255);
    public static readonly Color White = new(255, 255, 255, 255);
    public static readonly Color Yellow = new(255, 255, 0, 255);
}

public static class FColors
{
    public static readonly FColor AliceBlue = new(0.941f, 0.972f, 1.000f, 1.000f);
    public static readonly FColor AntiqueWhite = new(0.980f, 0.922f, 0.843f, 1.000f);
    public static readonly FColor Aqua = new(0.000f, 1.000f, 1.000f, 1.000f);
    public static readonly FColor Aquamarine = new(0.498f, 1.000f, 0.831f, 1.000f);
    public static readonly FColor Azure = new(0.941f, 1.000f, 1.000f, 1.000f);
    public static readonly FColor Beige = new(0.961f, 0.961f, 0.863f, 1.000f);
    public static readonly FColor Black = new(0.000f, 0.000f, 0.000f, 1.000f);
    public static readonly FColor Blue = new(0.000f, 0.000f, 1.000f, 1.000f);
    public static readonly FColor Brown = new(0.647f, 0.165f, 0.165f, 1.000f);
    public static readonly FColor Coral = new(1.000f, 0.498f, 0.314f, 1.000f);
    public static readonly FColor Crimson = new(0.863f, 0.078f, 0.235f, 1.000f);
    public static readonly FColor Cyan = new(0.000f, 1.000f, 1.000f, 1.000f);
    public static readonly FColor DarkBlue = new(0.000f, 0.000f, 0.545f, 1.000f);
    public static readonly FColor DarkGrey = new(0.663f, 0.663f, 0.663f, 1.000f);
    public static readonly FColor DarkGreen = new(0.000f, 0.392f, 0.000f, 1.000f);
    public static readonly FColor DarkRed = new(0.545f, 0.000f, 0.000f, 1.000f);
    public static readonly FColor DeepPink = new(1.000f, 0.078f, 0.576f, 1.000f);
    public static readonly FColor ForestGreen = new(0.133f, 0.545f, 0.133f, 1.000f);
    public static readonly FColor Gold = new(1.000f, 0.843f, 0.000f, 1.000f);
    public static readonly FColor Grey = new(0.502f, 0.502f, 0.502f, 1.000f);
    public static readonly FColor Green = new(0.000f, 1.000f, 0.000f, 1.000f);
    public static readonly FColor HotPink = new(1.000f, 0.412f, 0.706f, 1.000f);
    public static readonly FColor Indigo = new(0.294f, 0.000f, 0.510f, 1.000f);
    public static readonly FColor Ivory = new(1.000f, 1.000f, 0.941f, 1.000f);
    public static readonly FColor LightBlue = new(0.678f, 0.847f, 0.902f, 1.000f);
    public static readonly FColor LightGrey = new(0.827f, 0.827f, 0.827f, 1.000f);
    public static readonly FColor LightGreen = new(0.565f, 0.933f, 0.565f, 1.000f);
    public static readonly FColor LightPink = new(1.000f, 0.714f, 0.757f, 1.000f);
    public static readonly FColor Lime = new(0.000f, 1.000f, 0.000f, 1.000f);
    public static readonly FColor Magenta = new(1.000f, 0.000f, 1.000f, 1.000f);
    public static readonly FColor Maroon = new(0.502f, 0.000f, 0.000f, 1.000f);
    public static readonly FColor Navy = new(0.000f, 0.000f, 0.502f, 1.000f);
    public static readonly FColor Olive = new(0.502f, 0.502f, 0.000f, 1.000f);
    public static readonly FColor Orange = new(1.000f, 0.647f, 0.000f, 1.000f);
    public static readonly FColor Pink = new(1.000f, 0.753f, 0.796f, 1.000f);
    public static readonly FColor Purple = new(0.502f, 0.000f, 0.502f, 1.000f);
    public static readonly FColor Red = new(1.000f, 0.000f, 0.000f, 1.000f);
    public static readonly FColor RoyalBlue = new(0.255f, 0.412f, 0.882f, 1.000f);
    public static readonly FColor Silver = new(0.753f, 0.753f, 0.753f, 1.000f);
    public static readonly FColor SkyBlue = new(0.529f, 0.808f, 0.922f, 1.000f);
    public static readonly FColor Snow = new(1.000f, 0.980f, 0.980f, 1.000f);
    public static readonly FColor Teal = new(0.000f, 0.502f, 0.502f, 1.000f);
    public static readonly FColor Transparent = new(0.000f, 0.000f, 0.000f, 0.000f);
    public static readonly FColor Turquoise = new(0.251f, 0.878f, 0.816f, 1.000f);
    public static readonly FColor Violet = new(0.933f, 0.510f, 0.933f, 1.000f);
    public static readonly FColor White = new(1.000f, 1.000f, 1.000f, 1.000f);
    public static readonly FColor Yellow = new(1.000f, 1.000f, 0.000f, 1.000f);
}
