using System.Numerics;
using GameKit.Gpu;

namespace GameKit.Pencuil;

internal readonly record struct TextureRegionInstruction(int Depth, Texture Texture, Rectangle Area, Vector4 Uvs, FColor Tint);
internal readonly record struct ColoredRectangleInstruction(int Depth, Rectangle Area, Color Color);
