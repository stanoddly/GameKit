using Pixely.ShaderCommon;
using Pixely.Shaders;
using Pixely.Utilities;
using SDL;

namespace Pixely.Gpu;

internal sealed class GraphicsShader
{
    internal Pointer<SDL_GPUShader> Pointer { get; set; }
    internal ShaderBindingLayout BindingLayout { get; }
    internal ShaderSystemValueInputs SystemValueInputs { get; }

    internal GraphicsShader(
        Pointer<SDL_GPUShader> pointer,
        ShaderBindingLayout bindingLayout,
        ShaderSystemValueInputs systemValueInputs)
    {
        Pointer = pointer;
        BindingLayout = bindingLayout;
        SystemValueInputs = systemValueInputs;
    }
}

public sealed class GraphicsShaderProgram : IDisposable
{
    private readonly GpuDevice _gpuDevice;
    internal GraphicsShader VertexShader { get; }
    internal GraphicsShader FragmentShader { get; }

    internal GraphicsShaderProgram(
        GpuDevice gpuDevice,
        GraphicsShader vertexShader,
        GraphicsShader fragmentShader)
    {
        _gpuDevice = gpuDevice;
        VertexShader = vertexShader;
        FragmentShader = fragmentShader;
    }

    public void Dispose()
    {
        _gpuDevice.ReleaseGraphicsShaderProgram(this);
    }
}
