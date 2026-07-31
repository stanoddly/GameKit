using System.IO.Compression;

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

    [Test]
    [NonParallelizable]
    public void AddContentFromZipPatternResolvesPatternRelativeToAppBaseDirectory()
    {
        string originalWorkingDirectory = Directory.GetCurrentDirectory();
        string archiveFilename = $"content-{Guid.NewGuid():N}.pk3";
        string archivePath = Path.Combine(AppContext.BaseDirectory, archiveFilename);
        DirectoryInfo temporaryWorkingDirectory = Directory.CreateTempSubdirectory("GameKit.Content.Tests-");

        try
        {
            using (ZipArchive archive = System.IO.Compression.ZipFile.Open(archivePath, ZipArchiveMode.Create))
            {
                ZipArchiveEntry entry = archive.CreateEntry("marker.txt");
                using StreamWriter writer = new(entry.Open());
                writer.Write("from app directory");
            }

            Directory.SetCurrentDirectory(temporaryWorkingDirectory.FullName);

            using VirtualFileSystem fileSystem = new FileSystemBuilder()
                .AddContentFromZipPattern(archiveFilename)
                .Create();
            using StreamReader reader = new(fileSystem.OpenStream("marker.txt"));

            Assert.That(reader.ReadToEnd(), Is.EqualTo("from app directory"));
        }
        finally
        {
            Directory.SetCurrentDirectory(originalWorkingDirectory);
            File.Delete(archivePath);
            if (temporaryWorkingDirectory.Exists)
            {
                temporaryWorkingDirectory.Delete(true);
            }
        }
    }
}
