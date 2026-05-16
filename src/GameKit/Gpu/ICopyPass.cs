using GameKit.Content;

namespace GameKit.Gpu;

public interface ICopyPass: IDisposable
{
    bool IsEmpty { get; }
    GpuVertexBuffer<TVertexType> CreateVertexBuffer<TVertexType>(ReadOnlySpan<TVertexType> vertices) where TVertexType: unmanaged, IVertexType;

    GpuVertexBuffer<TVertexType> CreateVertexBuffer<TVertexType>(Shape<TVertexType> shape)
        where TVertexType : unmanaged, IVertexType;

    void UpdateVertexBuffer<TVertexType>(GpuVertexBuffer<TVertexType> vertexBuffer, ReadOnlySpan<TVertexType> vertices) where TVertexType: unmanaged, IVertexType;

    GpuIndexBuffer CreateIndexBuffer(ReadOnlySpan<ushort> indices);

    GpuIndexBuffer CreateIndexBuffer(ReadOnlySpan<uint> indices);

    void UpdateIndexBuffer(GpuIndexBuffer indexBuffer, ReadOnlySpan<ushort> indices);

    void UpdateIndexBuffer(GpuIndexBuffer indexBuffer, ReadOnlySpan<uint> indices);

    GpuStorageBuffer<T> CreateStorageBuffer<T>(ReadOnlySpan<T> data) where T : unmanaged;

    void UpdateStorageBuffer<T>(GpuStorageBuffer<T> storageBuffer, ReadOnlySpan<T> data) where T : unmanaged;

    Texture CreateTexture(Image image);
    TextureArray CreateTextureArray(ReadOnlySpan<Image> images);
}
