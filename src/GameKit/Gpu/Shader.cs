using System.Runtime.InteropServices;
using GameKit.ShaderCommon;
using GameKit.Shaders;
using GameKit.Utilities;
using SDL;

namespace GameKit.Gpu;

public class Shader: IDisposable
{
    private readonly GpuDevice _gpuDevice;
    internal Pointer<SDL_GPUShader> Pointer { get; set; }
    public ShaderStage Stage { get; }
    public ShaderBindingLayout BindingLayout { get; }

    internal Shader(GpuDevice gpuDevice, Pointer<SDL_GPUShader> pointer, ShaderStage stage, ShaderBindingLayout bindingLayout)
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