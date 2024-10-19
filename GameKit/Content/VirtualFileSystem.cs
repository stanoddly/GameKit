using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace GameKit.Content;

public abstract class VirtualFile
{
    public abstract string Path { get; }
    public abstract long Length { get; }
    public abstract Stream Open();
}

public abstract class VirtualFileSystem: IDisposable
{
    public abstract ReadOnlySpan<VirtualFile> GetFiles(string path);
    public abstract ReadOnlySpan<string> GetDirectories(string path);
    public abstract bool TryGetFile(string path, [NotNullWhen(true)] out VirtualFile? file);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VirtualFile GetFile(string path)
    {
        if (TryGetFile(path, out VirtualFile? contentFile))
        {
            return contentFile;
        }

        throw new FileNotFoundException();
    }

    // TODO: dispose pattern https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/dispose-pattern
    public virtual void Dispose()
    {
    }
}
