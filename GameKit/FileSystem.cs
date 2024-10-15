using System.Collections.Immutable;
using System.IO.Compression;

namespace GameKit;

public enum EntryType: byte
{
    Directory,
    File
};

public abstract class Entry
{
    public EntryType Type { get; protected set; }
    public abstract Stream Open();
}

public sealed class RealFileEntry: Entry
{
    private string _filename;

    public RealFileEntry(string filename)
    {
        _filename = filename;
        Type = EntryType.File;
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

public sealed class DirectoryEntry: Entry
{
    public override Stream Open()
    {
        throw new InvalidOperationException();
    }
}

internal readonly record struct FileSystemEntry(string Name, FileSystemEntryType Type);

internal readonly record struct FileInfo;

internal readonly record struct DirectoryInfo(string[] Children);

public class FileSystem
{
    private static readonly string[] Empty = new string[0]; 
    private Dictionary<string, FileInfo> _files = new Dictionary<string, FileInfo>();
    private Dictionary<string, DirectoryInfo> _directories = new Dictionary<string, DirectoryInfo>();

    public ReadOnlySpan<string> GetFiles(string path)
    {
        if (_directories.TryGetValue(path, out DirectoryInfo directory))
        {
            return directory.Children;
        }

        return Empty;
    }

    public ReadOnlySpan<string> GetDirectories(string path)
    {
        throw new NotImplementedException();
    }
}