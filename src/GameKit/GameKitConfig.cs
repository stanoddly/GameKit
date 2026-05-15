namespace GameKit;

#if DEBUG
public sealed record GameKitConfig(bool EnableSdlLogging = true, bool EnableGpuValidation = true);
#else
public sealed record GameKitConfig(bool EnableSdlLogging = false, bool EnableGpuValidation = false);
#endif
