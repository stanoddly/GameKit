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

    private static ShortRectangle NormalizeRegion(ShortRectangle region)
    {
        short x = region.X;
        short y = region.Y;
        short width = region.Width;
        short height = region.Height;
        if (width < 0)
        {
            x = (short)(x + width + 1);
            width = (short)-width;
        }
        if (height < 0)
        {
            y = (short)(y + height + 1);
            height = (short)-height;
        }
        return new ShortRectangle(x, y, width, height);
    }

    public void BuildSprites(params string[] directories)
    {
        var entries = new List<(string path, string texturePath, ShortRectangle originalRegion,
            bool isAnimatedFrame, string? animationPath, double frameDuration, bool repeat,
            int frameIndex, int totalFrames)>();
        var animatedSpriteInfos = new Dictionary<string, (double frameDuration, bool repeat, List<int> frameIndices)>();

        foreach (var directory in directories)
        {
            CollectSpritesRecursively(directory, entries, animatedSpriteInfos);
        }

        if (entries.Count == 0)
        {
            return;
        }

        // Deduplicate by (texturePath, normalizedRegion) — mirrored sprites share atlas space
        var dedupMap = new Dictionary<(string texturePath, ShortRectangle normalizedRegion), int>();
        var uniqueImages = new List<(Image image, ShortRectangle normalizedRegion)>();
        var entryToUnique = new int[entries.Count];

        for (int i = 0; i < entries.Count; i++)
        {
            ShortRectangle normalized = NormalizeRegion(entries[i].originalRegion);
            var key = (entries[i].texturePath, normalized);
            if (!dedupMap.TryGetValue(key, out int uniqueIndex))
            {
                uniqueIndex = uniqueImages.Count;
                Image image = _imageLoader.Load(entries[i].texturePath);
                uniqueImages.Add((image, normalized));
                dedupMap[key] = uniqueIndex;
            }
            entryToUnique[i] = uniqueIndex;
        }

        // Sort unique images by area descending for packing
        var sortedIndices = Enumerable.Range(0, uniqueImages.Count).ToList();
        sortedIndices.Sort((a, b) =>
        {
            int areaA = uniqueImages[a].normalizedRegion.Width * uniqueImages[a].normalizedRegion.Height;
            int areaB = uniqueImages[b].normalizedRegion.Width * uniqueImages[b].normalizedRegion.Height;
            return areaB.CompareTo(areaA);
        });

        // Build a reordered list for packing and a mapping back
        var sortedImages = new List<(Image image, ShortRectangle normalizedRegion)>(uniqueImages.Count);
        var sortedToOriginal = new int[uniqueImages.Count];
        var originalToSorted = new int[uniqueImages.Count];
        for (int i = 0; i < sortedIndices.Count; i++)
        {
            sortedToOriginal[i] = sortedIndices[i];
            originalToSorted[sortedIndices[i]] = i;
            sortedImages.Add(uniqueImages[sortedIndices[i]]);
        }

        // Pack
        (ShortSize atlasSize, List<PackedRectangle> packedRectangles) = PackImagesIntoAtlas(sortedImages);

        // Create atlas image (no mirroring — mirroring is handled by UV sign)
        RawImage atlasImage = CreateAtlasImage(sortedImages, packedRectangles, atlasSize);
        Texture atlasTexture = _textureLoader.Load(atlasImage);

        // Store static sprites
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (entry.isAnimatedFrame)
            {
                continue;
            }

            int uniqueIndex = entryToUnique[i];
            int sortedIndex = originalToSorted[uniqueIndex];
            PackedRectangle packed = packedRectangles[sortedIndex];
            ShortRectangle originalRegion = entry.originalRegion;

            short atlasX = (short)(packed.Rectangle.X + Padding);
            short atlasY = (short)(packed.Rectangle.Y + Padding);
            ShortRectangle atlasRegion = new ShortRectangle(atlasX, atlasY, originalRegion.Width, originalRegion.Height);
            _storage.StoreSprite(entry.path, new SpriteAsset(atlasTexture, atlasRegion));
        }

        // Store animated sprites
        var animationFramesByPath = new Dictionary<string, ShortRectangle[]>();
        foreach (var kv in animatedSpriteInfos)
        {
            animationFramesByPath[kv.Key] = new ShortRectangle[kv.Value.frameIndices.Count];
        }

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (!entry.isAnimatedFrame || entry.animationPath == null)
            {
                continue;
            }

            int uniqueIndex = entryToUnique[i];
            int sortedIndex = originalToSorted[uniqueIndex];
            PackedRectangle packed = packedRectangles[sortedIndex];
            ShortRectangle originalRegion = entry.originalRegion;

            short atlasX = (short)(packed.Rectangle.X + Padding);
            short atlasY = (short)(packed.Rectangle.Y + Padding);
            ShortRectangle atlasRegion = new ShortRectangle(atlasX, atlasY, originalRegion.Width, originalRegion.Height);
            animationFramesByPath[entry.animationPath][entry.frameIndex] = atlasRegion;
        }

        foreach (var kv in animatedSpriteInfos)
        {
            string animationPath = kv.Key;
            var (frameDuration, repeat, _) = kv.Value;
            var frames = animationFramesByPath[animationPath];
            var immutableFrames = System.Collections.Immutable.ImmutableArray.CreateRange(frames);
            AnimatedSpriteAsset animatedSpriteAsset = new AnimatedSpriteAsset((float)frameDuration, atlasTexture, immutableFrames, repeat, Vector2.Zero);
            _storage.StoreAnimatedSprite(animationPath, animatedSpriteAsset);
        }

        atlasImage.Dispose();
    }

    private void CollectSpritesRecursively(string directory,
        List<(string path, string texturePath, ShortRectangle originalRegion,
            bool isAnimatedFrame, string? animationPath, double frameDuration, bool repeat,
            int frameIndex, int totalFrames)> entries,
        Dictionary<string, (double frameDuration, bool repeat, List<int> frameIndices)> animatedSpriteInfos)
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
                    entries.Add((file.Path, spriteDto.Texture, spriteDto.TextureRegion,
                        false, null, 0, false, 0, 0));
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
                        entries.Add((file.Path, animatedDto.Texture, animatedDto.Frames[i],
                            true, file.Path, animatedDto.FrameDuration, animatedDto.Repeat, i, totalFrames));
                        frameIndices.Add(entries.Count - 1);
                    }
                    animatedSpriteInfos[file.Path] = (animatedDto.FrameDuration, animatedDto.Repeat, frameIndices);
                }
            }
        }
        ReadOnlySpan<string> subdirectories = _fileSystem.GetDirectories(directory);
        foreach (string subdirectory in subdirectories)
        {
            CollectSpritesRecursively(subdirectory, entries, animatedSpriteInfos);
        }
    }

    private (ShortSize atlasSize, List<PackedRectangle> packedRectangles) PackImagesIntoAtlas(
        List<(Image image, ShortRectangle normalizedRegion)> images)
    {
        List<PackedRectangle> packedRectangles = new List<PackedRectangle>(images.Count);

        int atlasWidth = 1024;
        int atlasHeight = 1024;
        List<ShortRectangle> freeRectangles = new List<ShortRectangle>
        {
            new ShortRectangle(0, 0, (short)atlasWidth, (short)atlasHeight)
        };

        for (int i = 0; i < images.Count; i++)
        {
            ShortRectangle region = images[i].normalizedRegion;
            short width = (short)(region.Width + Padding * 2);
            short height = (short)(region.Height + Padding * 2);

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
        List<(Image image, ShortRectangle normalizedRegion)> images,
        List<PackedRectangle> packedRectangles,
        ShortSize atlasSize)
    {
        int totalBytes = atlasSize.Width * atlasSize.Height * 4;
        byte[] atlasData = new byte[totalBytes];

        for (int i = 0; i < packedRectangles.Count; i++)
        {
            PackedRectangle packed = packedRectangles[i];
            (Image sourceImage, ShortRectangle region) = images[packed.ImageIndex];

            ReadOnlySpan<byte> sourceData = sourceImage.Data;
            ShortSize sourceSize = sourceImage.Size;

            for (int y = 0; y < region.Height; y++)
            {
                for (int x = 0; x < region.Width; x++)
                {
                    int sourceX = region.X + x;
                    int sourceY = region.Y + y;
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
}
