using GameKit.App;
using GameKit.Common;
using GameKit.Content;
using GameKit.Gpu;
using GameKit.Utilities;
using SDL;

namespace GameKit.ImageLoader.SdlImage;

public class SdlImage : Image
{
    private readonly byte[] _data;
    private readonly ShortSize _size;
    private readonly PixelFormat _pixelFormat;

    internal SdlImage(byte[] data, ShortSize size, PixelFormat pixelFormat)
    {
        _data = data;
        _size = size;
        _pixelFormat = pixelFormat;
    }

    public override ReadOnlySpan<byte> Data => _data;
    public override ShortSize Size => _size;
    public override PixelFormat PixelFormat => _pixelFormat;
    public override void Dispose() { }
}

public class SdlImageLoader : IContentLoader<Image>
{
    private readonly VirtualFileSystem _fileSystem;

    public SdlImageLoader(VirtualFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public Image Load(ReadOnlySpan<char> path)
    {
        using Stream fileStream = _fileSystem.GetFile(path).Open();
        byte[] fileData = new byte[fileStream.Length];
        fileStream.ReadExactly(fileData);

        unsafe
        {
            fixed (byte* fileDataPtr = fileData)
            {
                Pointer<SDL_IOStream> sdlStream = SDL3.SDL_IOFromConstMem((IntPtr)fileDataPtr, (UIntPtr)fileData.Length);
                if (sdlStream.IsNull())
                {
                    throw new InvalidOperationException($"SDL_IOFromConstMem failed: {SDL3.SDL_GetError()}");
                }

                Pointer<SDL_Surface> surface = SDL3_image.IMG_Load_IO(sdlStream, true);
                if (surface.IsNull())
                {
                    throw new InvalidOperationException($"IMG_Load_IO failed: {SDL3.SDL_GetError()}");
                }

                try
                {
                    return CreateImageFromSurface(surface);
                }
                finally
                {
                    SDL3.SDL_DestroySurface(surface);
                }
            }
        }
    }

    private static unsafe Image CreateImageFromSurface(Pointer<SDL_Surface> surface)
    {
        SDL_Surface* sdlSurface = surface;
        var size = new ShortSize((ushort)sdlSurface->w, (ushort)sdlSurface->h);
        var pitch = sdlSurface->pitch;
        var pixelData = (byte*)sdlSurface->pixels;
        var pixelFormat = (PixelFormat)sdlSurface->format;

        byte[] data;
        PixelFormat targetFormat;

        if (pixelFormat == PixelFormat.Argb8888)
        {
            int width = sdlSurface->w;
            int height = sdlSurface->h;
            data = new byte[width * height * 4];

            fixed (byte* dataPtr = data)
            {
                byte* dst = dataPtr;

                for (int y = 0; y < height; y++)
                {
                    byte* src = pixelData + (y * pitch);

                    for (int x = 0; x < width; x++)
                    {
                        byte b = src[0];
                        byte g = src[1];
                        byte r = src[2];
                        byte a = src[3];

                        dst[0] = r;
                        dst[1] = g;
                        dst[2] = b;
                        dst[3] = a;

                        src += 4;
                        dst += 4;
                    }
                }
            }

            targetFormat = PixelFormat.Rgba8888;
        }
        else if (pixelFormat == PixelFormat.Abgr8888)
        {
            int width = sdlSurface->w;
            int height = sdlSurface->h;
            data = new byte[width * height * 4];

            fixed (byte* dataPtr = data)
            {
                byte* dst = dataPtr;

                for (int y = 0; y < height; y++)
                {
                    byte* src = pixelData + (y * pitch);

                    for (int x = 0; x < width; x++)
                    {
                        byte r = src[0];
                        byte g = src[1];
                        byte b = src[2];
                        byte a = src[3];

                        dst[0] = r;
                        dst[1] = g;
                        dst[2] = b;
                        dst[3] = a;

                        src += 4;
                        dst += 4;
                    }
                }
            }

            targetFormat = PixelFormat.Rgba8888;
        }
        else if (pixelFormat == PixelFormat.Rgba8888)
        {
            int width = sdlSurface->w;
            int height = sdlSurface->h;
            int totalBytes = width * height * 4;
            data = new byte[totalBytes];

            fixed (byte* dataPtr = data)
            {
                for (int y = 0; y < height; y++)
                {
                    byte* src = pixelData + (y * pitch);
                    byte* dst = dataPtr + (y * width * 4);
                    Buffer.MemoryCopy(src, dst, width * 4, width * 4);
                }
            }

            targetFormat = PixelFormat.Rgba8888;
        }
        else
        {
            int totalBytes = pitch * sdlSurface->h;
            data = new byte[totalBytes];
            fixed (byte* dataPtr = data)
            {
                Buffer.MemoryCopy(pixelData, dataPtr, totalBytes, totalBytes);
            }
            targetFormat = pixelFormat;
        }

        return new SdlImage(data, size, targetFormat);
    }
}

public static class SdlImageGameKitBuilderExtensions
{
    public static GameKitAppBuilder AddSdlImageLoader(this GameKitAppBuilder builder)
    {
        builder.RegisterType<SdlImageLoader>().As<IContentLoader<Image>>();
        return builder;
    }
}
