namespace GameKit;

public enum GpuBackend
{
    Automatic,
    Vulkan,
    Direct3D12,
    Metal
}

#if DEBUG
public sealed record GameKitConfig(
    bool EnableSdlLogging = true,
    bool EnableGpuValidation = true,
    GpuBackend GpuBackend = GpuBackend.Automatic);
#else
public sealed record GameKitConfig(
    bool EnableSdlLogging = false,
    bool EnableGpuValidation = false,
    GpuBackend GpuBackend = GpuBackend.Automatic);
#endif
