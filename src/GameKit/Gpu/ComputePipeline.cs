using GameKit.ShaderCommon;
using GameKit.Utilities;
using SDL;

namespace GameKit.Gpu;

public class ComputePipeline : IDisposable
{
    private readonly IGpuDevice _gpuDevice;
    internal Pointer<SDL_GPUComputePipeline> Pointer { get; set; }
    public ShaderBindingLayout BindingLayout { get; }

    internal ComputePipeline(IGpuDevice gpuDevice, Pointer<SDL_GPUComputePipeline> pointer, ShaderBindingLayout bindingLayout)
    {
        _gpuDevice = gpuDevice;
        Pointer = pointer;
        BindingLayout = bindingLayout;
    }

    public void Dispose()
    {
        _gpuDevice.ReleaseComputePipeline(this);
    }
}
