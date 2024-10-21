using System.Collections;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace GameKit.Content;

public class DictFileSystem : VirtualFileSystem
{
    private readonly FrozenDictionary<string, VirtualFile[]> _files;
    private readonly FrozenDictionary<string, string[]> _directories;
    private readonly FrozenDictionary<string, VirtualFile> _directFilesLookup;

    public DictFileSystem(FrozenDictionary<string, VirtualFile[]> files, FrozenDictionary<string, string[]> directories)
    {
        _files = files;
        _directories = directories;
        _directFilesLookup = files.Values.SelectMany(item => item).ToFrozenDictionary(item => item.Path);
    }

    public DictFileSystem(Dictionary<string, VirtualFile[]> files, Dictionary<string, string[]> directories)
    {
        _files = files.ToFrozenDictionary();
        _directories = directories.ToFrozenDictionary();
        _directFilesLookup = files.Values.SelectMany(item => item).ToFrozenDictionary(item => item.Path);
    }

    public override ReadOnlySpan<VirtualFile> GetFiles(string path)
    {
        return _files[path];
    }

    public override ReadOnlySpan<string> GetDirectories(string path)
    {
        return _directories[path];
    }

    public override bool TryGetFile(string path, [NotNullWhen(true)] out VirtualFile? file)
    {
        return _directFilesLookup.TryGetValue(path, out file);
    }
}