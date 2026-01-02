using GameKit.Content;

namespace GameKit.Gpu;

public interface ICopyPass: IDisposable
{
    GpuVertexBuffer<TVertexType> CreateVertexBuffer<TVertexType>(ReadOnlySpan<TVertexType> vertices) where TVertexType: unmanaged, IVertexType;

    GpuVertexBuffer<TVertexType> CreateVertexBuffer<TVertexType>(Shape<TVertexType> shape)
        where TVertexType : unmanaged, IVertexType;

    void UpdateVertexBuffer<TVertexType>(GpuVertexBuffer<TVertexType> vertexBuffer, ReadOnlySpan<TVertexType> vertices) where TVertexType: unmanaged, IVertexType;

    GpuStorageBuffer<T> CreateStorageBuffer<T>(ReadOnlySpan<T> data) where T : unmanaged;

    void UpdateStorageBuffer<T>(GpuStorageBuffer<T> storageBuffer, ReadOnlySpan<T> data) where T : unmanaged;

    Texture CreateTexture(Image image);
    TextureArray CreateTextureArray(ReadOnlySpan<Image> images);
}