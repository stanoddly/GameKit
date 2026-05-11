namespace GameKit.Gpu;

public interface IComputePass : IDisposable
{
    void BindComputePipeline(ComputePipeline pipeline);

    void BindSamplers(ReadOnlySpan<Texture> textures, Sampler sampler, uint slot = 0);

    void BindStorageTextures(ReadOnlySpan<Texture> textures, uint slot = 0);
    void BindStorageTexture(Texture texture, uint slot = 0);

    void BindStorageBuffers(ReadOnlySpan<GpuStorageBuffer> buffers, uint slot = 0);
    void BindStorageBuffer(GpuStorageBuffer buffer, uint slot = 0);

    void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ);
    void DispatchIndirect(GpuStorageBuffer buffer, uint offset = 0);
}
