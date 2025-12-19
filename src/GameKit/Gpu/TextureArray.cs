using GameKit.Common;
using GameKit.Utilities;
using SDL;

namespace GameKit.Gpu;

public class TextureArray : Texture
{
    private readonly IGpuDevice _gpuDevice;

    public ushort LayerCount { get; }

    internal TextureArray(
        IGpuDevice gpuDevice,
        Pointer<SDL_GPUTexture> sdlGpuTexture,
        ShortSize size,
        ushort layerCount,
        TextureFormat format) : base(sdlGpuTexture, size, format)
    {
        _gpuDevice = gpuDevice;
        LayerCount = layerCount;
    }

    public override void Dispose()
    {
        _gpuDevice.ReleaseTexture(this);
    }
}