using System.Collections.Frozen;

namespace GameKit.Content;

public interface IContentLoader<out TContent> where TContent: class
{
    Type SupportedType => typeof(TContent);
    TContent Load(IContentManager contentManager, VirtualFileSystem fileSystem, string path);
}

public interface IContentManager
{
    public TContent Load<TContent>(string path) where TContent: class;
}

public sealed class ContentManager: IContentManager
{
    private readonly FrozenDictionary<Type, IContentLoader<object>> _loaders;
    private readonly VirtualFileSystem _virtualFileSystem;

    public ContentManager(VirtualFileSystem virtualFileSystem, IEnumerable<IContentLoader<object>> contentLoaders)
    {
        _virtualFileSystem = virtualFileSystem;

        _loaders = contentLoaders.ToFrozenDictionary(item => item.SupportedType);
    }
    
    public TContent Load<TContent>(string path) where TContent: class
    {
        if (!_loaders.TryGetValue(typeof(TContent), out var obj))
        {
            throw new NotSupportedException($"\"IContentLoader<{typeof(TContent).Name}>\" hasn't been registered.");
        }
        
        IContentLoader<TContent> loader = (IContentLoader<TContent>)obj;
            
        return loader.Load(this, _virtualFileSystem, path);
    }
}
