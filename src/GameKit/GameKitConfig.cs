namespace GameKit;

public sealed record GameKitConfig
{
    #if DEBUG
    private const bool DefaultDebugLogging = true;
    #else
    private const bool DefaultDebugLogging = false;
    #endif

    public bool DebugLogging { get; init; } = DefaultDebugLogging;
}
