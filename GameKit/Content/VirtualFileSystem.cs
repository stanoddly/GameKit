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
    public abstract ReadOnlySpan<string> GetFiles(string path);
    public abstract ReadOnlySpan<string> GetDirectories(string path);
    public abstract bool TryReadFile(string path, [NotNullWhen(true)] out Stream? stream);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Stream ReadFile(string path)
    {
        if (TryReadFile(path, out Stream? stream))
        {
            return stream;
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

public sealed class ZipContentFile
{
    private ZipArchiveEntry _entry;

    public ZipContentFile(ZipArchiveEntry entry)
    {
        _entry = entry;
    }

    public Stream Open()
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

    public override ReadOnlySpan<string> GetFiles(string path)
    {
        throw new NotImplementedException();
    }

    public override bool TryReadFile(string path, [NotNullWhen(true)] out Stream? stream)
    {
        throw new NotImplementedException();
    }

    public override void Dispose()
    {
        throw new NotImplementedException();
    }
}

public readonly record struct DirectoryInfo(ImmutableArray<string> SubDirectories, ImmutableArray<string> Files);

public sealed class VirtualFileSystem
{
    private IReadOnlyDictionary<string, FileEntry> _files;
    private IReadOnlyDictionary<string, DirectoryInfo> _directories;

    public VirtualFileSystem(IReadOnlyDictionary<string, FileEntry> files, IReadOnlyDictionary<string, DirectoryInfo> directories)
    {
        _files = files;
        _directories = directories;
    }

    public ImmutableArray<string> GetFiles(string path)
    {
        if (_directories.TryGetValue(path, out DirectoryInfo directory))
        {
            return directory.Files;
        }

        throw new DirectoryNotFoundException(path);
    }

    public ImmutableArray<string> GetDirectories(string path)
    {
        if (_directories.TryGetValue(path, out DirectoryInfo directory))
        {
            return directory.Files;
        }

        throw new DirectoryNotFoundException(path);
    }

    public Stream ReadFile(string path)
    {
        if (_files.TryGetValue(path, out FileEntry? fileEntry))
        {
            return fileEntry.Open();
        }
        throw new FileNotFoundException(path);
    }
    
    public bool TryReadFile(string path, out Stream? stream)
    {
        
        if (!_files.TryGetValue(path, out FileEntry? fileEntry))
        {
            stream = null;
            return false;
        }
        
        stream = fileEntry.Open();
        return true;
    }
}