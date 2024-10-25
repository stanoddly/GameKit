namespace GameKit.Content.Tests;

public class CompositeFileSystemTests: BaseVirtualFileSystemTests
{
    [SetUp]
    public void Setup()
    {
        FileSystem =
            new CompositeFileSystem([new NativeFileSystem("ContentPart1"), new NativeFileSystem("ContentPart2")]);
    }
}