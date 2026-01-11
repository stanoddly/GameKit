using System.Numerics;
using GameKit.Common;
using GameKit.Utilities;
using SDL;

namespace GameKit.Gpu;

public abstract class Texture: IDisposable, IGpuMemorySized
{
    private readonly long _sizeInBytes;

    internal Pointer<SDL_GPUTexture> SdlGpuTexture { get; set; }
    public TextureFormat Format { get; }
    public ShortSize Size { get; }
    public long SizeInBytes => _sizeInBytes;

    internal Texture(Pointer<SDL_GPUTexture> sdlGpuTexture, ShortSize size, TextureFormat format, long sizeInBytes)
    {
        SdlGpuTexture = sdlGpuTexture;
        Size = size;
        Format = format;
        _sizeInBytes = sizeInBytes;
    }

    public Vector4 CalculateTextureRegionUVs(ShortRectangle sourceRectangle)
    {
        float left = sourceRectangle.X;
        float top = sourceRectangle.Y;
        float right = sourceRectangle.X + sourceRectangle.Width;
        float bottom = sourceRectangle.Y + sourceRectangle.Height;

        (ushort width, ushort height) = Size;
        
        Vector4 textureCoords = new Vector4(
            left / width,
            top / height,
            right / width,
            bottom / height
        );

        return textureCoords;
    }

    public abstract void Dispose();
}

public class UserTexture: Texture
{
    private readonly IGpuDevice _gpuDevice;

    internal UserTexture(IGpuDevice gpuDevice, Pointer<SDL_GPUTexture> sdlGpuTexture, ShortSize size, TextureFormat format)
        : base(sdlGpuTexture, size, format, format.CalculateSizeInBytes(size.Width, size.Height))
    {
        _gpuDevice = gpuDevice;
    }

    public override void Dispose()
    {
        _gpuDevice.ReleaseTexture(this);
    }
}

public class SwapchainTexture : Texture
{
    internal SwapchainTexture(Pointer<SDL_GPUTexture> sdlGpuTexture, ShortSize size, TextureFormat format)
        : base(sdlGpuTexture, size, format, format.CalculateSizeInBytes(size.Width, size.Height))
    {
    }

    public override void Dispose()
    {
    }
}