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
        var pixelFormat = (PixelFormat)sdlSurface->format;

        // Convert all formats to ABGR8888 - on little-endian systems this gives us
        // [R, G, B, A] byte order in memory, which is what we need for RGBA8888 output
        if (pixelFormat != PixelFormat.Abgr8888)
        {
            Pointer<SDL_Surface> convertedSurface = SDL3.SDL_ConvertSurface(surface, SDL_PixelFormat.SDL_PIXELFORMAT_ABGR8888);
            if (convertedSurface.IsNull())
            {
                throw new InvalidOperationException($"SDL_ConvertSurface failed for format {pixelFormat}: {SDL3.SDL_GetError()}");
            }

            try
            {
                return CreateImageFromSurface(convertedSurface);
            }
            finally
            {
                SDL3.SDL_DestroySurface(convertedSurface);
            }
        }

        var size = new ShortSize((ushort)sdlSurface->w, (ushort)sdlSurface->h);
        var pitch = sdlSurface->pitch;
        var pixelData = (byte*)sdlSurface->pixels;

        int width = sdlSurface->w;
        int height = sdlSurface->h;
        byte[] data = new byte[width * height * 4];

        fixed (byte* dataPtr = data)
        {
            for (int y = 0; y < height; y++)
            {
                byte* src = pixelData + (y * pitch);
                byte* dst = dataPtr + (y * width * 4);
                Buffer.MemoryCopy(src, dst, width * 4, width * 4);
            }
        }

        return new SdlImage(data, size, PixelFormat.Rgba8888);
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
