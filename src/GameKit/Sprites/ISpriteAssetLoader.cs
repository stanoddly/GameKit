namespace GameKit.Sprites;

public interface ISpriteAssetLoader
{
    SpriteAsset Load(ReadOnlySpan<char> path);
}
