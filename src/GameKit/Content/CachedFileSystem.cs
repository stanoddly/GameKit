using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace GameKit.Content;

public sealed class CachedFileSystem: VirtualFileSystem
{
    private readonly VirtualFileSystem _sourceVirtualFileSystem;
    private readonly DictFileSystem _dictFileSystem;

    private CachedFileSystem(VirtualFileSystem sourceVirtualFileSystem, DictFileSystem dictFileSystem)
    {
        _dictFileSystem = dictFileSystem;
        _sourceVirtualFileSystem = sourceVirtualFileSystem;
    }

    public override ReadOnlySpan<VirtualFile> GetFiles(string path)
    {
        return _dictFileSystem.GetFiles(path);
    }

    public override ReadOnlySpan<string> GetDirectories(string path)
    {
        return _dictFileSystem.GetDirectories(path);
    }

    public override bool TryGetFile(string path, [NotNullWhen(true)] out VirtualFile? file)
    {
        return _dictFileSystem.TryGetFile(path, out file);
    }

    public override void Dispose()
    {
        _sourceVirtualFileSystem.Dispose();
    }

    public static VirtualFileSystem Create(VirtualFileSystem source)
    {
        Stack<string> analyzedDirectories = new();
        analyzedDirectories.Push(".");
        
        List<(string, ImmutableArray<string>)> resultDirectories = new();
        List<(string, ImmutableArray<VirtualFile>)> resultFiles = new();
         
        while (analyzedDirectories.TryPop(out string? directory))
        {
            ReadOnlySpan<string> sourceSubdirectories = source.GetDirectories(directory);
            resultDirectories.Add((directory, ImmutableArray.Create(sourceSubdirectories)));
            
            ReadOnlySpan<VirtualFile> sourceFiles = source.GetFiles(directory);
            
            resultFiles.Add((directory, ImmutableArray.Create(sourceFiles)));
            
            foreach (string sourceSubdirectory in sourceSubdirectories)
            {
                analyzedDirectories.Push(sourceSubdirectory);
            }
        }

        var frozenDirectories = resultDirectories.ToFrozenDictionary(item => item.Item1, item => item.Item2);
        var frozenFiles = resultFiles.ToFrozenDictionary(item => item.Item1, item => item.Item2);

        return new CachedFileSystem(source, new DictFileSystem(frozenFiles, frozenDirectories));
    }
}