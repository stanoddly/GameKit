namespace Pixely.Sprites;

public interface ISpriteAssetLoader
{
    SpriteAsset Load(ReadOnlySpan<char> path);
}
