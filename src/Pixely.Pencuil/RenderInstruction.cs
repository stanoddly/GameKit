using System.Numerics;
using Pixely.Gpu;

namespace Pixely.Pencuil;

internal readonly record struct TextureRegionInstruction(int Depth, Texture Texture, Rectangle Area, Vector4 Uvs, FColor Tint);
internal readonly record struct ColoredRectangleInstruction(int Depth, Rectangle Area, Color Color);
