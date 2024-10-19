using System.Diagnostics.CodeAnalysis;

namespace GameKit.Content;

public class CompositeFileSystem: VirtualFileSystem
{
    private readonly List<VirtualFileSystem> _fileSystems;

    public CompositeFileSystem(IEnumerable<VirtualFileSystem> fileSystems)
    {
        _fileSystems = fileSystems.ToList();
    }

    public override ReadOnlySpan<VirtualFile> GetFiles(string path)
    {
        Dictionary<string, VirtualFile> files = new();

        foreach (VirtualFileSystem fileSystem in _fileSystems)
        {
            ReadOnlySpan<VirtualFile> fileSystemFiles = fileSystem.GetFiles(path);

            foreach (VirtualFile fileSystemFile in fileSystemFiles)
            {
                files[fileSystemFile.Path] = fileSystemFile;
            }
        }
        
        return files.Values.ToArray();
    }

    public override ReadOnlySpan<string> GetDirectories(string path)
    {
        HashSet<string> finalDirectories = new();

        foreach (VirtualFileSystem fileSystem in _fileSystems)
        {
            ReadOnlySpan<string> directories = fileSystem.GetDirectories(path);

            foreach (string directory in directories)
            {
                finalDirectories.Add(directory);
            }
        }
        
        return finalDirectories.ToArray();
    }

    public override bool TryGetFile(string path, [NotNullWhen(true)] out VirtualFile? file)
    {
        for (int i = (_fileSystems.Count - 1); i < _fileSystems.Count; i--)
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
    
        foreach (var disposable in _fileSystems)
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