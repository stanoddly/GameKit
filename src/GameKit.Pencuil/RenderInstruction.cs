using GameKit.Common;
using GameKit.Gpu;

namespace GameKit.Pencuil;

internal readonly record struct TextureRegionInstruction(int Depth, Texture Texture, ShortRectangle Area);
internal readonly record struct ColoredRectangleInstruction(int Depth, ShortRectangle Area, Color Color);
