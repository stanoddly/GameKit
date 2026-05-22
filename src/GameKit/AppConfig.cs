using GameKit.Gpu;

namespace GameKit;

public sealed record AppConfig(Size<uint>? Size = null, string? Title = null, FColor? ClearColor = null, bool Fullscreen = false, bool Resizable = false, bool Transparent = false, bool Borderless = false);
