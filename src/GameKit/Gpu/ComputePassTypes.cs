namespace GameKit.Gpu;

public readonly struct StorageTextureReadWriteBinding
{
    public required Texture Texture { get; init; }
    public uint MipLevel { get; init; }
    public uint Layer { get; init; }
    public bool Cycle { get; init; }
}

public readonly struct StorageBufferReadWriteBinding
{
    public required GpuStorageBuffer Buffer { get; init; }
    public bool Cycle { get; init; }
}
