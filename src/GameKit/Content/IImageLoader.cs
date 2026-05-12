namespace GameKit.Content;

public interface IImageLoader
{
    Image Load(ReadOnlySpan<char> path);
}
