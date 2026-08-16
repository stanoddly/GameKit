namespace GameKit;

public enum WindowCloseBehavior
{
    QuitApplication
}

public sealed record WindowConfig(
    Size<uint>? Size = null,
    string? Title = null,
    bool Fullscreen = false,
    bool Resizable = false,
    bool Transparent = false,
    bool Borderless = false,
    bool AlwaysOnTop = false,
    WindowCloseBehavior CloseBehavior = WindowCloseBehavior.QuitApplication);
