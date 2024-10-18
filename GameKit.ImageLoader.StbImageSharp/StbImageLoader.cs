using GameKit.Content;
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
    public override (int, int) Size => (_imageResult.Width, _imageResult.Height);
    public override void Dispose() {}
}

public class StbImageLoader : IContentLoader<Image>
{
    public Image Load(IContentManager contentManager, FileSystem fileSystem, string path)
    {
        using Stream fileStream = fileSystem.GetFile(path).Open();
        ImageResult imageResult = ImageResult.FromStream(fileStream, ColorComponents.RedGreenBlueAlpha);

        return new StbImage(imageResult);
    }
}

public static class StbImageGameKitBuilderExtensions
{
    public static GameKitAppBuilder UseStbImageLoader(this GameKitAppBuilder builder)
    {
        StbImageLoader imageLoader = new StbImageLoader();
        builder.WithContentLoader(imageLoader);
        return builder;
    }
}