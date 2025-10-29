using GameKit.Content;

namespace GameKit.Gpu;

public interface ICopyPass: IDisposable
{
    GpuVertexBuffer<TVertexType> CreateVertexBuffer<TVertexType>(ReadOnlySpan<TVertexType> vertices) where TVertexType: unmanaged, IVertexType;

    GpuVertexBuffer<TVertexType> CreateVertexBuffer<TVertexType>(Shape<TVertexType> shape)
        where TVertexType : unmanaged, IVertexType;

    void UpdateVertexBuffer<TVertexType>(GpuVertexBuffer<TVertexType> vertexBuffer, ReadOnlySpan<TVertexType> vertices) where TVertexType: unmanaged, IVertexType;
    Texture CreateTexture(Image image);
}