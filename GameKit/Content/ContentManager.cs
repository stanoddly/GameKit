namespace GameKit.Content;

public interface IContentLoader<out TContent> where TContent: class
{
    Type SupportedType => typeof(TContent);
    TContent Load(IContentManager contentManager, VirtualFileSystem virtualFileSystem, string path);
}

public interface IContentManager
{
    public TContent Load<TContent>(string path) where TContent: class;
    public Stream OpenStream(string path);
}

public sealed class ContentManager: IContentManager
{
    private readonly Dictionary<Type, IContentLoader<object>> _loaders = new();
    private readonly VirtualFileSystem _virtualFileSystem;

    public ContentManager(VirtualFileSystem virtualFileSystem, IEnumerable<IContentLoader<object>> contentLoaders)
    {
        _virtualFileSystem = virtualFileSystem;

        foreach (IContentLoader<object> contentLoader in contentLoaders)
        {
            _loaders.Add(contentLoader.SupportedType, contentLoader);
        }
    }
    
    public TContent Load<TContent>(string path) where TContent: class
    {
        if (!_loaders.TryGetValue(typeof(TContent), out var obj))
        {
            // TODO: improve
            throw new Exception($"Can not load content of type {typeof(TContent).Name}");
        }

        // TODO: isn't it safe to use unsafe cast here?
        IContentLoader<TContent> loader = (IContentLoader<TContent>)obj;
            
        return loader.Load(this, _virtualFileSystem, path);
    }

    public Stream OpenStream(string path)
    {
        throw new NotImplementedException();
    }
}
