namespace GameKit.Content;

public class FileSystemBuilder
{
    private enum CacheLevel
    {
        None, LazyCache, HardCache
    }
    
    private readonly List<VirtualFileSystem> _fileSystems = new();
    private CacheLevel _cacheLevel = CacheLevel.None;
    
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
        string currentDir = Directory.GetCurrentDirectory();
        string[] currentDirParts = currentDir.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

        if (currentDirParts.Length < 3 || !currentDirParts[^1].StartsWith("net") || currentDirParts[^3] != "bin")
        {
            // TODO: exception content
            throw new Exception();
        }

        string[] parts;

        if (subdirectory != null)
        {
            parts = new string[currentDirParts.Length - 2];

            for (int i = 0; i < currentDirParts.Length - 3; i++)
            {
                parts[i] = currentDirParts[i];
            }
            parts[^1] = subdirectory;
        }
        else
        {
            parts = new string[currentDirParts.Length - 3];

            for (int i = 0; i < currentDirParts.Length - 3; i++)
            {
                parts[i] = currentDirParts[i];
            }
        }
        
        string directoryName = Path.Combine(parts);

        if (NativeFileSystem.NativeDirSeparatorIsSlash)
        {
            directoryName = Path.DirectorySeparatorChar + directoryName;
        }
        
        AddContentFromDirectory(directoryName);

        return this;
    }

    public FileSystemBuilder WithCache(bool lazyCache = false)
    {
        _cacheLevel = lazyCache ? CacheLevel.LazyCache : CacheLevel.HardCache;
        return this;
    }

    public VirtualFileSystem Create()
    {
        VirtualFileSystem finalVirtualFileSystem;

        if (_fileSystems.Count == 0)
        {
            throw new Exception();
        }

        if (_fileSystems.Count == 1)
        {
            finalVirtualFileSystem = _fileSystems[0];
        }
        else
        {
            finalVirtualFileSystem = new CompositeFileSystem(_fileSystems);
        }

        if (_cacheLevel != CacheLevel.None)
        {
            finalVirtualFileSystem = new CachedFileSystem(finalVirtualFileSystem);
        }

        return finalVirtualFileSystem;
    }
}