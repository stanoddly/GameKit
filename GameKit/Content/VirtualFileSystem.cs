using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace GameKit.Content;

public abstract class ContentFile
{
    public abstract string Path { get; }
    public abstract long Length { get; }
    public abstract Stream Open();
}

public abstract class FileSystem: IDisposable
{
    public abstract ReadOnlySpan<ContentFile> GetFiles(string path);
    public abstract ReadOnlySpan<string> GetDirectories(string path);
    public abstract bool TryGetFile(string path, [NotNullWhen(true)] out ContentFile? stream);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ContentFile GetFile(string path)
    {
        if (TryGetFile(path, out ContentFile? contentFile))
        {
            return contentFile;
        }

        throw new FileNotFoundException();
    }

    public virtual void Dispose()
    {
    }
}

public sealed class NativeContentFile: ContentFile
{
    private string _filename;
    private string _nativeFilename;

    public NativeContentFile(string filename, string nativeFilename)
    {
        _filename = filename;
        _nativeFilename = nativeFilename;
    }

    public override Stream Open()
    {
        return File.OpenRead(_nativeFilename);
    }

    public override string Path => _filename;
    public override long Length => new FileInfo(_nativeFilename).Length;
}

public sealed class ZipContentFile: ContentFile
{
    private readonly ZipArchiveEntry _entry;

    public ZipContentFile(ZipArchiveEntry entry, string path)
    {
        _entry = entry;
        Path = path;
    }

    public override string Path { get; }
    public override long Length => _entry.Length;

    public override Stream Open()
    {
        return _entry.Open();
    }
}

public sealed class ZipFileSystem: FileSystem
{
    public override ReadOnlySpan<string> GetDirectories(string path)
    {
        throw new NotImplementedException();
    }

    public override bool TryGetFile(string path, [NotNullWhen(true)] out ContentFile? stream)
    {
        throw new NotImplementedException();
    }

    public override ReadOnlySpan<ContentFile> GetFiles(string path)
    {
        throw new NotImplementedException();
    }

    public override void Dispose()
    {
        throw new NotImplementedException();
    }
}

public sealed class VirtualFileSystem: FileSystem
{
    private readonly IReadOnlyDictionary<string, ImmutableArray<ContentFile>> _files;
    private readonly IReadOnlyDictionary<string, ImmutableArray<string>> _directories;
    private FrozenDictionary<string, ContentFile> _directFilesLookup;

    public VirtualFileSystem(IReadOnlyDictionary<string, ImmutableArray<ContentFile>> files, IReadOnlyDictionary<string, ImmutableArray<string>> directories)
    {
        _files = files;
        _directories = directories;
        
        _directFilesLookup = files
            .Select(pair => pair.Value)
            .SelectMany(item => item)
            .ToFrozenDictionary(item => item.Path, item=>item);
    }

    public override ReadOnlySpan<ContentFile> GetFiles(string path)
    {
        if (_files.TryGetValue(path, out ImmutableArray<ContentFile> files))
        {
            return files.AsSpan();
        }

        throw new DirectoryNotFoundException(path);
    }

    public override ReadOnlySpan<string> GetDirectories(string path)
    {
        if (_directories.TryGetValue(path, out ImmutableArray<string> directories))
        {
            return directories.AsSpan();
        }

        throw new DirectoryNotFoundException(path);
    }

    public override bool TryGetFile(string path, [NotNullWhen(true)] out ContentFile? stream)
    {
        return _directFilesLookup.TryGetValue(path, out stream);
    }
}
