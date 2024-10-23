namespace GameKit.Content.Tests;

public class ContentTests
{
    private record ContentA(string Value);
    private class ContentLoaderA : IContentLoader<ContentA>
    {
        public ContentA Load(IContentManager contentManager, VirtualFileSystem fileSystem, string path)
        {
            using Stream stream = fileSystem.OpenStream(path);
            using StreamReader reader = new StreamReader(stream);
            return new ContentA(reader.ReadToEnd());
        }
    }
    
    private record ContentB(string Value);
    private class ContentLoaderB : IContentLoader<ContentB>
    {
        public ContentB Load(IContentManager contentManager, VirtualFileSystem fileSystem, string path)
        {
            using Stream stream = fileSystem.OpenStream(path);
            using StreamReader reader = new StreamReader(stream);
            return new ContentB(reader.ReadToEnd());
        }
    }

    private record ContentNonexistent;
    
    [Test]
    public void LoadLoadsContentProperly()
    {
        // arrange
        DictFileSystem fileSystem = DictFileSystemFactory.Create();
        ContentManager contentManager = new ContentManager(fileSystem, [new ContentLoaderA(), new ContentLoaderB()]);
        
        // act
        ContentA contentA = contentManager.Load<ContentA>("dir1/dir1a.txt");
        ContentB contentB = contentManager.Load<ContentB>("dir1/dir1b.txt");

        // assert
        Assert.That(contentA.Value, Is.EqualTo("Hello dir1a"));
        Assert.That(contentB.Value, Is.EqualTo("Hello dir1b"));
    }
    
    [Test]
    public void LoadFailsOnUnknownType()
    {
        // arrange
        DictFileSystem fileSystem = DictFileSystemFactory.Create();
        ContentManager contentManager = new ContentManager(fileSystem, [new ContentLoaderA(), new ContentLoaderB()]);
        
        // act & assert
        Assert.Throws<NotSupportedException>(() => contentManager.Load<ContentNonexistent>("dir1/dir1a.txt"));
    }
}
