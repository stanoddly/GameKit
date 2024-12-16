using System.Collections.Immutable;
using System.Numerics;
using System.Text.Json;
using GameKit.Content;
using GameKit.Gpu;
using GameKit.Utilities;

namespace GameKit.Sprites;

public record SpriteAsset(Texture Texture, Rectangle SourceRectangle);

public record AnimatedSpriteAsset(float FrameDuration, Texture Texture, ImmutableArray<Rectangle> Frames, bool Repeat);

public sealed class SpriteAssetLoader: IContentLoader<SpriteAsset>, IContentLoader<AnimatedSpriteAsset>
{
    private VirtualFileSystem _fileSystem;
    private readonly IContentLoader<Texture> _textureLoader;
    private readonly Dictionary<string, SpriteAsset> _sprites = new();
    private readonly Dictionary<string, AnimatedSpriteAsset> _animatedSprites = [];
    private readonly JsonSerializerOptions _options = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip, PropertyNameCaseInsensitive = true,
        Converters = {new RectangleJsonConverter()}
    };

    public SpriteAssetLoader(IContentLoader<Texture> textureLoader, VirtualFileSystem fileSystem)
    {
        _textureLoader = textureLoader;
        _fileSystem = fileSystem;
    }

    private AnimatedSpriteAsset CreateAnimation(AnimatedSpriteDto animatedSpriteDto)
    {
        Texture texture = _textureLoader.Load(animatedSpriteDto.Image);
        AnimatedSpriteAsset animatedSpriteAsset = new AnimatedSpriteAsset((float)animatedSpriteDto.FrameDuration, texture, animatedSpriteDto.Frames, animatedSpriteDto.Repeat);

        return animatedSpriteAsset;
    }

    SpriteAsset IContentLoader<SpriteAsset>.Load(string path)
    {
        if (_sprites.TryGetValue(path, out SpriteAsset? existingSprite))
        {
            return existingSprite;
        }

        using var spritesJsonStream = _fileSystem.OpenStream(path);

        SpriteDto spriteDto = JsonSerializer.Deserialize<SpriteDto>(spritesJsonStream, _options)
                               ?? throw new JsonException("Deserialization returned null for SpriteDto.");
        
        Texture texture = _textureLoader.Load(spriteDto.Image);
        SpriteAsset spriteAsset = new SpriteAsset(texture, spriteDto.SourceRectangle);
        
        _sprites[path] = spriteAsset;

        return spriteAsset;
    }

    AnimatedSpriteAsset IContentLoader<AnimatedSpriteAsset>.Load(string path)
    {
        if (_animatedSprites.TryGetValue(path, out AnimatedSpriteAsset? existingAnimation))
        {
            return existingAnimation;
        }

        using var stream = _fileSystem.OpenStream(path);
        AnimatedSpriteDto animatedSpriteDto = JsonSerializer.Deserialize<AnimatedSpriteDto>(stream, _options)
                                    ?? throw new JsonException("Deserialization returned null for AnimatedSpriteDto.");

        AnimatedSpriteAsset animatedSpriteAsset = CreateAnimation(animatedSpriteDto);
        _animatedSprites[path] = animatedSpriteAsset;

        return animatedSpriteAsset;
    }

    public void Dispose()
    {
        _sprites.Clear();
        _animatedSprites.Clear();
    }
}
