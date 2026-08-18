using System.IO.Compression;
using Assert = NUnit.Framework.Assert;

namespace Pixely.Content.Tests;

public sealed class ZipFileSystemTests
{
    private DirectoryInfo _temporaryDirectory = null!;

    [SetUp]
    public void Setup()
    {
        _temporaryDirectory = Directory.CreateTempSubdirectory("Pixely.Content.Tests-");
    }

    [Test]
    public void GetDirectoriesFromLeafDirectoryReturnsEmpty()
    {
        using ZipFileSystem fileSystem = CreateFileSystem(["sprites/terrain/ground.json"]);

        ReadOnlySpan<VirtualFile> files = fileSystem.GetFiles("sprites/terrain");
        ReadOnlySpan<string> directories = fileSystem.GetDirectories("sprites/terrain");

        Assert.That(files.ToArray().Select(file => file.Path),
            Is.EquivalentTo(new[] { "sprites/terrain/ground.json" }));
        Assert.That(directories.Length, Is.Zero);
    }

    [Test]
    public void GetDirectoriesFromNestedDirectoryReturnsRootRelativePaths()
    {
        using ZipFileSystem fileSystem = CreateFileSystem(["sprites/terrain/ground.json"]);

        ReadOnlySpan<string> directories = fileSystem.GetDirectories("sprites");

        Assert.That(directories.ToArray(), Is.EquivalentTo(new[] { "sprites/terrain" }));
    }

    [Test]
    public void GetDirectoriesIncludesExplicitEmptyDirectories()
    {
        using ZipFileSystem fileSystem = CreateFileSystem([], ["sprites/empty/"]);

        ReadOnlySpan<string> directories = fileSystem.GetDirectories("sprites");
        ReadOnlySpan<VirtualFile> files = fileSystem.GetFiles("sprites/empty");
        ReadOnlySpan<string> emptyDirectoryChildren = fileSystem.GetDirectories("sprites/empty");

        Assert.That(directories.ToArray(), Is.EquivalentTo(new[] { "sprites/empty" }));
        Assert.That(files.Length, Is.Zero);
        Assert.That(emptyDirectoryChildren.Length, Is.Zero);
    }

    [TearDown]
    public void Teardown()
    {
        _temporaryDirectory.Delete(true);
    }

    private ZipFileSystem CreateFileSystem(string[] filePaths, string[]? directoryPaths = null)
    {
        string archivePath = Path.Combine(_temporaryDirectory.FullName, "Content.pk3");

        using (ZipArchive archive = System.IO.Compression.ZipFile.Open(archivePath, ZipArchiveMode.Create))
        {
            foreach (string directoryPath in directoryPaths ?? [])
            {
                archive.CreateEntry(directoryPath);
            }

            foreach (string filePath in filePaths)
            {
                archive.CreateEntry(filePath);
            }
        }

        return ZipFileSystem.Create(archivePath);
    }
}
