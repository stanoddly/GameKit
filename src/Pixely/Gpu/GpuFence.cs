using Pixely.Utilities;
using SDL;

namespace Pixely.Gpu;

public class GpuFence : IDisposable
{
    private readonly GpuDevice _gpuDevice;
    internal Pointer<SDL_GPUFence> Pointer { get; private set; }

    internal GpuFence(GpuDevice gpuDevice, Pointer<SDL_GPUFence> pointer)
    {
        _gpuDevice = gpuDevice;
        Pointer = pointer;
    }

    public bool IsSignaled()
    {
        ThrowIfDisposed();
        unsafe
        {
            return SDL3.SDL_QueryGPUFence(_gpuDevice.SdlGpuDevice, Pointer);
        }
    }

    public void Dispose()
    {
        if (!Pointer.IsNull)
        {
            unsafe
            {
                SDL3.SDL_ReleaseGPUFence(_gpuDevice.SdlGpuDevice, Pointer);
            }
            Pointer = Pointer<SDL_GPUFence>.Null;
        }
    }

    private void ThrowIfDisposed()
    {
        if (Pointer.IsNull)
        {
            throw new ObjectDisposedException(nameof(GpuFence));
        }
    }
}
