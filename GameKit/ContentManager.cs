using System.IO.Abstractions;

namespace GameKit;

public interface IContentLoaderRegistration
{
    void Register(ContentManager contentManager);
}

public abstract class ContentLoader<TContent>: IContentLoaderRegistration
{
    public void Register(ContentManager contentManager)
    {
        contentManager.InjectLoader(this);
    }
    
    public abstract TContent Load(IFileSystem fileSystem, string path);
}

public class ContentManager
{
    private readonly Dictionary<Type, object> _loaders = new();
    private readonly IFileSystem _fileSystem;

    public ContentManager(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    internal void InjectLoader<TContent>(ContentLoader<TContent> loader)
    {
        _loaders.Add(typeof(TContent), loader);
    }
    
    public TContent Load<TContent>(string path)
    {
        if (!_loaders.TryGetValue(typeof(TContent), out var obj))
        {
            // TODO: improve
            throw new Exception($"Can not load content of type {typeof(TContent).Name}");
        }
            
        // TODO: it's safe to use unsafe cast here
        ContentLoader<TContent> loader = (ContentLoader<TContent>)obj;
            
        return loader.Load(_fileSystem, path);
    }
}
