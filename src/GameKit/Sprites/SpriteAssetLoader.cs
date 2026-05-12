using System.Text.Json;
using GameKit.Common;
using GameKit.Content;
using GameKit.Gpu;
using GameKit.Utilities;

namespace GameKit.Sprites;

public sealed class SpriteAssetLoader : ISpriteAssetLoader
{
    private readonly VirtualFileSystem _fileSystem;
    private readonly ITextureLoader _textureLoader;
    private readonly SpriteAssetStorage _storage;

    public SpriteAssetLoader(ITextureLoader textureLoader, VirtualFileSystem fileSystem, SpriteAssetStorage storage)
    {
        _fileSystem = fileSystem;
        _textureLoader = textureLoader;
        _storage = storage;
    }

    public SpriteAsset Load(ReadOnlySpan<char> path)
    {
        if (_storage.TryGetSprite(path, out SpriteAsset? existingSprite))
        {
            return existingSprite;
        }

        using Stream spritesJsonStream = _fileSystem.OpenStream(path);

        SpriteDto spriteDto = JsonSerializer.Deserialize(spritesJsonStream, SpriteDtosJsonContext.Default.SpriteDto)
                              ?? throw new JsonException("Deserialization returned null for SpriteDto.");

        Texture texture = _textureLoader.Load(spriteDto.Texture);
        ShortRectangle imageRegion = spriteDto.TextureRegion;

        SpriteAsset spriteAsset = new SpriteAsset(texture, imageRegion, spriteDto.Flip);

        _storage.StoreSprite(path.ToString(), spriteAsset);

        return spriteAsset;
    }
}
