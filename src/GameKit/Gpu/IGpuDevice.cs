using GameKit.Common;
using GameKit.Shaders;

namespace GameKit.Gpu;

public interface IGpuDevice : IDisposable
{
    GpuMemoryStats MemoryStats { get; }

    ShaderFormats GetSupportedShaderFormats();

    bool IsTextureFormatSupported(TextureFormat format, TextureType type, TextureUsage usage);

    CommandBuffer AcquireCommandBuffer();
    
    Sampler CreateSampler(SamplerConfig config);
    
    Texture CreateDepthBufferTexture(ShortSize size, DepthBufferFormat format, bool sampler = false);
    
    Texture CreateColorTargetTexture(ShortSize size, TextureFormat format);

    Texture CreateTexture(ShortSize size, TextureFormat format, TextureUsage usage);

    void RegisterTexture(Texture texture);

    void RegisterVertexBuffer(GpuVertexBuffer vertexBuffer);

    void RegisterGraphicsPipeline(GraphicsPipeline graphicsPipeline);

    void RegisterComputePipeline(ComputePipeline computePipeline);

    void RegisterShader(GraphicsShader shader);
    
    void ReleaseTexture(Texture texture);
    
    void ReleaseGraphicsPipeline(GraphicsPipeline pipeline);

    void ReleaseComputePipeline(ComputePipeline computePipeline);

    void ReleaseShader(GraphicsShader shader);
    
    void ReleaseVertexBuffer(GpuVertexBuffer vertexBuffer);

    void ReleaseSampler(Sampler sampler);

    void RegisterStorageBuffer(GpuStorageBuffer storageBuffer);

    void ReleaseStorageBuffer(GpuStorageBuffer storageBuffer);

    GpuVertexBuffer<TVertexType> CreateVertexBuffer<TVertexType>(int length) where TVertexType : unmanaged, IVertexType;

    void WaitForFences(ReadOnlySpan<GpuFence> fences, bool waitAll = true);
}
