using System.Collections.Frozen;

namespace GameKit.Content;

public interface IContentLoader<out TContent>: IDisposable where TContent: class
{
    Type SupportedType => typeof(TContent);
    TContent Load(IContentManager contentManager, VirtualFileSystem fileSystem, string path);

    void IDisposable.Dispose()
    {
    }
}

public interface IContentManager: IDisposable
{
    public TContent Load<TContent>(string path) where TContent: class;
}

public sealed class ContentManager: IContentManager
{
    private readonly FrozenDictionary<Type, IContentLoader<object>> _loaders;
    private readonly VirtualFileSystem _fileSystem;

    public ContentManager(VirtualFileSystem fileSystem, IEnumerable<IContentLoader<object>> contentLoaders)
    {
        _fileSystem = fileSystem;

        _loaders = contentLoaders.ToFrozenDictionary(item => item.SupportedType);
    }

    public TContent Load<TContent>(string path) where TContent: class
    {
        if (!_loaders.TryGetValue(typeof(TContent), out var obj))
        {
            throw new NotSupportedException($"\"IContentLoader<{typeof(TContent).Name}>\" hasn't been registered.");
        }
        
        IContentLoader<TContent> loader = (IContentLoader<TContent>)obj;
            
        return loader.Load(this, _fileSystem, path);
    }

    public Stream OpenStream(string path)
    {
        return _fileSystem.OpenStream(path);
    }

    public void Dispose()
    {
        List<Exception> exceptions = new List<Exception>();
        foreach (var loader in _loaders.Values)
        {
            try
            {
                loader.Dispose();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
        {
            throw new AggregateException(exceptions);
        }
    }
}
