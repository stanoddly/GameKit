using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection;

namespace GameKit.Content;

public sealed class EmbeddedFile : VirtualFile
{
    private readonly Assembly _assembly;
    public EmbeddedFile(Assembly assembly, string resourceName)
    {
        Path = resourceName;
        _assembly = assembly;
    }
    public override string Path { get; }
    
    public override Stream Open()
    {
        Stream? stream = _assembly.GetManifestResourceStream(Path);

        if (stream == null)
        {
            throw new Exception();
        }

        return stream;
    }
}

public static class EmbeddedFileSystem
{
    public static VirtualFileSystem Create(Assembly assembly)
    {
        Dictionary<string, List<string>> directoryToDirectoriesLookup = new();
        Dictionary<string, List<VirtualFile>> directoryToFilesLookup = new();
        
        foreach(var resourceName in assembly.GetManifestResourceNames())
        {
            string? directory = Path.GetDirectoryName(resourceName);
            
            string parentDirectory = directory ?? string.Empty;

            List<VirtualFile> files = directoryToFilesLookup.GetValueOrNew(parentDirectory);
            files.Add(new EmbeddedFile(assembly, resourceName));

            if (directory == null)
            {
                continue;
            }

            while (directory != null)
            {
                string previous = directory;
                directory = Path.GetDirectoryName(previous);
                parentDirectory = directory ?? string.Empty;
                
                List<string> directories = directoryToDirectoriesLookup.GetValueOrNew(parentDirectory);
                
                directories.Add(previous);
            }
        }
        
        var frozenDirectories = directoryToDirectoriesLookup.Select(pair => new KeyValuePair<string, ImmutableArray<string>>(pair.Key, pair.Value.ToImmutableArray())).ToFrozenDictionary();
        var frozenFiles = directoryToFilesLookup.Select(pair => new KeyValuePair<string, ImmutableArray<VirtualFile>>(pair.Key, pair.Value.ToImmutableArray())).ToFrozenDictionary();

        return new DictFileSystem(frozenFiles, frozenDirectories);
    }
}