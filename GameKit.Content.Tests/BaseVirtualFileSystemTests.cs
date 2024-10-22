using Assert = NUnit.Framework.Assert;

namespace GameKit.Content.Tests;

public abstract class BaseVirtualFileSystemTests
{
    protected VirtualFileSystem FileSystem { get; set; } = DictFileSystem.Empty;

    [Test]
    public void GetFilesFromRootSucceeds()
    {
        // act
        var files = FileSystem.GetFiles(".");

        // assert
        string[] expected = ["a.txt", "b.txt"];
        VirtualFile[] items = files.ToArray();
        CollectionAssert.AreEquivalent(expected, items.Select(x => x.Path));
    }
    
    [Test]
    public void GetDirectoriesFromRootSucceeds()
    {
        // act
        var dirs = FileSystem.GetDirectories(".");

        // assert
        string[] expected = ["dir1", "dir2"];
        string[] items = dirs.ToArray();
        CollectionAssert.AreEquivalent(expected, items);
    }
    
    [Test]
    public void GetFilesFromSubdirectorySucceeds()
    {
        // act
        var files = FileSystem.GetFiles("dir1");

        // assert
        string[] expected = ["dir1/dir1a.txt", "dir1/dir1b.txt"];
        VirtualFile[] items = files.ToArray();
        CollectionAssert.AreEquivalent(expected, items.Select(x => x.Path));
    }
    
    [Test]
    public void GetFilesFromNonexistentDirectoryThrowsDirectoryNotFoundException()
    {
        Assert.Throws<DirectoryNotFoundException>(() => FileSystem.GetFiles("nonexistent"));
    }
    
    [Test]
    public void GetDirectoriesFromNonexistentDirectoryThrowsDirectoryNotFoundException()
    {
        Assert.Throws<DirectoryNotFoundException>(() => FileSystem.GetDirectories("nonexistent"));
    }
    
    [Test]
    public void OpenStreamFromFileSucceeds()
    {
        // act
        using var stream = FileSystem.OpenStream("a.txt");
        using StreamReader reader = new StreamReader(stream);
        var fileContents = reader.ReadToEnd();
        
        // assert
        Assert.That(fileContents, Is.EqualTo("Hello a"));
    }
    
    [TearDown]
    public void Teardown()
    {
        FileSystem.Dispose();
    }
}