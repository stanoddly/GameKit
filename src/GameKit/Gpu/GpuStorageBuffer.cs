using System.Runtime.CompilerServices;
using GameKit.Utilities;
using SDL;

namespace GameKit.Gpu;

public abstract class GpuStorageBuffer : IDisposable, IGpuMemorySized
{
    internal IGpuDevice GpuDevice { get; set; }
    internal Pointer<SDL_GPUBuffer> SdlBuffer { get; set; }

    public int BufferSize { get; }
    public int Size { get; internal set; }
    public abstract long SizeInBytes { get; }

    protected GpuStorageBuffer(IGpuDevice gpuDevice, Pointer<SDL_GPUBuffer> sdlBuffer, int size)
    {
        GpuDevice = gpuDevice;
        SdlBuffer = sdlBuffer;
        BufferSize = size;
        Size = size;
    }

    public abstract void Dispose();
}

public class GpuStorageBuffer<T> : GpuStorageBuffer where T : unmanaged
{
    public int BufferSizeBytes => Unsafe.SizeOf<T>() * BufferSize;
    public override long SizeInBytes => BufferSizeBytes;

    internal GpuStorageBuffer(IGpuDevice gpuDevice, Pointer<SDL_GPUBuffer> sdlBuffer, int size)
        : base(gpuDevice, sdlBuffer, size)
    {
    }

    public override void Dispose()
    {
        GpuDevice.ReleaseStorageBuffer((GpuStorageBuffer)this);
    }
}
