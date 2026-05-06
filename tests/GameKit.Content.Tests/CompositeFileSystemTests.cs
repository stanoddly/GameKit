using Assert = NUnit.Framework.Assert;

namespace GameKit.Content.Tests;

public class CompositeFileSystemTests: BaseVirtualFileSystemTests
{
    [SetUp]
    public void Setup()
    {
        FileSystem =
            new CompositeFileSystem([new NativeFileSystem("ContentPart1"), new NativeFileSystem("ContentPart2")]);
    }

    [Test]
    public void GetFilesSucceedsWhenEarlierFileSystemLacksDirectory()
    {
        ReadOnlySpan<VirtualFile> files = FileSystem.GetFiles("dir2");

        string[] expected = ["dir2/dir2a.txt", "dir2/dir2b.txt"];
        VirtualFile[] items = files.ToArray();
        Assert.That(items.Select(x => x.Path), Is.EquivalentTo(expected));
    }
}
