using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;

namespace GameKit;

public abstract class FileEntry
{
    public abstract Stream Open();
}

public sealed class RealFileEntry: FileEntry
{
    private string _filename;

    public RealFileEntry(string filename)
    {
        _filename = filename;
    }

    public override Stream Open()
    {
        return File.OpenRead(_filename);
    }
}

public sealed class ZipFileEntry
{
    private ZipArchiveEntry _entry;

    public ZipFileEntry(ZipArchiveEntry entry)
    {
        _entry = entry;
    }

    public Stream Open()
    {
        return _entry.Open();
    }
}

public interface IFileSystem
{
    ReadOnlySpan<string> GetFiles(string path);
    ReadOnlySpan<string> GetDirectories(string path);
    bool TryReadFile(string path, [NotNullWhen(true)] out Stream? stream);
    
    Stream ReadFile(string path)
    {
        if (TryReadFile(path, out Stream? stream))
        {
            return stream;
        }

        throw new FileNotFoundException();
    }
}

public sealed class ContentFileSystem: IFileSystem
{
    private string _rootDirectory;

    public ContentFileSystem(string rootDirectory)
    {
        _rootDirectory = rootDirectory;
    }

    public ReadOnlySpan<string> GetFiles(string path)
    {
        Directory.GetFiles()
    }

    public ReadOnlySpan<string> GetDirectories(string path)
    {
        throw new NotImplementedException();
    }

    public bool TryReadFile(string path, [NotNullWhen(true)] out Stream? stream)
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