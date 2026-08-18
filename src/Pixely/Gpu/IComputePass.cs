namespace Pixely.Gpu;

public interface IComputePass : IDisposable
{
    void BindComputePipeline(ComputePipeline pipeline);

    void BindSamplers(ReadOnlySpan<Texture> textures, Sampler sampler, uint slot = 0);

    void BindReadOnlyStorageTextures(ReadOnlySpan<Texture> textures, uint slot = 0);
    void BindReadOnlyStorageTexture(Texture texture, uint slot = 0);

    void BindReadOnlyStorageBuffers(ReadOnlySpan<GpuStorageBuffer> buffers, uint slot = 0);
    void BindReadOnlyStorageBuffer(GpuStorageBuffer buffer, uint slot = 0);

    void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ);
    void DispatchIndirect(GpuStorageBuffer buffer, uint offset = 0);
}
