namespace GameKit.Content;

public class FileSystemBuilder
{
    private readonly List<VirtualFileSystem> _fileSystems = new();
    private bool _cached = false;

    public FileSystemBuilder AddContentFromDirectory(string directory)
    {
        AddSourceFileSystem(new NativeFileSystem(directory));
        return this;
    }

    public FileSystemBuilder AddSourceFileSystem(VirtualFileSystem virtualFileSystem)
    {
        _fileSystems.Add(virtualFileSystem);
        return this;
    }

    public FileSystemBuilder AddContentFromProjectDirectory(string? subdirectory = null)
    {
        string? projectDirectory = GetProjectDirectory();

        if (projectDirectory == null)
        {
            throw new InvalidOperationException(
                "Unable to determine project directory. Ensure you are running from the project directory or from 'bin/[configuration]/net*' directory.");
        }

        string contentDirectory = subdirectory != null
            ? Path.Combine(projectDirectory, subdirectory)
            : projectDirectory;

        AddContentFromDirectory(contentDirectory);

        return this;
    }

    private static string? GetProjectDirectory()
    {
        DirectoryInfo? dir = new(AppContext.BaseDirectory);

        while (dir != null)
        {
            if (dir.GetFiles("*.csproj").Length > 0)
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        return AppContext.BaseDirectory;
    }

    public FileSystemBuilder AddContentFromZip(string filename)
    {
        ZipFileSystem fileSystem = ZipFileSystem.Create(filename); 
        AddSourceFileSystem(fileSystem);
        
        return this;
    }

    public FileSystemBuilder WithCache()
    {
        _cached = true;
        return this;
    }

    public VirtualFileSystem Create()
    {
        VirtualFileSystem finalVirtualFileSystem;

        if (_fileSystems.Count == 0)
        {
            return DictFileSystem.Empty;
        }

        if (_fileSystems.Count == 1)
        {
            finalVirtualFileSystem = _fileSystems[0];
        }
        else
        {
            finalVirtualFileSystem = new CompositeFileSystem(_fileSystems);
        }

        if (_cached)
        {
            finalVirtualFileSystem = CachedFileSystem.Create(finalVirtualFileSystem);
        }

        return finalVirtualFileSystem;
    }
}
