namespace GameKit.Gpu;

public readonly struct GpuMemoryStats
{
    public int TextureCount { get; }
    public long TextureBytes { get; }
    public int VertexBufferCount { get; }
    public long VertexBufferBytes { get; }
    public int StorageBufferCount { get; }
    public long StorageBufferBytes { get; }

    public long TotalBytes => TextureBytes + VertexBufferBytes + StorageBufferBytes;

    public GpuMemoryStats(
        int textureCount,
        long textureBytes,
        int vertexBufferCount,
        long vertexBufferBytes,
        int storageBufferCount,
        long storageBufferBytes)
    {
        TextureCount = textureCount;
        TextureBytes = textureBytes;
        VertexBufferCount = vertexBufferCount;
        VertexBufferBytes = vertexBufferBytes;
        StorageBufferCount = storageBufferCount;
        StorageBufferBytes = storageBufferBytes;
    }
}
