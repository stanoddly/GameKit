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

    public virtual long Length => _content.Length;
    public override Stream Open()
    {
        return new MemoryStream(_content);
    }
}

public class DictFileSystem : VirtualFileSystem
{
    private readonly FrozenDictionary<string, ImmutableArray<VirtualFile>> _files;
    private readonly FrozenDictionary<string, ImmutableArray<string>> _directories;
    private readonly FrozenDictionary<string, VirtualFile> _directFilesLookup;
    private readonly FrozenDictionary<string, ImmutableArray<VirtualFile>>.AlternateLookup<ReadOnlySpan<char>> _filesLookup;
    private readonly FrozenDictionary<string, ImmutableArray<string>>.AlternateLookup<ReadOnlySpan<char>> _directoriesLookup;
    private readonly FrozenDictionary<string, VirtualFile>.AlternateLookup<ReadOnlySpan<char>> _directFilesSpanLookup;

    public DictFileSystem(FrozenDictionary<string, ImmutableArray<VirtualFile>> files,
        FrozenDictionary<string, ImmutableArray<string>> directories)
    {
        _files = files;
        _directories = directories;
        _directFilesLookup = _files.Values.SelectMany(item => item).ToFrozenDictionary(item => item.Path);
        _filesLookup = _files.GetAlternateLookup<ReadOnlySpan<char>>();
        _directoriesLookup = _directories.GetAlternateLookup<ReadOnlySpan<char>>();
        _directFilesSpanLookup = _directFilesLookup.GetAlternateLookup<ReadOnlySpan<char>>();
    }

    public override ReadOnlySpan<VirtualFile> GetFiles(ReadOnlySpan<char> path)
    {
        if (!_filesLookup.TryGetValue(path, out var files))
        {
            throw new DirectoryNotFoundException(path.ToString());
        }

        return files.AsSpan();
    }

    public override bool TryGetDirectories(ReadOnlySpan<char> path, out ReadOnlySpan<string> result)
    {
        if (_directoriesLookup.TryGetValue(path, out var directories))
        {
            result = directories.AsSpan();
            return true;
        }

        result = Array.Empty<string>();
        return false;
    }

    public override bool TryGetFile(ReadOnlySpan<char> path, [NotNullWhen(true)] out VirtualFile? file)
    {
        return _directFilesSpanLookup.TryGetValue(path, out file);
    }

    public static readonly DictFileSystem Empty = new(
        FrozenDictionary<string, ImmutableArray<VirtualFile>>.Empty,
        FrozenDictionary<string, ImmutableArray<string>>.Empty);
}
