using System.Collections.Immutable;
using System.Numerics;
using System.Text.Json;
using GameKit.Content;
using GameKit.Gpu;
using GameKit.Utilities;

namespace GameKit.Sprites;

public record Sprite(Texture Texture, Vector2 TopLeft, Vector2 BottomRight, Vector2 Pivot);

public readonly record struct AnimatedSpriteFrame(Vector2 TopLeft, Vector2 BottomRight, Vector2 Pivot); 
public record AnimatedSprite(float FrameDuration, Texture Texture, ImmutableArray<AnimatedSpriteFrame> Frames, bool Repeat);

public sealed class SpriteLoader: IContentLoader<Sprite>, IContentLoader<AnimatedSprite>
{
    private readonly Dictionary<string, Sprite> _sprites = new();
    private readonly Dictionary<string, AnimatedSprite> _animatedSprites = [];
    private readonly JsonSerializerOptions _options = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip, PropertyNameCaseInsensitive = true,
        Converters = {new Vector2JsonConverter()}
    };

    private AnimatedSprite CreateAnimation(IContentManager contentManager, AnimatedSpriteDto animatedSpriteDto)
    {
        Texture texture = contentManager.Load<Texture>(animatedSpriteDto.Image);

        ImmutableArray<AnimatedSpriteFrame>.Builder animationFramesBuilder = ImmutableArray.CreateBuilder<AnimatedSpriteFrame>(animatedSpriteDto.Frames.Count);
        foreach (AnimationFrameDto animationFrameDto in animatedSpriteDto.Frames)
        {
            AnimatedSpriteFrame animatedSpriteFrame = new AnimatedSpriteFrame(animationFrameDto.TopLeft, animationFrameDto.BottomRight,
                animationFrameDto.Pivot);
            animationFramesBuilder.Add(animatedSpriteFrame);
        }

        ImmutableArray<AnimatedSpriteFrame> animationFrames = animationFramesBuilder.MoveToImmutable();
        AnimatedSprite animatedSprite = new AnimatedSprite((float)animatedSpriteDto.FrameDuration, texture, animationFrames, animatedSpriteDto.Repeat);

        return animatedSprite;
    }

    Sprite IContentLoader<Sprite>.Load(IContentManager contentManager, VirtualFileSystem fileSystem, string path)
    {
        if (_sprites.TryGetValue(path, out Sprite? existingSprite))
        {
            return existingSprite;
        }

        using var spritesJsonStream = contentManager.OpenStream(path);

        SpriteDto spriteDto = JsonSerializer.Deserialize<SpriteDto>(spritesJsonStream, _options)
                               ?? throw new JsonException("Deserialization returned null for SpriteDto.");
        
        Texture texture = contentManager.Load<Texture>(spriteDto.Filename);
        Sprite sprite = new Sprite(texture, spriteDto.TopLeft, spriteDto.BottomRight, spriteDto.Pivot);
        
        _sprites[path] = sprite;

        return sprite;
    }

    AnimatedSprite IContentLoader<AnimatedSprite>.Load(IContentManager contentManager, VirtualFileSystem fileSystem, string path)
    {
        if (_animatedSprites.TryGetValue(path, out AnimatedSprite? existingAnimation))
        {
            return existingAnimation;
        }

        using var stream = contentManager.OpenStream(path);
        AnimatedSpriteDto animatedSpriteDto = JsonSerializer.Deserialize<AnimatedSpriteDto>(stream, _options)
                                    ?? throw new JsonException("Deserialization returned null for AnimatedSpriteDto.");

        AnimatedSprite animatedSprite = CreateAnimation(contentManager, animatedSpriteDto);
        _animatedSprites[path] = animatedSprite;

        return animatedSprite;
    }

    public void Dispose()
    {
        _sprites.Clear();
        _animatedSprites.Clear();
    }
}
