using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text;
using GameKit.Common;
using GameKit.Content;
using GameKit.Gpu;
using GameKit.Sprites;
using Microsoft.Extensions.DependencyInjection;

namespace GameKit.Tests;

public class AtlasPackerTests
{
    private static byte[] MakeSpriteJson(short x, short y, ushort w, ushort h, string texture = "img.png") =>
        Encoding.UTF8.GetBytes($$"""{"texture":"{{texture}}","textureRegion":[{{x}},{{y}},{{w}},{{h}}]}""");

    private static byte[] MakeSpriteJson(short x, short y, ushort w, ushort h, SpriteFlip flip, string texture = "img.png") =>
        Encoding.UTF8.GetBytes($$"""{"texture":"{{texture}}","textureRegion":[{{x}},{{y}},{{w}},{{h}}],"flip":"{{flip}}"}""");

    private static byte[] MakeAnimatedSpriteJson(string texture, double frameDuration, bool repeat, params ShortRectangle[] frames)
    {
        var sb = new StringBuilder();
        sb.Append($$"""{"texture":"{{texture}}","frameDuration":{{frameDuration}},"repeat":{{(repeat ? "true" : "false")}},"frames":[""");
        for (int i = 0; i < frames.Length; i++)
        {
            if (i > 0) sb.Append(',');
            var f = frames[i];
            sb.Append($"[{f.X},{f.Y},{f.Width},{f.Height}]");
        }
        sb.Append("]}");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static RawImage MakeImage(ushort width, ushort height)
    {
        byte[] data = new byte[width * height * 4];
        // Fill with recognizable pattern: pixel (x,y) gets RGBA = (x%256, y%256, 0, 255)
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            int i = (y * width + x) * 4;
            data[i] = (byte)(x % 256);
            data[i + 1] = (byte)(y % 256);
            data[i + 2] = 0;
            data[i + 3] = 255;
        }
        return new RawImage(data, new ShortSize(width, height), PixelFormat.Rgba8888);
    }

    private static (SpriteAssetStorage storage, SpriteAtlasBuilder builder) BuildAtlas(
        Dictionary<string, ImmutableArray<VirtualFile>> files,
        Dictionary<string, ImmutableArray<string>> directories,
        Dictionary<string, RawImage>? images = null)
    {
        var fs = new DictFileSystem(files.ToFrozenDictionary(), directories.ToFrozenDictionary());
        var storage = new SpriteAssetStorage();
        images ??= new Dictionary<string, RawImage>
        {
            ["img.png"] = MakeImage(256, 256)
        };

        var services = new ServiceCollection();
        services.AddSingleton<SpriteAtlasBuilderConfig>(new SpriteAtlasBuilderConfig(directories.Keys.ToArray()));
        services.AddSingleton<ITextureLoader>(new StubTextureLoader());
        services.AddSingleton<IContentLoader<Image>>(new StubImageLoader(images));
        services.AddSingleton<VirtualFileSystem>(fs);
        services.AddSingleton(storage);
        var sp = services.BuildServiceProvider();

        var builder = SpriteAtlasBuilder.Create(sp);
        return (storage, builder);
    }

    [Test]
    public void SingleSprite_StoredWithCorrectRegionSize()
    {
        var (storage, _) = BuildAtlas(
            new()
            {
                ["sprites"] =
                [
                    new ByteVirtualFile("sprites/hero.json", MakeSpriteJson(0, 0, 32, 32))
                ]
            },
            new() { ["sprites"] = [] });

        Assert.That(storage.TryGetSprite("sprites/hero.json", out var sprite), Is.True);
        Assert.That(sprite!.ImageRegion.Width, Is.EqualTo(32));
        Assert.That(sprite.ImageRegion.Height, Is.EqualTo(32));
    }

    [Test]
    public void MultipleSprites_AllStoredWithCorrectSizes()
    {
        var (storage, _) = BuildAtlas(
            new()
            {
                ["sprites"] =
                [
                    new ByteVirtualFile("sprites/a.json", MakeSpriteJson(0, 0, 64, 48)),
                    new ByteVirtualFile("sprites/b.json", MakeSpriteJson(0, 0, 100, 80)),
                    new ByteVirtualFile("sprites/c.json", MakeSpriteJson(0, 0, 16, 16)),
                ]
            },
            new() { ["sprites"] = [] });

        Assert.That(storage.TryGetSprite("sprites/a.json", out var a), Is.True);
        Assert.That(storage.TryGetSprite("sprites/b.json", out var b), Is.True);
        Assert.That(storage.TryGetSprite("sprites/c.json", out var c), Is.True);

        Assert.That(a!.ImageRegion.Width, Is.EqualTo(64));
        Assert.That(a.ImageRegion.Height, Is.EqualTo(48));
        Assert.That(b!.ImageRegion.Width, Is.EqualTo(100));
        Assert.That(b.ImageRegion.Height, Is.EqualTo(80));
        Assert.That(c!.ImageRegion.Width, Is.EqualTo(16));
        Assert.That(c.ImageRegion.Height, Is.EqualTo(16));
    }

    [Test]
    public void MultipleSprites_RegionsDontOverlap()
    {
        var (storage, _) = BuildAtlas(
            new()
            {
                ["sprites"] =
                [
                    new ByteVirtualFile("sprites/a.json", MakeSpriteJson(0, 0, 100, 80)),
                    new ByteVirtualFile("sprites/b.json", MakeSpriteJson(0, 0, 60, 120)),
                    new ByteVirtualFile("sprites/c.json", MakeSpriteJson(0, 0, 200, 50)),
                    new ByteVirtualFile("sprites/d.json", MakeSpriteJson(0, 0, 90, 90)),
                    new ByteVirtualFile("sprites/e.json", MakeSpriteJson(0, 0, 150, 70)),
                ]
            },
            new() { ["sprites"] = [] });

        var regions = new List<ShortRectangle>();
        foreach (string name in new[] { "sprites/a.json", "sprites/b.json", "sprites/c.json", "sprites/d.json", "sprites/e.json" })
        {
            Assert.That(storage.TryGetSprite(name, out var s), Is.True, $"Sprite {name} not found");
            regions.Add(s!.ImageRegion);
        }

        for (int i = 0; i < regions.Count; i++)
        for (int j = i + 1; j < regions.Count; j++)
        {
            Assert.That(RectanglesOverlap(regions[i], regions[j]), Is.False,
                $"Regions {i} and {j} overlap: {regions[i]} vs {regions[j]}");
        }
    }

    [Test]
    public void SpriteWithSubRegion_PreservesRegionSize()
    {
        var (storage, _) = BuildAtlas(
            new()
            {
                ["sprites"] =
                [
                    new ByteVirtualFile("sprites/sub.json", MakeSpriteJson(10, 20, 50, 40))
                ]
            },
            new() { ["sprites"] = [] });

        Assert.That(storage.TryGetSprite("sprites/sub.json", out var sprite), Is.True);
        Assert.That(sprite!.ImageRegion.Width, Is.EqualTo(50));
        Assert.That(sprite.ImageRegion.Height, Is.EqualTo(40));
    }

    [Test]
    public void HorizontalFlip_PreservesSpriteFlip()
    {
        var (storage, _) = BuildAtlas(
            new()
            {
                ["sprites"] =
                [
                    new ByteVirtualFile("sprites/mirror.json", MakeSpriteJson(0, 0, 30, 40, SpriteFlip.Horizontal))
                ]
            },
            new() { ["sprites"] = [] });

        Assert.That(storage.TryGetSprite("sprites/mirror.json", out var sprite), Is.True);
        Assert.That(sprite!.Flip, Is.EqualTo(SpriteFlip.Horizontal));
        Assert.That(sprite.ImageRegion.Width, Is.EqualTo(30));
        Assert.That(sprite.ImageRegion.Height, Is.EqualTo(40));
    }

    [Test]
    public void VerticalFlip_PreservesSpriteFlip()
    {
        var (storage, _) = BuildAtlas(
            new()
            {
                ["sprites"] =
                [
                    new ByteVirtualFile("sprites/flip.json", MakeSpriteJson(0, 0, 30, 40, SpriteFlip.Vertical))
                ]
            },
            new() { ["sprites"] = [] });

        Assert.That(storage.TryGetSprite("sprites/flip.json", out var sprite), Is.True);
        Assert.That(sprite!.Flip, Is.EqualTo(SpriteFlip.Vertical));
        Assert.That(sprite.ImageRegion.Width, Is.EqualTo(30));
        Assert.That(sprite.ImageRegion.Height, Is.EqualTo(40));
    }

    [Test]
    public void BothFlip_PreservesSpriteFlip()
    {
        var (storage, _) = BuildAtlas(
            new()
            {
                ["sprites"] =
                [
                    new ByteVirtualFile("sprites/both.json", MakeSpriteJson(0, 0, 30, 40, SpriteFlip.Both))
                ]
            },
            new() { ["sprites"] = [] });

        Assert.That(storage.TryGetSprite("sprites/both.json", out var sprite), Is.True);
        Assert.That(sprite!.Flip, Is.EqualTo(SpriteFlip.Both));
        Assert.That(sprite.ImageRegion.Width, Is.EqualTo(30));
        Assert.That(sprite.ImageRegion.Height, Is.EqualTo(40));
    }

    [Test]
    public void NoFlip_DefaultsToNone()
    {
        var (storage, _) = BuildAtlas(
            new()
            {
                ["sprites"] =
                [
                    new ByteVirtualFile("sprites/normal.json", MakeSpriteJson(0, 0, 32, 32))
                ]
            },
            new() { ["sprites"] = [] });

        Assert.That(storage.TryGetSprite("sprites/normal.json", out var sprite), Is.True);
        Assert.That(sprite!.Flip, Is.EqualTo(SpriteFlip.None));
    }

    [Test]
    public void AnimatedSprite_AllFramesStored()
    {
        var frames = new[]
        {
            new ShortRectangle(0, 0, 32, 32),
            new ShortRectangle(32, 0, 32, 32),
            new ShortRectangle(64, 0, 32, 32),
        };
        var (storage, _) = BuildAtlas(
            new()
            {
                ["sprites"] =
                [
                    new ByteVirtualFile("sprites/walk.json", MakeAnimatedSpriteJson("img.png", 0.1, true, frames))
                ]
            },
            new() { ["sprites"] = [] });

        Assert.That(storage.TryGetAnimatedSprite("sprites/walk.json", out var anim), Is.True);
        Assert.That(anim!.Frames.Length, Is.EqualTo(3));
        foreach (var frame in anim.Frames)
        {
            Assert.That(frame.Width, Is.EqualTo(32));
            Assert.That(frame.Height, Is.EqualTo(32));
        }
    }

    [Test]
    public void AnimatedSprite_FramesDontOverlap()
    {
        var frames = new[]
        {
            new ShortRectangle(0, 0, 48, 48),
            new ShortRectangle(48, 0, 48, 48),
            new ShortRectangle(96, 0, 48, 48),
            new ShortRectangle(144, 0, 48, 48),
        };
        var (storage, _) = BuildAtlas(
            new()
            {
                ["sprites"] =
                [
                    new ByteVirtualFile("sprites/run.json", MakeAnimatedSpriteJson("img.png", 0.05, false, frames))
                ]
            },
            new() { ["sprites"] = [] });

        Assert.That(storage.TryGetAnimatedSprite("sprites/run.json", out var anim), Is.True);
        for (int i = 0; i < anim!.Frames.Length; i++)
        for (int j = i + 1; j < anim.Frames.Length; j++)
        {
            Assert.That(RectanglesOverlap(anim.Frames[i], anim.Frames[j]), Is.False,
                $"Animated frames {i} and {j} overlap");
        }
    }

    [Test]
    public void AnimatedSprite_PropertiesPreserved()
    {
        var frames = new[]
        {
            new ShortRectangle(0, 0, 32, 32),
            new ShortRectangle(32, 0, 32, 32),
        };
        var (storage, _) = BuildAtlas(
            new()
            {
                ["sprites"] =
                [
                    new ByteVirtualFile("sprites/anim.json", MakeAnimatedSpriteJson("img.png", 0.25, true, frames))
                ]
            },
            new() { ["sprites"] = [] });

        Assert.That(storage.TryGetAnimatedSprite("sprites/anim.json", out var anim), Is.True);
        Assert.That(anim!.FrameDuration, Is.EqualTo(0.25f));
        Assert.That(anim.Repeat, Is.True);
    }

    [Test]
    public void RecursiveDirectories_AllSpritesFound()
    {
        var (storage, _) = BuildAtlas(
            new()
            {
                ["root"] =
                [
                    new ByteVirtualFile("root/a.json", MakeSpriteJson(0, 0, 32, 32))
                ],
                ["root/sub"] =
                [
                    new ByteVirtualFile("root/sub/b.json", MakeSpriteJson(0, 0, 64, 64))
                ]
            },
            new()
            {
                ["root"] = ["root/sub"],
                ["root/sub"] = []
            });

        Assert.That(storage.TryGetSprite("root/a.json", out _), Is.True);
        Assert.That(storage.TryGetSprite("root/sub/b.json", out _), Is.True);
    }

    [Test]
    public void MixedStaticAndAnimated_BothTypesStored()
    {
        var frames = new[]
        {
            new ShortRectangle(0, 0, 32, 32),
            new ShortRectangle(32, 0, 32, 32),
        };
        var (storage, _) = BuildAtlas(
            new()
            {
                ["sprites"] =
                [
                    new ByteVirtualFile("sprites/hero.json", MakeSpriteJson(0, 0, 64, 64)),
                    new ByteVirtualFile("sprites/walk.json", MakeAnimatedSpriteJson("img.png", 0.1, true, frames)),
                ]
            },
            new() { ["sprites"] = [] });

        Assert.That(storage.TryGetSprite("sprites/hero.json", out var sprite), Is.True);
        Assert.That(sprite!.ImageRegion.Width, Is.EqualTo(64));

        Assert.That(storage.TryGetAnimatedSprite("sprites/walk.json", out var anim), Is.True);
        Assert.That(anim!.Frames.Length, Is.EqualTo(2));
    }

    [Test]
    public void EmptyDirectory_NoCrash()
    {
        var (storage, _) = BuildAtlas(
            new() { ["empty"] = [] },
            new() { ["empty"] = [] });

        Assert.That(storage.TryGetSprite("anything", out _), Is.False);
    }

    [Test]
    public void ManySprites_AllPackedWithNoOverlaps()
    {
        var files = new ImmutableArray<VirtualFile>[1];
        var sprites = new List<VirtualFile>();
        for (int i = 0; i < 30; i++)
        {
            ushort w = (ushort)(16 + i % 5 * 16);
            ushort h = (ushort)(16 + i % 7 * 16);
            sprites.Add(new ByteVirtualFile($"sprites/s{i}.json", MakeSpriteJson(0, 0, w, h)));
        }

        var (storage, _) = BuildAtlas(
            new() { ["sprites"] = [.. sprites] },
            new() { ["sprites"] = [] });

        var regions = new List<ShortRectangle>();
        for (int i = 0; i < 30; i++)
        {
            Assert.That(storage.TryGetSprite($"sprites/s{i}.json", out var s), Is.True, $"Sprite s{i} not found");
            regions.Add(s!.ImageRegion);
        }

        for (int i = 0; i < regions.Count; i++)
        for (int j = i + 1; j < regions.Count; j++)
        {
            Assert.That(RectanglesOverlap(regions[i], regions[j]), Is.False,
                $"Regions {i} and {j} overlap: {regions[i]} vs {regions[j]}");
        }
    }

    [Test]
    public void FlippedSprite_SharesAtlasPositionWithOriginal()
    {
        var (storage, _) = BuildAtlas(
            new()
            {
                ["sprites"] =
                [
                    new ByteVirtualFile("sprites/right.json", MakeSpriteJson(0, 0, 32, 32)),
                    new ByteVirtualFile("sprites/left.json", MakeSpriteJson(0, 0, 32, 32, SpriteFlip.Horizontal)),
                ]
            },
            new() { ["sprites"] = [] });

        Assert.That(storage.TryGetSprite("sprites/right.json", out var right), Is.True);
        Assert.That(storage.TryGetSprite("sprites/left.json", out var left), Is.True);
        Assert.That(right!.Flip, Is.EqualTo(SpriteFlip.None));
        Assert.That(left!.Flip, Is.EqualTo(SpriteFlip.Horizontal));
        Assert.That(left.ImageRegion.X, Is.EqualTo(right.ImageRegion.X));
        Assert.That(left.ImageRegion.Y, Is.EqualTo(right.ImageRegion.Y));
        Assert.That(left.ImageRegion.Width, Is.EqualTo(right.ImageRegion.Width));
        Assert.That(left.ImageRegion.Height, Is.EqualTo(right.ImageRegion.Height));
    }

    [Test]
    public void DifferentSourceImages_AllPacked()
    {
        var images = new Dictionary<string, RawImage>
        {
            ["tex1.png"] = MakeImage(128, 128),
            ["tex2.png"] = MakeImage(64, 64),
        };

        var (storage, _) = BuildAtlas(
            new()
            {
                ["sprites"] =
                [
                    new ByteVirtualFile("sprites/a.json",
                        Encoding.UTF8.GetBytes("""{"texture":"tex1.png","textureRegion":[0,0,50,50]}""")),
                    new ByteVirtualFile("sprites/b.json",
                        Encoding.UTF8.GetBytes("""{"texture":"tex2.png","textureRegion":[0,0,30,30]}""")),
                ]
            },
            new() { ["sprites"] = [] },
            images);

        Assert.That(storage.TryGetSprite("sprites/a.json", out var a), Is.True);
        Assert.That(storage.TryGetSprite("sprites/b.json", out var b), Is.True);
        Assert.That(a!.ImageRegion.Width, Is.EqualTo(50));
        Assert.That(b!.ImageRegion.Width, Is.EqualTo(30));
        Assert.That(RectanglesOverlap(a.ImageRegion, b.ImageRegion), Is.False);
    }

    private static bool RectanglesOverlap(ShortRectangle a, ShortRectangle b)
    {
        return a.X < b.X + b.Width
               && a.X + a.Width > b.X
               && a.Y < b.Y + b.Height
               && a.Y + a.Height > b.Y;
    }

    private class StubTextureLoader : ITextureLoader
    {
        public Texture Load(string path) => null!;
        public Texture Load(Image image) => null!;
    }

    private class StubImageLoader(Dictionary<string, RawImage> images) : IContentLoader<Image>
    {
        public Image Load(ReadOnlySpan<char> path) =>
            images.GetValueOrDefault(path.ToString()) ?? MakeImage(256, 256);
    }
}
