using System.Diagnostics.CodeAnalysis;

namespace GameKit.Sprites;

public sealed class SpriteAssetStorage
{
    private readonly Dictionary<string, SpriteAsset> _sprites = new();
    private readonly Dictionary<string, AnimatedSpriteAsset> _animatedSprites = new();

    public bool TryGetSprite(string path, [NotNullWhen(true)] out SpriteAsset? sprite)
    {
        return _sprites.TryGetValue(path, out sprite);
    }

    public bool TryGetAnimatedSprite(string path, [NotNullWhen(true)] out AnimatedSpriteAsset? animatedSprite)
    {
        return _animatedSprites.TryGetValue(path, out animatedSprite);
    }

    public void StoreSprite(string path, SpriteAsset sprite)
    {
        _sprites[path] = sprite;
    }

    public void StoreAnimatedSprite(string path, AnimatedSpriteAsset animatedSprite)
    {
        _animatedSprites[path] = animatedSprite;
    }

    public void Clear()
    {
        _sprites.Clear();
        _animatedSprites.Clear();
    }
}
