using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Pixely.Content;

public sealed class CachedFileSystem: VirtualFileSystem
{
    private readonly VirtualFileSystem _sourceVirtualFileSystem;
    private readonly DictFileSystem _dictFileSystem;

    private CachedFileSystem(VirtualFileSystem sourceVirtualFileSystem, DictFileSystem dictFileSystem)
    {
        _dictFileSystem = dictFileSystem;
        _sourceVirtualFileSystem = sourceVirtualFileSystem;
    }

    public override bool TryGetFiles(ReadOnlySpan<char> path, out ReadOnlySpan<VirtualFile> result)
    {
        return _dictFileSystem.TryGetFiles(path, out result);
    }

    public override bool TryGetDirectories(ReadOnlySpan<char> path, out ReadOnlySpan<string> result)
    {
        return _dictFileSystem.TryGetDirectories(path, out result);
    }

    public override bool TryGetFile(ReadOnlySpan<char> path, [NotNullWhen(true)] out VirtualFile? file)
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

        FrozenDictionary<string, ImmutableArray<string>> frozenDirectories =
            resultDirectories.ToFrozenDictionary(item => item.Item1, item => item.Item2);
        FrozenDictionary<string, ImmutableArray<VirtualFile>> frozenFiles =
            resultFiles.ToFrozenDictionary(item => item.Item1, item => item.Item2);

        return new CachedFileSystem(source, new DictFileSystem(frozenFiles, frozenDirectories));
    }
}
