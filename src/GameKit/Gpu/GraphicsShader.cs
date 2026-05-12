using System.Runtime.InteropServices;
using GameKit.ShaderCommon;
using GameKit.Shaders;
using GameKit.Utilities;
using SDL;

namespace GameKit.Gpu;

public abstract class GraphicsShader: IDisposable
{
    private readonly GpuDevice _gpuDevice;
    internal Pointer<SDL_GPUShader> Pointer { get; set; }
    internal ShaderStage Stage { get; }
    public ShaderBindingLayout BindingLayout { get; }

    private protected GraphicsShader(GpuDevice gpuDevice, Pointer<SDL_GPUShader> pointer, ShaderStage stage, ShaderBindingLayout bindingLayout)
    {
        _gpuDevice = gpuDevice;
        Pointer = pointer;
        Stage = stage;

        BindingLayout = bindingLayout;
    }

    public void Dispose()
    {
        _gpuDevice.ReleaseShader(this);
    }
}

public sealed class VertexShader: GraphicsShader
{
    internal VertexShader(GpuDevice gpuDevice, Pointer<SDL_GPUShader> pointer, ShaderBindingLayout bindingLayout)
        : base(gpuDevice, pointer, ShaderStage.Vertex, bindingLayout)
    {
    }
}

public sealed class FragmentShader: GraphicsShader
{
    internal FragmentShader(GpuDevice gpuDevice, Pointer<SDL_GPUShader> pointer, ShaderBindingLayout bindingLayout)
        : base(gpuDevice, pointer, ShaderStage.Fragment, bindingLayout)
    {
    }
}
