namespace GameKit.Content.Tests;

public class NativeFileSystemTests: BaseVirtualFileSystemTests
{
    [SetUp]
    public void Setup()
    {
        FileSystem = new NativeFileSystem("Content");
    }
}