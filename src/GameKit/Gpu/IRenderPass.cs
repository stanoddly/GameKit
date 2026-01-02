namespace GameKit.Gpu;

public interface IRenderPass: IDisposable
{
    void BindGraphicsPipeline(GraphicsPipeline graphicsPipeline);

    void BindVertexBuffer<TVertexType>(GpuVertexBuffer<TVertexType> buffer)
        where TVertexType : unmanaged, IVertexType;

    void BindFragmentSamplers(ReadOnlySpan<Texture> textures, Sampler sampler, uint slot = 0);
    void BindFragmentSampler(Texture texture, Sampler sampler);
    void BindFragmentSamplerArray(TextureArray textureArray, Sampler sampler, uint slot = 0);

    void BindVertexStorageBuffers(ReadOnlySpan<GpuStorageBuffer> buffers, uint slot = 0);
    void BindVertexStorageBuffer(GpuStorageBuffer buffer, uint slot = 0);
    void BindFragmentStorageBuffers(ReadOnlySpan<GpuStorageBuffer> buffers, uint slot = 0);
    void BindFragmentStorageBuffer(GpuStorageBuffer buffer, uint slot = 0);

    void DrawPrimitive();
}