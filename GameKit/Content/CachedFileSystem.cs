using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace GameKit.Content;

public class CachedFileSystem: VirtualFileSystem
{
    private readonly Dictionary<string, VirtualFile[]> _files = new();
    private readonly Dictionary<string, string[]> _directories = new();
    private readonly Dictionary<string, VirtualFile> _directFilesLookup = new();
    private readonly VirtualFileSystem _sourceVirtualFileSystem;

    public CachedFileSystem(VirtualFileSystem sourceVirtualFileSystem)
    {
        _sourceVirtualFileSystem = sourceVirtualFileSystem;
    }

    public override ReadOnlySpan<VirtualFile> GetFiles(string path)
    {
        ref VirtualFile[] cachedFiles = ref CollectionsMarshal.GetValueRefOrAddDefault(_files, path, out bool cachedFileExists)!;

        if (cachedFileExists)
        {
            return cachedFiles;
        }

        ReadOnlySpan<VirtualFile> sourceFiles = _sourceVirtualFileSystem.GetFiles(path);
        
        cachedFiles = sourceFiles.ToArray();
        foreach (VirtualFile contentFile in cachedFiles)
        {
            // TryAdd so we don't overwrite existing cache
            _directFilesLookup.TryAdd(contentFile.Path, contentFile);
        }

        return cachedFiles;
    }

    public override ReadOnlySpan<string> GetDirectories(string path)
    {
        ref string[] cachedSubdirectories = ref CollectionsMarshal.GetValueRefOrAddDefault(_directories, path, out bool cachedDirsExist)!;

        if (cachedDirsExist)
        {
            return cachedSubdirectories;
        }

        ReadOnlySpan<string> sourceSubdirectories = _sourceVirtualFileSystem.GetDirectories(path);
        
        cachedSubdirectories = sourceSubdirectories.ToArray();

        return cachedSubdirectories;
    }

    public override bool TryGetFile(string path, [NotNullWhen(true)] out VirtualFile? file)
    {
        ref VirtualFile cachedFile = ref CollectionsMarshal.GetValueRefOrAddDefault(_directFilesLookup, path, out bool cachedFileExists)!;

        if (cachedFileExists)
        {
            file = cachedFile;
            return true;
        }

        bool sourceFileExists = _sourceVirtualFileSystem.TryGetFile(path, out file);

        if (sourceFileExists && file != null)
        {
            cachedFile = file;
        }
        
        return sourceFileExists;
    }

    public override void Dispose()
    {
        _sourceVirtualFileSystem.Dispose();
    }
}