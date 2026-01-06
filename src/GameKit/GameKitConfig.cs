namespace GameKit;

#if DEBUG
public sealed record GameKitConfig(bool DebugLogging = true);
#else
public sealed record GameKitConfig(bool DebugLogging = false);
#endif
