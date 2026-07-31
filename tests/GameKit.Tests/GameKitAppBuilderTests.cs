using System.IO.Compression;
using GameKit.App;
using GameKit.Content;

namespace GameKit.Tests;

public class GameKitAppBuilderTests
{
    [Test]
    [NonParallelizable]
    public void AddContentFromZipPatternResolvesPatternRelativeToAppBaseDirectory()
    {
        string originalWorkingDirectory = Directory.GetCurrentDirectory();
        string archiveFilename = $"content-{Guid.NewGuid():N}.pk3";
        string archivePath = Path.Combine(AppContext.BaseDirectory, archiveFilename);
        DirectoryInfo temporaryWorkingDirectory = Directory.CreateTempSubdirectory("GameKit.Tests-");

        try
        {
            using (ZipArchive archive = System.IO.Compression.ZipFile.Open(archivePath, ZipArchiveMode.Create))
            {
                ZipArchiveEntry entry = archive.CreateEntry("marker.txt");
                using StreamWriter writer = new(entry.Open());
                writer.Write("from app directory");
            }

            Directory.SetCurrentDirectory(temporaryWorkingDirectory.FullName);

            using IGameKitApp app = new GameKitAppBuilder()
                .AddContentFromZipPattern(archiveFilename)
                .Build();
            VirtualFileSystem fileSystem = app.GetRequiredService<VirtualFileSystem>();
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
