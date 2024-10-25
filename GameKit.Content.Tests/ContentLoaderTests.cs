namespace GameKit.Content.Tests;

public class ContentLoaderTests
{
    private record ContentA;
    
    private class LoaderA : IContentLoader<ContentA>
    {
        public ContentA Load(IContentManager contentManager, VirtualFileSystem fileSystem, string path)
        {
            return new ContentA();
        }
    }
    
    
    [Test]
    public void SupportedTypeFromCovariantReturnsCorrectType()
    {
        // arrange
        IContentLoader<object> contentLoader = new LoaderA();
        
        // act
        Type type = contentLoader.SupportedType;
        
        // assert
        Assert.That(type, Is.EqualTo(typeof(ContentA)));
    }
}
