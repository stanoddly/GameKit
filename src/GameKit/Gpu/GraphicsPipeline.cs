using GameKit.Utilities;
using SDL;

namespace GameKit.Gpu;

public class GraphicsPipeline: IDisposable
{
    private readonly IGpuDevice _gpuDevice;
    private Type _vertexBufferType;
    internal Pointer<SDL_GPUGraphicsPipeline> Pointer { get; set; }

    internal GraphicsPipeline(IGpuDevice gpuDevice, Pointer<SDL_GPUGraphicsPipeline> pointer, Type vertexBufferType)
    {
        _gpuDevice = gpuDevice;
        Pointer = pointer;
        _vertexBufferType = vertexBufferType;
    }
    

    public void Dispose()
    {
        _gpuDevice.ReleaseGraphicsPipeline(this);
    }
}
