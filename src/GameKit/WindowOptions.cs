namespace GameKit;

public sealed record WindowOptions(
    Size<uint>? Size = null,
    string? Title = null,
    bool Fullscreen = false,
    bool Resizable = false,
    bool Transparent = false,
    bool Borderless = false,
    bool AlwaysOnTop = false,
    bool StopGameOnClose = false);
