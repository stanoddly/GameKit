namespace Pixely.Sprites;

public interface IAnimatedSpriteAssetLoader
{
    AnimatedSpriteAsset Load(ReadOnlySpan<char> path);
}
