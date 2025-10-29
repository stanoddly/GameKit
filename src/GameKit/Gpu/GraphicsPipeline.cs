using GameKit.Utilities;
using SDL;

namespace GameKit.Gpu;

public class GraphicsPipeline: IDisposable
{
    private readonly IGpuDevice _gpuDevice;
    internal Pointer<SDL_GPUGraphicsPipeline> Pointer { get; set; }

    internal GraphicsPipeline(IGpuDevice gpuDevice, Pointer<SDL_GPUGraphicsPipeline> pointer)
    {
        _gpuDevice = gpuDevice;
        Pointer = pointer;
    }

    public void Dispose()
    {
        _gpuDevice.ReleaseGraphicsPipeline(this);
    }
}
