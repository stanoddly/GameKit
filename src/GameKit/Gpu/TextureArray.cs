using GameKit.Common;
using GameKit.Utilities;
using SDL;

namespace GameKit.Gpu;

public class TextureArray : Texture
{
    private readonly GpuDevice _gpuDevice;

    public ushort LayerCount { get; }

    internal TextureArray(
        GpuDevice gpuDevice,
        Pointer<SDL_GPUTexture> sdlGpuTexture,
        ShortSize size,
        ushort layerCount,
        TextureFormat format) : base(sdlGpuTexture, size, format, format.CalculateSizeInBytes(size.Width, size.Height, layerCount))
    {
        _gpuDevice = gpuDevice;
        LayerCount = layerCount;
    }

    public override void Dispose()
    {
        _gpuDevice.ReleaseTexture(this);
    }
}