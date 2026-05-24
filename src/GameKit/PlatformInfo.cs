namespace GameKit;

public sealed record PlatformInfo(string? SdlVideoDriver)
{
    public bool SupportsAlwaysOnTopWindows
    {
        get { return !string.Equals(SdlVideoDriver, "wayland", StringComparison.OrdinalIgnoreCase); }
    }
}
