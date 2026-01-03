using GameKit.Common;
using GameKit.Shaders;

namespace GameKit.Gpu;

public interface IGpuDevice : IDisposable, IInitializable
{
    ShaderFormats GetSupportedShaderFormats();
    
    CommandBuffer AcquireCommandBuffer();
    
    Sampler CreateSampler(SamplerConfig config);
    
    Texture CreateDepthBufferTexture(ShortSize size, DepthBufferFormat format, bool sampler = false);
    
    Texture CreateColorTargetTexture(ShortSize size, TextureFormat format);

    void RegisterTexture(Texture texture);

    void RegisterVertexBuffer(GpuVertexBuffer vertexBuffer);

    void RegisterGraphicsPipeline(GraphicsPipeline graphicsPipeline);

    void RegisterShader(Shader shader);
    
    void ReleaseTexture(Texture texture);
    
    void ReleaseGraphicsPipeline(GraphicsPipeline pipeline);
    
    void ReleaseShader(Shader shader);
    
    void ReleaseVertexBuffer(GpuVertexBuffer vertexBuffer);

    void ReleaseSampler(Sampler sampler);

    void RegisterStorageBuffer(GpuStorageBuffer storageBuffer);

    void ReleaseStorageBuffer(GpuStorageBuffer storageBuffer);

    GpuVertexBuffer<TVertexType> CreateVertexBuffer<TVertexType>(int length) where TVertexType : unmanaged, IVertexType;

    void WaitForFences(ReadOnlySpan<GpuFence> fences, bool waitAll = true);
}