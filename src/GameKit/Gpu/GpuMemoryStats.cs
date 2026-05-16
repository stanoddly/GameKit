namespace GameKit.Gpu;

public readonly struct GpuMemoryStats
{
    public int TextureCount { get; }
    public long TextureBytes { get; }
    public int VertexBufferCount { get; }
    public long VertexBufferBytes { get; }
    public int IndexBufferCount { get; }
    public long IndexBufferBytes { get; }
    public int StorageBufferCount { get; }
    public long StorageBufferBytes { get; }

    public long TotalBytes => TextureBytes + VertexBufferBytes + IndexBufferBytes + StorageBufferBytes;

    public GpuMemoryStats(
        int textureCount,
        long textureBytes,
        int vertexBufferCount,
        long vertexBufferBytes,
        int indexBufferCount,
        long indexBufferBytes,
        int storageBufferCount,
        long storageBufferBytes)
    {
        TextureCount = textureCount;
        TextureBytes = textureBytes;
        VertexBufferCount = vertexBufferCount;
        VertexBufferBytes = vertexBufferBytes;
        IndexBufferCount = indexBufferCount;
        IndexBufferBytes = indexBufferBytes;
        StorageBufferCount = storageBufferCount;
        StorageBufferBytes = storageBufferBytes;
    }
}
