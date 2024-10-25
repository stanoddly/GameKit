using System.Collections;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace GameKit.Content;

public class ByteVirtualFile: VirtualFile
{
    private readonly byte[] _content;
    public override string Path { get; }

    public ByteVirtualFile(string path, byte[] content)
    {
        Path = path;
        _content = content;
    }
    
    public ByteVirtualFile(string path, ReadOnlySpan<byte> content)
    {
        Path = path;
        _content = content.ToArray();
    }
    
    public override long Length => _content.Length;
    public override Stream Open()
    {
        return new MemoryStream(_content);
    }
}

public class DictFileSystem : VirtualFileSystem
{
    private readonly IReadOnlyDictionary<string, ImmutableArray<VirtualFile>> _files;
    private readonly IReadOnlyDictionary<string, ImmutableArray<string>> _directories;
    private readonly FrozenDictionary<string, VirtualFile> _directFilesLookup;

    public DictFileSystem(IReadOnlyDictionary<string, ImmutableArray<VirtualFile>> files,
        IReadOnlyDictionary<string, ImmutableArray<string>> directories)
    {
        _files = files;
        _directories = directories;
        _directFilesLookup = files.Values.SelectMany(item => item).ToFrozenDictionary(item => item.Path);
    }

    public override ReadOnlySpan<VirtualFile> GetFiles(string path)
    {
        if (!_files.TryGetValue(path, out var files))
        {
            throw new DirectoryNotFoundException(path);
        }
        
        return files.AsSpan();
    }

    public override ReadOnlySpan<string> GetDirectories(string path)
    {
        if (!_directories.TryGetValue(path, out var directories))
        {
            throw new DirectoryNotFoundException(path);
        }
        return directories.AsSpan();
    }

    public override bool TryGetFile(string path, [NotNullWhen(true)] out VirtualFile? file)
    {
        return _directFilesLookup.TryGetValue(path, out file);
    }

    public static readonly DictFileSystem Empty = new(
        FrozenDictionary<string, ImmutableArray<VirtualFile>>.Empty,
        FrozenDictionary<string, ImmutableArray<string>>.Empty);
}
