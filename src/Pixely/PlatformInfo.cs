namespace Pixely;

public sealed record PlatformInfo(string? SdlVideoDriver)
{
    public bool SupportsAlwaysOnTopWindows
    {
        get { return !string.Equals(SdlVideoDriver, "wayland", StringComparison.OrdinalIgnoreCase); }
    }

    public bool SupportsSetWindowPosition
    {
        get { return !string.Equals(SdlVideoDriver, "wayland", StringComparison.OrdinalIgnoreCase); }
    }

    public bool SupportsClickThrough
    {
        get { return !string.Equals(SdlVideoDriver, "wayland", StringComparison.OrdinalIgnoreCase); }
    }
}
