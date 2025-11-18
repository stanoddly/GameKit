using GameKit.ShaderCommon;
using GameKit.Utilities;
using SDL;

namespace GameKit.Gpu;

public class GraphicsPipeline: IDisposable
{
    private readonly IGpuDevice _gpuDevice;
    internal Pointer<SDL_GPUGraphicsPipeline> Pointer { get; set; }
    public VertexTypeId VertexTypeId { get; }
    
    public Shader VertexShader { get; }
    public Shader FragmentShader { get; }

    internal GraphicsPipeline(IGpuDevice gpuDevice, Pointer<SDL_GPUGraphicsPipeline> pointer, VertexTypeId vertexTypeId, Shader vertexShader, Shader fragmentShader)
    {
        _gpuDevice = gpuDevice;
        Pointer = pointer;
        VertexTypeId = vertexTypeId;
        VertexShader = vertexShader;
        FragmentShader = fragmentShader;
    }
    

    public void Dispose()
    {
        _gpuDevice.ReleaseGraphicsPipeline(this);
    }
}
