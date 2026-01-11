using System.Runtime.CompilerServices;
using GameKit.Utilities;
using SDL;

namespace GameKit.Gpu;

public abstract class GpuVertexBuffer: IDisposable, IGpuMemorySized
{
    internal IGpuDevice GpuDevice { get; set; }

    internal Pointer<SDL_GPUBuffer> SdlVertexBuffer { get; set; }
    internal Pointer<SDL_GPUBuffer> SdlIndexBuffer { get; set; }
    public int BufferSize { get; }
    public int Size { get; internal set; }
    public abstract long SizeInBytes { get; }
    
    protected GpuVertexBuffer(IGpuDevice gpuDevice, Pointer<SDL_GPUBuffer> sdlVertexBuffer, Pointer<SDL_GPUBuffer> sdlIndexBuffer, int size)
    {
        GpuDevice = gpuDevice;
        SdlVertexBuffer = sdlVertexBuffer;
        SdlIndexBuffer = sdlIndexBuffer;
        BufferSize = size;
        Size = size;
    }
    
    public void Dispose()
    {
        GpuDevice.ReleaseVertexBuffer(this);
    }
}

public class GpuVertexBuffer<TVertexType>: GpuVertexBuffer where TVertexType : unmanaged, IVertexType
{
    internal GpuVertexBuffer(GpuDevice gpuDevice, Pointer<SDL_GPUBuffer> sdlVertexBuffer, Pointer<SDL_GPUBuffer> sdlIndexBuffer, int size)
        : base(gpuDevice, sdlVertexBuffer, sdlIndexBuffer, size)
    {
    }

    public int BufferSizeBytes => Unsafe.SizeOf<TVertexType>() * BufferSize;
    public override long SizeInBytes => BufferSizeBytes;
}
