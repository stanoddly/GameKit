using System.Diagnostics.CodeAnalysis;

namespace Pixely.Content;

public sealed class CompositeFileSystem: VirtualFileSystem
{
    private readonly List<VirtualFileSystem> _fileSystems;

    public CompositeFileSystem(IEnumerable<VirtualFileSystem> fileSystems)
    {
        _fileSystems = fileSystems.ToList();
    }

    public override bool TryGetFiles(ReadOnlySpan<char> path, out ReadOnlySpan<VirtualFile> result)
    {
        Dictionary<string, VirtualFile> files = new();
        bool foundFiles = false;

        foreach (VirtualFileSystem fileSystem in _fileSystems)
        {
            bool found = fileSystem.TryGetFiles(path, out ReadOnlySpan<VirtualFile> fileSystemFiles);

            if (!found)
            {
                continue;
            }

            foundFiles = true;

            foreach (VirtualFile fileSystemFile in fileSystemFiles)
            {
                files[fileSystemFile.Path] = fileSystemFile;
            }
        }

        if (!foundFiles)
        {
            result = Array.Empty<VirtualFile>();
            return false;
        }

        result = files.Values.ToArray();
        return true;
    }

    public override bool TryGetDirectories(ReadOnlySpan<char> path, out ReadOnlySpan<string> result)
    {
        HashSet<string>? finalDirectories = null;

        foreach (VirtualFileSystem fileSystem in _fileSystems)
        {
            bool found = fileSystem.TryGetDirectories(path, out ReadOnlySpan<string> directories);

            if (found)
            {
                finalDirectories ??= new();
                foreach (string directory in directories)
                {
                    finalDirectories.Add(directory);
                }
            }
        }

        if (finalDirectories == null)
        {
            result = Array.Empty<string>();
            return false;
        }

        result = finalDirectories.ToArray();
        return true;
    }

    public override bool TryGetFile(ReadOnlySpan<char> path, [NotNullWhen(true)] out VirtualFile? file)
    {
        for (int i = (_fileSystems.Count - 1); i >= 0; i--)
        {
            if (_fileSystems[i].TryGetFile(path, out file))
            {
                return true;
            }
        }

        file = null;
        return false;
    }

    public override void Dispose()
    {
        List<Exception> exceptions = new List<Exception>();
    
        foreach (VirtualFileSystem disposable in _fileSystems)
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }
    
        if (exceptions.Any())
        {
            throw new AggregateException("Failed to dispose one or more filesystems", exceptions);
        }
    }
}
