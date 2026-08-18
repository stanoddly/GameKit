using Assert = NUnit.Framework.Assert;

namespace Pixely.Content.Tests;

public abstract class BaseVirtualFileSystemTests
{
    // this is supposed to be assigned in a derived class
    protected VirtualFileSystem FileSystem { get; set; } = DictFileSystem.Empty;

    [Test]
    public void GetFilesFromRootSucceeds()
    {
        // act
        ReadOnlySpan<VirtualFile> files = FileSystem.GetFiles(".");

        // assert
        string[] expected = ["a.txt", "b.txt"];
        VirtualFile[] items = files.ToArray();
        Assert.That(items.Select(x => x.Path), Is.EquivalentTo(expected));
    }
    
    [Test]
    public void GetDirectoriesFromRootSucceeds()
    {
        // act
        ReadOnlySpan<string> dirs = FileSystem.GetDirectories(".");

        // assert
        string[] expected = ["dir1", "dir2"];
        string[] items = dirs.ToArray();
        Assert.That(items, Is.EquivalentTo(expected));
    }
    
    [Test]
    public void GetFilesFromSubdirectorySucceeds()
    {
        // act
        ReadOnlySpan<VirtualFile> files = FileSystem.GetFiles("dir1");

        // assert
        string[] expected = ["dir1/dir1a.txt", "dir1/dir1b.txt"];
        VirtualFile[] items = files.ToArray();
        Assert.That(items.Select(x => x.Path), Is.EquivalentTo(expected));
    }

    [Test]
    public void TryGetFilesFromSubdirectorySucceeds()
    {
        bool found = FileSystem.TryGetFiles("dir1", out ReadOnlySpan<VirtualFile> files);

        string[] expected = ["dir1/dir1a.txt", "dir1/dir1b.txt"];
        VirtualFile[] items = files.ToArray();
        Assert.That(found, Is.True);
        Assert.That(items.Select(x => x.Path), Is.EquivalentTo(expected));
    }
    
    [Test]
    public void GetFilesFromNonexistentDirectoryThrowsDirectoryNotFoundException()
    {
        Assert.Throws<DirectoryNotFoundException>(() => FileSystem.GetFiles("nonexistent"));
    }

    [Test]
    public void TryGetFilesFromNonexistentDirectoryReturnsFalse()
    {
        bool found = FileSystem.TryGetFiles("nonexistent", out ReadOnlySpan<VirtualFile> files);

        Assert.That(found, Is.False);
        Assert.That(files.Length, Is.EqualTo(0));
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
        using Stream stream = FileSystem.OpenStream("a.txt");
        using StreamReader reader = new StreamReader(stream);
        string fileContents = reader.ReadToEnd();
        
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
