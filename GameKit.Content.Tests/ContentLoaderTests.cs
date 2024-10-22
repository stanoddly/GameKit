namespace GameKit.Content.Tests;

public class ContentLoaderTests
{
    public record ContentA;
    
    private class LoaderA : IContentLoader<ContentA>
    {
        public ContentA Load(IContentManager contentManager, VirtualFileSystem virtualFileSystem, string path)
        {
            throw new NotImplementedException();
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