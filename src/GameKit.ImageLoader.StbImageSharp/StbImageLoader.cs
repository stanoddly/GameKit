using GameKit.App;
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
    private VirtualFileSystem _fileSystem;

    public StbImageLoader(VirtualFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public Image Load(string path)
    {
        using Stream fileStream = _fileSystem.GetFile(path).Open();
        ImageResult imageResult = ImageResult.FromStream(fileStream, ColorComponents.RedGreenBlueAlpha);

        return new StbImage(imageResult);
    }
}

public static class StbImageGameKitBuilderExtensions
{
    public static GameKitApp AddStbImageLoader(this GameKitApp builder)
    {
        builder.AddScoped<StbImageLoader, IContentLoader<Image>>();
        return builder;
    }
}