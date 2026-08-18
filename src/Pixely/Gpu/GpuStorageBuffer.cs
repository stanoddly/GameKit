using System.Runtime.CompilerServices;
using Pixely.Utilities;
using SDL;

namespace Pixely.Gpu;

public abstract class GpuStorageBuffer : IDisposable, IGpuMemorySized
{
    internal GpuDevice GpuDevice { get; set; }
    internal Pointer<SDL_GPUBuffer> SdlBuffer { get; set; }

    public int BufferSize { get; }
    public int Size { get; internal set; }
    public long SizeInBytes { get; }
    public abstract int ElementSize { get; }

    protected GpuStorageBuffer(GpuDevice gpuDevice, Pointer<SDL_GPUBuffer> sdlBuffer, int size, long sizeInBytes)
    {
        GpuDevice = gpuDevice;
        SdlBuffer = sdlBuffer;
        BufferSize = size;
        Size = size;
        SizeInBytes = sizeInBytes;
    }

    public abstract void Dispose();
}

public class GpuStorageBuffer<T> : GpuStorageBuffer where T : unmanaged
{
    public override int ElementSize => Unsafe.SizeOf<T>();

    internal GpuStorageBuffer(GpuDevice gpuDevice, Pointer<SDL_GPUBuffer> sdlBuffer, int size)
        : base(gpuDevice, sdlBuffer, size, Unsafe.SizeOf<T>() * size)
    {
    }

    public override void Dispose()
    {
        GpuDevice.ReleaseStorageBuffer((GpuStorageBuffer)this);
    }
}
