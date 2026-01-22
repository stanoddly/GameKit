using System.Diagnostics.CodeAnalysis;

namespace GameKit.Sprites;

public sealed class SpriteAssetStorage
{
    private readonly Dictionary<string, SpriteAsset> _sprites = new();
    private readonly Dictionary<string, AnimatedSpriteAsset> _animatedSprites = new();
    private readonly Dictionary<string, SpriteAsset>.AlternateLookup<ReadOnlySpan<char>> _spritesLookup;
    private readonly Dictionary<string, AnimatedSpriteAsset>.AlternateLookup<ReadOnlySpan<char>> _animatedSpritesLookup;

    public SpriteAssetStorage()
    {
        _spritesLookup = _sprites.GetAlternateLookup<ReadOnlySpan<char>>();
        _animatedSpritesLookup = _animatedSprites.GetAlternateLookup<ReadOnlySpan<char>>();
    }

    public bool TryGetSprite(ReadOnlySpan<char> path, [NotNullWhen(true)] out SpriteAsset? sprite)
    {
        return _spritesLookup.TryGetValue(path, out sprite);
    }

    public bool TryGetAnimatedSprite(ReadOnlySpan<char> path, [NotNullWhen(true)] out AnimatedSpriteAsset? animatedSprite)
    {
        return _animatedSpritesLookup.TryGetValue(path, out animatedSprite);
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
