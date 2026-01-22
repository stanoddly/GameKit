using GameKit.Content;

namespace GameKit;

public class NullImageLoader: IContentLoader<Image>
{
    public Image Load(ReadOnlySpan<char> path)
    {
        throw new InvalidOperationException("For image loading reference for example GameKit.ImageLoader.StbImageSharp package.");
    }
}