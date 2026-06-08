namespace GameKit.Content.Tests;

public class FileSystemBuilderTests
{
    [Test]
    public void CreateReturnsNativeFileSystemDirectly()
    {
        // arrange
        VirtualFileSystem fileSystem = new FileSystemBuilder()
            .AddContentFromDirectory("Content")
            .Create();
        
        // assert
        Assert.That(fileSystem is NativeFileSystem);
    }
    
    [Test]
    public void CreateReturnsCompositeFileSystem()
    {
        // arrange
        VirtualFileSystem fileSystem = new FileSystemBuilder()
            .AddContentFromDirectory("ContentPart1")
            .AddContentFromDirectory("ContentPart2")
            .Create();
        
        // assert
        Assert.That(fileSystem is CompositeFileSystem);
    }
    
    [Test]
    public void CreateReturnsCachedFileSystem()
    {
        // arrange
        VirtualFileSystem fileSystem = new FileSystemBuilder()
            .AddContentFromDirectory("Content")
            .WithCache()
            .Create();
        
        // assert
        Assert.That(fileSystem is CachedFileSystem);
    }
    
    [Test]
    public void CreateReturnsNativeFileSystemFromProjectsDirectory()
    {
        // arrange
        VirtualFileSystem fileSystem = new FileSystemBuilder()
            .AddContentFromProjectDirectory("ContentInDevRoot")
            .Create();
        
        // assert
        Assert.That(fileSystem is NativeFileSystem);
        NativeFileSystem nativeFileSystem = (NativeFileSystem)fileSystem;
        var expectedPath = Path.Join(typeof(FileSystemBuilderTests).Namespace, "ContentInDevRoot");
        Assert.That(nativeFileSystem.RootPath.EndsWith(expectedPath));
    }

    [Test]
    public void AddContentFromProjectDirectoryPrefersAppBaseDirectoryWhenContentExists()
    {
        // arrange
        string expectedPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "Content"));
        // This test exercises the app-base-directory branch, which only applies when the
        // directory actually exists next to the test assembly (copied at build time).
        Assert.That(Directory.Exists(expectedPath), $"Expected '{expectedPath}' to exist next to the test assembly.");
        VirtualFileSystem fileSystem = new FileSystemBuilder()
            .AddContentFromProjectDirectory("Content")
            .Create();

        // assert
        Assert.That(fileSystem is NativeFileSystem);
        NativeFileSystem nativeFileSystem = (NativeFileSystem)fileSystem;
        Assert.That(nativeFileSystem.RootPath, Is.EqualTo(expectedPath));
    }
}
