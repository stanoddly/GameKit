using GameKit.Content;

namespace GameKit.Gpu;

public class GpuMemorySystem: ICopyPass
{
    private readonly IGpuDevice _gpuDevice;
    private CommandBuffer? _commandBuffer;
    private ICopyPass? _copyPassImplementation;

    public GpuMemorySystem(IGpuDevice gpuDevice)
    {
        _gpuDevice = gpuDevice;
    }

    private ICopyPass GetOrCreateCopyPass()
    {
        if (_copyPassImplementation == null)
        {
            _commandBuffer = _gpuDevice.AcquireCommandBuffer();
            _copyPassImplementation = _commandBuffer.CreateCopyPass();
        }

        return _copyPassImplementation;
    }

    public GpuVertexBuffer<TVertexType> CreateVertexBuffer<TVertexType>(ReadOnlySpan<TVertexType> vertices) where TVertexType : unmanaged, IVertexType
    {
        return GetOrCreateCopyPass().CreateVertexBuffer(vertices);
    }

    public GpuVertexBuffer<TVertexType> CreateVertexBuffer<TVertexType>(Shape<TVertexType> shape) where TVertexType : unmanaged, IVertexType
    {
        return GetOrCreateCopyPass().CreateVertexBuffer(shape);
    }

    public void UpdateVertexBuffer<TVertexType>(GpuVertexBuffer<TVertexType> vertexBuffer, ReadOnlySpan<TVertexType> vertices) where TVertexType : unmanaged, IVertexType
    {
        GetOrCreateCopyPass().UpdateVertexBuffer(vertexBuffer, vertices);
    }

    public GpuStorageBuffer<T> CreateStorageBuffer<T>(ReadOnlySpan<T> data) where T : unmanaged
    {
        return GetOrCreateCopyPass().CreateStorageBuffer(data);
    }

    public void UpdateStorageBuffer<T>(GpuStorageBuffer<T> storageBuffer, ReadOnlySpan<T> data) where T : unmanaged
    {
        GetOrCreateCopyPass().UpdateStorageBuffer(storageBuffer, data);
    }

    public Texture CreateTexture(Image image)
    {
        return GetOrCreateCopyPass().CreateTexture(image);
    }

    public TextureArray CreateTextureArray(ReadOnlySpan<Image> images)
    {
        return GetOrCreateCopyPass().CreateTextureArray(images);
    }

    public void Dispose()
    {
        Submit();
    }

    public void Submit()
    {
        _copyPassImplementation?.Dispose();
        _copyPassImplementation = null;
        _commandBuffer?.Dispose();
        _commandBuffer = null;
    }
}