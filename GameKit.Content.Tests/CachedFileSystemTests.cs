namespace GameKit.Content.Tests;

public class CachedFileSystemTests: BaseVirtualFileSystemTests
{
    [SetUp]
    public void Setup()
    {
        FileSystem = CachedFileSystem.Create(new NativeFileSystem("Content"));
    }
}