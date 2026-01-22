using System.Diagnostics.CodeAnalysis;

namespace GameKit.Content;

public abstract class VirtualFile
{
    public abstract string Path { get; }
    public abstract Stream Open();
}

public abstract class VirtualFileSystem: IDisposable
{
    public abstract ReadOnlySpan<VirtualFile> GetFiles(ReadOnlySpan<char> path);
    public abstract bool TryGetDirectories(ReadOnlySpan<char> path, out ReadOnlySpan<string> result);
    public abstract bool TryGetFile(ReadOnlySpan<char> path, [NotNullWhen(true)] out VirtualFile? file);

    public ReadOnlySpan<string> GetDirectories(ReadOnlySpan<char> path)
    {
        if (TryGetDirectories(path, out var directories))
        {
            return directories;
        }

        throw new DirectoryNotFoundException(path.ToString());
    }

    public VirtualFile GetFile(ReadOnlySpan<char> path)
    {
        if (TryGetFile(path, out VirtualFile? contentFile))
        {
            return contentFile;
        }

        throw new FileNotFoundException(path.ToString());
    }

    public Stream OpenStream(ReadOnlySpan<char> path)
    {
        return GetFile(path).Open();
    }

    // TODO: dispose pattern https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/dispose-pattern
    public virtual void Dispose()
    {
    }
}
