using GameKit.App;
using GameKit.Common;
using GameKit.Content;
using GameKit.Gpu;
using StbImageSharp;

namespace GameKit.ImageLoader.StbImageSharp;

public class StbImage : Image
{
    private ImageResult _imageResult;

    internal StbImage(ImageResult imageResult)
    {
        _imageResult = imageResult;
    }

    public override ReadOnlySpan<byte> Data => _imageResult.Data;
    public override ShortSize Size => new((ushort)_imageResult.Width, (ushort)_imageResult.Height);
    public override PixelFormat PixelFormat { get; } = PixelFormat.Rgba8888;
    public override void Dispose() {}
}

public class StbImageLoader : IContentLoader<Image>
{
    private VirtualFileSystem _fileSystem;

    public StbImageLoader(VirtualFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public Image Load(ReadOnlySpan<char> path)
    {
        using Stream fileStream = _fileSystem.GetFile(path).Open();
        ImageResult imageResult = ImageResult.FromStream(fileStream, ColorComponents.RedGreenBlueAlpha);

        return new StbImage(imageResult);
    }
}

public static class StbImageGameKitBuilderExtensions
{
    public static GameKitAppBuilder AddStbImageLoader(this GameKitAppBuilder builder)
    {
        builder.RegisterType<StbImageLoader>().As<IContentLoader<Image>>();
        return builder;
    }
}
