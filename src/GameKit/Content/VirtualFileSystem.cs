using System.Diagnostics.CodeAnalysis;

namespace GameKit.Content;

public abstract class VirtualFile
{
    public abstract string Path { get; }
    public abstract Stream Open();
}

public abstract class VirtualFileSystem: IDisposable
{
    public abstract ReadOnlySpan<VirtualFile> GetFiles(string path);
    public abstract bool TryGetDirectories(string path, out ReadOnlySpan<string> result);
    public abstract bool TryGetFile(string path, [NotNullWhen(true)] out VirtualFile? file);
    
    public ReadOnlySpan<string> GetDirectories(string path)
    {
        if (TryGetDirectories(path, out var directories))
        {
            return directories;
        }
        
        throw new DirectoryNotFoundException(path);
    }
    
    public VirtualFile GetFile(string path)
    {
        if (TryGetFile(path, out VirtualFile? contentFile))
        {
            return contentFile;
        }

        throw new FileNotFoundException(path);
    }

    public Stream OpenStream(string path)
    {
        return GetFile(path).Open();
    }

    // TODO: dispose pattern https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/dispose-pattern
    public virtual void Dispose()
    {
    }
}
