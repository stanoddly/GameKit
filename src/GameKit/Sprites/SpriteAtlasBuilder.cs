using System.Numerics;
using System.Text.Json;
using GameKit.Common;
using GameKit.Content;
using GameKit.Gpu;
using GameKit.Utilities;

namespace GameKit.Sprites;

public record SpriteAtlasBuilderConfig(string[] Directories);

public sealed class SpriteAtlasBuilder
{
    private const int Padding = 2;

    private readonly record struct PackedRectangle(ShortRectangle Rectangle, int ImageIndex);

    private readonly VirtualFileSystem _fileSystem;
    private readonly ITextureLoader _textureLoader;
    private readonly IContentLoader<Image> _imageLoader;
    private readonly SpriteAssetStorage _storage;
    private readonly JsonSerializerOptions _options = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        PropertyNameCaseInsensitive = true,
        Converters = { new ShortRectangleJsonConverter() }
    };

    public static SpriteAtlasBuilder Create(IServiceProvider serviceProvider)
    {
        SpriteAtlasBuilderConfig spriteAtlasBuilderConfig = serviceProvider.GetMandatoryService<SpriteAtlasBuilderConfig>();
        ITextureLoader textureLoader = serviceProvider.GetMandatoryService<ITextureLoader>();
        IContentLoader<Image> contentLoader = serviceProvider.GetMandatoryService<IContentLoader<Image>>();
        VirtualFileSystem fileSystem = serviceProvider.GetMandatoryService<VirtualFileSystem>();
        SpriteAssetStorage storage = serviceProvider.GetMandatoryService<SpriteAssetStorage>();

        SpriteAtlasBuilder spriteAtlasBuilder = new(textureLoader, contentLoader, fileSystem, storage);
        
        spriteAtlasBuilder.BuildSprites(spriteAtlasBuilderConfig.Directories);

        return spriteAtlasBuilder;
    }

    internal SpriteAtlasBuilder(ITextureLoader textureLoader, IContentLoader<Image> imageLoader, VirtualFileSystem fileSystem, SpriteAssetStorage storage)
    {
        _textureLoader = textureLoader;
        _imageLoader = imageLoader;
        _fileSystem = fileSystem;
        _storage = storage;
    }

    private static (ShortRectangle normalized, bool mirrorX, bool mirrorY) NormalizeRegion(ShortRectangle region)
    {
        bool mirrorX = false;
        bool mirrorY = false;
        short x = region.X;
        short y = region.Y;
        short width = region.Width;
        short height = region.Height;
        if (width < 0)
        {
            x = (short)(x + width + 1);
            width = (short)-width;
            mirrorX = true;
        }
        if (height < 0)
        {
            y = (short)(y + height + 1);
            height = (short)-height;
            mirrorY = true;
        }
        return (new ShortRectangle(x, y, width, height), mirrorX, mirrorY);
    }

    public void BuildSprites(params string[] directories)
    {
        var spriteImages = new List<(string path, Image image, ShortRectangle originalRegion, bool isAnimatedFrame, string? animationPath, double frameDuration, bool repeat, int frameIndex, int totalFrames, bool mirrorX, bool mirrorY)>();
        var animatedSpriteInfos = new Dictionary<string, (double frameDuration, bool repeat, List<int> frameIndices)>();
        foreach (var directory in directories)
        {
            CollectAllSpritesRecursivelyWithMirroring(directory, spriteImages, animatedSpriteInfos);
        }
        if (spriteImages.Count == 0)
        {
            return;
        }
        spriteImages.Sort((a, b) =>
        {
            int areaA = a.originalRegion.Width * a.originalRegion.Height;
            int areaB = b.originalRegion.Width * b.originalRegion.Height;
            return areaB.CompareTo(areaA);
        });
        (ShortSize atlasSize, List<PackedRectangle> packedRectangles) = PackImagesIntoAtlas(spriteImages.ConvertAll(x => (x.path, x.image, x.originalRegion)));
        RawImage atlasImage = CreateAtlasImageWithMirroring(spriteImages, packedRectangles, atlasSize);
        Texture atlasTexture = _textureLoader.Load(atlasImage);
        var staticSpriteMap = new Dictionary<string, int>();
        var animatedFrameMap = new Dictionary<int, (string animationPath, int frameIndex)>();
        for (int i = 0; i < spriteImages.Count; i++)
        {
            var entry = spriteImages[i];
            if (!entry.isAnimatedFrame)
            {
                staticSpriteMap[entry.path] = i;
            }
            else if (entry.animationPath != null)
            {
                animatedFrameMap[i] = (entry.animationPath, entry.frameIndex);
            }
        }
        foreach (var kv in staticSpriteMap)
        {
            int i = kv.Value;
            (string path, _, ShortRectangle originalRegion, _, _, _, _, _, _, _, _) = spriteImages[i];
            PackedRectangle packed = packedRectangles[i];
            ShortRectangle atlasRegion = new ShortRectangle(
                (short)(packed.Rectangle.X + Padding),
                (short)(packed.Rectangle.Y + Padding),
                originalRegion.Width,
                originalRegion.Height
            );
            SpriteAsset spriteAsset = new SpriteAsset(atlasTexture, atlasRegion);
            _storage.StoreSprite(path, spriteAsset);
        }
        var animationFramesByPath = new Dictionary<string, ShortRectangle[]>();
        foreach (var kv in animatedSpriteInfos)
        {
            animationFramesByPath[kv.Key] = new ShortRectangle[kv.Value.frameIndices.Count];
        }
        foreach (var kv in animatedFrameMap)
        {
            int i = kv.Key;
            (string animationPath, int frameIndex) = kv.Value;
            PackedRectangle packed = packedRectangles[i];
            ShortRectangle originalRegion = spriteImages[i].originalRegion;
            ShortRectangle atlasRegion = new ShortRectangle(
                (short)(packed.Rectangle.X + Padding),
                (short)(packed.Rectangle.Y + Padding),
                originalRegion.Width,
                originalRegion.Height
            );
            animationFramesByPath[animationPath][frameIndex] = atlasRegion;
        }
        foreach (var kv in animatedSpriteInfos)
        {
            string animationPath = kv.Key;
            var (frameDuration, repeat, frameIndices) = kv.Value;
            var frames = animationFramesByPath[animationPath];
            var immutableFrames = System.Collections.Immutable.ImmutableArray.CreateRange(frames);
            AnimatedSpriteAsset animatedSpriteAsset = new AnimatedSpriteAsset((float)frameDuration, atlasTexture, immutableFrames, repeat, Vector2.Zero);
            _storage.StoreAnimatedSprite(animationPath, animatedSpriteAsset);
        }
        atlasImage.Dispose();
    }

    private void CollectAllSpritesRecursivelyWithMirroring(string directory, List<(string path, Image image, ShortRectangle originalRegion, bool isAnimatedFrame, string? animationPath, double frameDuration, bool repeat, int frameIndex, int totalFrames, bool mirrorX, bool mirrorY)> spriteImages, Dictionary<string, (double frameDuration, bool repeat, List<int> frameIndices)> animatedSpriteInfos)
    {
        ReadOnlySpan<VirtualFile> files = _fileSystem.GetFiles(directory);
        foreach (VirtualFile file in files)
        {
            if (file.Path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                using var stream = file.Open();
                SpriteDto? spriteDto = null;
                AnimatedSpriteDto? animatedDto = null;
                try { spriteDto = JsonSerializer.Deserialize<SpriteDto>(stream, _options); } catch { }
                if (spriteDto != null)
                {
                    var (norm, mirrorX, mirrorY) = NormalizeRegion(spriteDto.TextureRegion);
                    Image image = _imageLoader.Load(spriteDto.Texture);
                    spriteImages.Add((file.Path, image, norm, false, null, 0, false, 0, 0, mirrorX, mirrorY));
                    continue;
                }
                stream.Position = 0;
                try { animatedDto = JsonSerializer.Deserialize<AnimatedSpriteDto>(stream, _options); } catch { }
                if (animatedDto != null)
                {
                    int totalFrames = animatedDto.Frames.Length;
                    var frameIndices = new List<int>(totalFrames);
                    for (int i = 0; i < totalFrames; i++)
                    {
                        var (norm, mirrorX, mirrorY) = NormalizeRegion(animatedDto.Frames[i]);
                        Image image = _imageLoader.Load(animatedDto.Texture);
                        spriteImages.Add((file.Path, image, norm, true, file.Path, animatedDto.FrameDuration, animatedDto.Repeat, i, totalFrames, mirrorX, mirrorY));
                        frameIndices.Add(spriteImages.Count - 1);
                    }
                    animatedSpriteInfos[file.Path] = (animatedDto.FrameDuration, animatedDto.Repeat, frameIndices);
                }
            }
        }
        ReadOnlySpan<string> subdirectories = _fileSystem.GetDirectories(directory);
        foreach (string subdirectory in subdirectories)
        {
            CollectAllSpritesRecursivelyWithMirroring(subdirectory, spriteImages, animatedSpriteInfos);
        }
    }

    private void CollectAllSpritesRecursively(string directory, List<(string path, Image image, ShortRectangle originalRegion, bool isAnimatedFrame, string? animationPath, double frameDuration, bool repeat, int frameIndex, int totalFrames)> spriteImages, Dictionary<string, (double frameDuration, bool repeat, List<int> frameIndices)> animatedSpriteInfos)
    {
        ReadOnlySpan<VirtualFile> files = _fileSystem.GetFiles(directory);
        foreach (VirtualFile file in files)
        {
            if (file.Path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                using var stream = file.Open();
                SpriteDto? spriteDto = null;
                AnimatedSpriteDto? animatedDto = null;
                try { spriteDto = JsonSerializer.Deserialize<SpriteDto>(stream, _options); } catch { }
                if (spriteDto != null)
                {
                    Image image = _imageLoader.Load(spriteDto.Texture);
                    spriteImages.Add((file.Path, image, spriteDto.TextureRegion, false, null, 0, false, 0, 0));
                    continue;
                }
                stream.Position = 0;
                try { animatedDto = JsonSerializer.Deserialize<AnimatedSpriteDto>(stream, _options); } catch { }
                if (animatedDto != null)
                {
                    int totalFrames = animatedDto.Frames.Length;
                    var frameIndices = new List<int>(totalFrames);
                    for (int i = 0; i < totalFrames; i++)
                    {
                        Image image = _imageLoader.Load(animatedDto.Texture);
                        spriteImages.Add((file.Path, image, animatedDto.Frames[i], true, file.Path, animatedDto.FrameDuration, animatedDto.Repeat, i, totalFrames));
                        frameIndices.Add(spriteImages.Count - 1);
                    }
                    animatedSpriteInfos[file.Path] = (animatedDto.FrameDuration, animatedDto.Repeat, frameIndices);
                }
            }
        }
        ReadOnlySpan<string> subdirectories = _fileSystem.GetDirectories(directory);
        foreach (string subdirectory in subdirectories)
        {
            CollectAllSpritesRecursively(subdirectory, spriteImages, animatedSpriteInfos);
        }
    }

    private (ShortSize atlasSize, List<PackedRectangle> packedRectangles) PackImagesIntoAtlas(
        List<(string path, Image image, ShortRectangle originalRegion)> spriteImages)
    {
        List<PackedRectangle> packedRectangles = new List<PackedRectangle>(spriteImages.Count);

        int atlasWidth = 1024;
        int atlasHeight = 1024;
        List<ShortRectangle> freeRectangles = new List<ShortRectangle>
        {
            new ShortRectangle(0, 0, (short)atlasWidth, (short)atlasHeight)
        };

        for (int i = 0; i < spriteImages.Count; i++)
        {
            (_, _, ShortRectangle originalRegion) = spriteImages[i];
            short width = (short)(originalRegion.Width + Padding * 2);
            short height = (short)(originalRegion.Height + Padding * 2);

            ShortRectangle? bestRect = FindBestFitRectangle(freeRectangles, (ushort)width, (ushort)height);

            if (bestRect == null)
            {
                atlasWidth *= 2;
                atlasHeight *= 2;

                freeRectangles.Clear();
                freeRectangles.Add(new ShortRectangle(0, 0, (short)atlasWidth, (short)atlasHeight));
                packedRectangles.Clear();
                i = -1;
                continue;
            }

            ShortRectangle usedRect = new ShortRectangle(bestRect.Value.X, bestRect.Value.Y, width, height);
            packedRectangles.Add(new PackedRectangle(usedRect, i));

            freeRectangles.Remove(bestRect.Value);

            SplitRectangle(freeRectangles, bestRect.Value, usedRect);
        }

        return (new ShortSize((ushort)atlasWidth, (ushort)atlasHeight), packedRectangles);
    }

    private ShortRectangle? FindBestFitRectangle(List<ShortRectangle> freeRectangles, ushort width, ushort height)
    {
        ShortRectangle? bestRect = null;
        int bestShortSide = int.MaxValue;

        foreach (ShortRectangle rect in freeRectangles)
        {
            if (rect.Width >= width && rect.Height >= height)
            {
                int leftoverHoriz = rect.Width - width;
                int leftoverVert = rect.Height - height;
                int shortSide = Math.Min(leftoverHoriz, leftoverVert);

                if (shortSide < bestShortSide)
                {
                    bestRect = rect;
                    bestShortSide = shortSide;
                }
            }
        }

        return bestRect;
    }

    private void SplitRectangle(List<ShortRectangle> freeRectangles, ShortRectangle freeRect, ShortRectangle usedRect)
    {
        int rightWidth = freeRect.X + freeRect.Width - (usedRect.X + usedRect.Width);
        int bottomHeight = freeRect.Y + freeRect.Height - (usedRect.Y + usedRect.Height);

        bool hasRight = rightWidth > 0;
        bool hasBottom = bottomHeight > 0;

        if (hasRight && hasBottom)
        {
            // Guillotine cut: give the larger remainder the full extent
            if (rightWidth > bottomHeight)
            {
                // Right gets full height, bottom gets only used width
                freeRectangles.Add(new ShortRectangle(
                    (short)(usedRect.X + usedRect.Width), freeRect.Y,
                    (short)rightWidth, freeRect.Height));
                freeRectangles.Add(new ShortRectangle(
                    freeRect.X, (short)(usedRect.Y + usedRect.Height),
                    usedRect.Width, (short)bottomHeight));
            }
            else
            {
                // Bottom gets full width, right gets only used height
                freeRectangles.Add(new ShortRectangle(
                    freeRect.X, (short)(usedRect.Y + usedRect.Height),
                    freeRect.Width, (short)bottomHeight));
                freeRectangles.Add(new ShortRectangle(
                    (short)(usedRect.X + usedRect.Width), freeRect.Y,
                    (short)rightWidth, usedRect.Height));
            }
        }
        else if (hasRight)
        {
            freeRectangles.Add(new ShortRectangle(
                (short)(usedRect.X + usedRect.Width), freeRect.Y,
                (short)rightWidth, freeRect.Height));
        }
        else if (hasBottom)
        {
            freeRectangles.Add(new ShortRectangle(
                freeRect.X, (short)(usedRect.Y + usedRect.Height),
                freeRect.Width, (short)bottomHeight));
        }
    }

    private RawImage CreateAtlasImage(
        List<(string path, Image image, ShortRectangle originalRegion)> spriteImages,
        List<PackedRectangle> packedRectangles,
        ShortSize atlasSize)
    {
        int totalBytes = atlasSize.Width * atlasSize.Height * 4;
        byte[] atlasData = new byte[totalBytes];

        for (int i = 0; i < packedRectangles.Count; i++)
        {
            PackedRectangle packed = packedRectangles[i];
            (_, Image sourceImage, ShortRectangle originalRegion) = spriteImages[packed.ImageIndex];

            ReadOnlySpan<byte> sourceData = sourceImage.Data;
            ShortSize sourceSize = sourceImage.Size;

            for (int y = 0; y < originalRegion.Height; y++)
            {
                for (int x = 0; x < originalRegion.Width; x++)
                {
                    int sourceX = originalRegion.X + x;
                    int sourceY = originalRegion.Y + y;
                    int sourceIndex = (sourceY * sourceSize.Width + sourceX) * 4;

                    int atlasX = packed.Rectangle.X + Padding + x;
                    int atlasY = packed.Rectangle.Y + Padding + y;
                    int atlasIndex = (atlasY * atlasSize.Width + atlasX) * 4;

                    if (sourceIndex + 3 < sourceData.Length && atlasIndex + 3 < atlasData.Length)
                    {
                        atlasData[atlasIndex] = sourceData[sourceIndex];
                        atlasData[atlasIndex + 1] = sourceData[sourceIndex + 1];
                        atlasData[atlasIndex + 2] = sourceData[sourceIndex + 2];
                        atlasData[atlasIndex + 3] = sourceData[sourceIndex + 3];
                    }
                }
            }
        }

        // TODO: make sure pixel format is aligned
        return new RawImage(atlasData, atlasSize, PixelFormat.Rgba8888);
    }

    private RawImage CreateAtlasImageWithMirroring(
        List<(string path, Image image, ShortRectangle originalRegion, bool isAnimatedFrame, string? animationPath, double frameDuration, bool repeat, int frameIndex, int totalFrames, bool mirrorX, bool mirrorY)> spriteImages,
        List<PackedRectangle> packedRectangles,
        ShortSize atlasSize)
    {
        int totalBytes = atlasSize.Width * atlasSize.Height * 4;
        byte[] atlasData = new byte[totalBytes];
        for (int i = 0; i < packedRectangles.Count; i++)
        {
            PackedRectangle packed = packedRectangles[i];
            var entry = spriteImages[packed.ImageIndex];
            Image sourceImage = entry.image;
            ShortRectangle originalRegion = entry.originalRegion;
            bool mirrorX = entry.mirrorX;
            bool mirrorY = entry.mirrorY;
            ReadOnlySpan<byte> sourceData = sourceImage.Data;
            ShortSize sourceSize = sourceImage.Size;
            for (int y = 0; y < originalRegion.Height; y++)
            {
                int srcY = mirrorY ? (originalRegion.Y + originalRegion.Height - 1 - y) : (originalRegion.Y + y);
                for (int x = 0; x < originalRegion.Width; x++)
                {
                    int srcX = mirrorX ? (originalRegion.X + originalRegion.Width - 1 - x) : (originalRegion.X + x);
                    int sourceIndex = (srcY * sourceSize.Width + srcX) * 4;
                    int atlasX = packed.Rectangle.X + Padding + x;
                    int atlasY = packed.Rectangle.Y + Padding + y;
                    int atlasIndex = (atlasY * atlasSize.Width + atlasX) * 4;
                    if (sourceIndex + 3 < sourceData.Length && atlasIndex + 3 < atlasData.Length)
                    {
                        atlasData[atlasIndex] = sourceData[sourceIndex];
                        atlasData[atlasIndex + 1] = sourceData[sourceIndex + 1];
                        atlasData[atlasIndex + 2] = sourceData[sourceIndex + 2];
                        atlasData[atlasIndex + 3] = sourceData[sourceIndex + 3];
                    }
                }
            }
        }
        
        // TODO: make sure pixel format is aligned
        return new RawImage(atlasData, atlasSize, PixelFormat.Rgba8888);
    }
}
