using Assert = NUnit.Framework.Assert;

namespace GameKit.Content.Tests;

public abstract class BaseVirtualFileSystemTests
{
    // this is supposed to be assigned in a derived class
    protected VirtualFileSystem FileSystem { get; set; } = DictFileSystem.Empty;

    [Test]
    public void GetFilesFromRootSucceeds()
    {
        // act
        var files = FileSystem.GetFiles(".");

        // assert
        string[] expected = ["a.txt", "b.txt"];
        VirtualFile[] items = files.ToArray();
        Assert.That(items.Select(x => x.Path), Is.EquivalentTo(expected));
    }
    
    [Test]
    public void GetDirectoriesFromRootSucceeds()
    {
        // act
        var dirs = FileSystem.GetDirectories(".");

        // assert
        string[] expected = ["dir1", "dir2"];
        string[] items = dirs.ToArray();
        Assert.That(items, Is.EquivalentTo(expected));
    }
    
    [Test]
    public void GetFilesFromSubdirectorySucceeds()
    {
        // act
        var files = FileSystem.GetFiles("dir1");

        // assert
        string[] expected = ["dir1/dir1a.txt", "dir1/dir1b.txt"];
        VirtualFile[] items = files.ToArray();
        Assert.That(items.Select(x => x.Path), Is.EquivalentTo(expected));
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
    
    [Test]
    public void OpenStreamFromNonexistentThrowsException()
    {
        // act & assert
        Assert.Throws<FileNotFoundException>(() => FileSystem.OpenStream("nonexistent"));
    }
    
    [TearDown]
    public void Teardown()
    {
        FileSystem.Dispose();
    }
}