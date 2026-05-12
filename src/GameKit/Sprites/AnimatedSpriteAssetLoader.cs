using System.Collections.Immutable;
using System.Numerics;
using System.Text.Json;
using GameKit.Common;
using GameKit.Content;
using GameKit.Gpu;
using GameKit.Utilities;

namespace GameKit.Sprites;

public sealed class AnimatedSpriteAssetLoader : IAnimatedSpriteAssetLoader
{
    private readonly VirtualFileSystem _fileSystem;
    private readonly ITextureLoader _textureLoader;
    private readonly SpriteAssetStorage _storage;

    public AnimatedSpriteAssetLoader(ITextureLoader textureLoader, VirtualFileSystem fileSystem, SpriteAssetStorage storage)
    {
        _textureLoader = textureLoader;
        _fileSystem = fileSystem;
        _storage = storage;
    }

    private AnimatedSpriteAsset CreateAnimation(AnimatedSpriteDto animatedSpriteDto)
    {
        Texture texture = _textureLoader.Load(animatedSpriteDto.Texture);
        ImmutableArray<ShortRectangle>.Builder builder = ImmutableArray.CreateBuilder<ShortRectangle>(animatedSpriteDto.Frames.Length);
        foreach (ShortRectangle frame in animatedSpriteDto.Frames)
        {
            builder.Add(frame);
        }
        Vector2 anchorOffset = animatedSpriteDto.AnchorOffset is { Length: 2 } offset
            ? new Vector2(offset[0], offset[1])
            : Vector2.Zero;
        AnimatedSpriteAsset animatedSpriteAsset = new AnimatedSpriteAsset((float)animatedSpriteDto.FrameDuration, texture, builder.MoveToImmutable(), anchorOffset, animatedSpriteDto.Flip);
        return animatedSpriteAsset;
    }

    public AnimatedSpriteAsset Load(ReadOnlySpan<char> path)
    {
        if (_storage.TryGetAnimatedSprite(path, out AnimatedSpriteAsset? existingAnimation))
        {
            return existingAnimation;
        }
        using Stream stream = _fileSystem.OpenStream(path);
        AnimatedSpriteDto animatedSpriteDto = JsonSerializer.Deserialize(stream, SpriteDtosJsonContext.Default.AnimatedSpriteDto)
                                        ?? throw new JsonException("Deserialization returned null for AnimatedSpriteDto.");
        AnimatedSpriteAsset animatedSpriteAsset = CreateAnimation(animatedSpriteDto);
        _storage.StoreAnimatedSprite(path.ToString(), animatedSpriteAsset);
        return animatedSpriteAsset;
    }
}
