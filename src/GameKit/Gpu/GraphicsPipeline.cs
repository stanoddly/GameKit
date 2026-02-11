using System.Collections.Immutable;
using GameKit.ShaderCommon;
using GameKit.Utilities;
using SDL;

namespace GameKit.Gpu;

public class GraphicsPipeline: IDisposable
{
    private readonly IGpuDevice _gpuDevice;
    internal Pointer<SDL_GPUGraphicsPipeline> Pointer { get; set; }

    /// <summary>
    /// Gets the vertex type IDs for each buffer slot configured in this pipeline.
    /// Index corresponds to buffer slot number.
    /// </summary>
    public ImmutableArray<VertexTypeId> VertexBufferTypeIds { get; }

    /// <summary>
    /// Gets the number of vertex buffer slots configured in this pipeline.
    /// </summary>
    public int VertexBufferSlotCount => VertexBufferTypeIds.Length;

    public Shader VertexShader { get; }
    public Shader FragmentShader { get; }
    public DepthBufferFormat DepthBufferFormat { get; }

    internal GraphicsPipeline(IGpuDevice gpuDevice, Pointer<SDL_GPUGraphicsPipeline> pointer, ImmutableArray<VertexTypeId> vertexBufferTypeIds, Shader vertexShader, Shader fragmentShader, DepthBufferFormat depthBufferFormat)
    {
        _gpuDevice = gpuDevice;
        Pointer = pointer;
        VertexBufferTypeIds = vertexBufferTypeIds;
        VertexShader = vertexShader;
        FragmentShader = fragmentShader;
        DepthBufferFormat = depthBufferFormat;
    }


    public void Dispose()
    {
        _gpuDevice.ReleaseGraphicsPipeline(this);
    }
}
