using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;

namespace GameKit.Content;

/// <summary>
/// A virtual file implementation for files within a ZIP archive.
/// </summary>
internal class ZipFile : VirtualFile
{
    private readonly ZipArchiveEntry _entry;
    
    public ZipFile(ZipArchiveEntry entry)
    {
        _entry = entry;
    }
    
    public override string Path => _entry.FullName;
    
    public override Stream Open()
    {
        // Decompress the ZIP entry into a memory stream for full seeking capability
        var memoryStream = new MemoryStream((int)_entry.Length);
        using (var entryStream = _entry.Open())
        {
            entryStream.CopyTo(memoryStream);
        }
        
        // Reset position to beginning and return the seekable memory stream
        memoryStream.Position = 0;
        return memoryStream;
    }
}

/// <summary>
/// A virtual file system implementation for ZIP archives.
/// </summary>
public class ZipFileSystem : VirtualFileSystem
{
    private readonly ZipArchive _archive;
    private readonly Dictionary<string, List<ZipFile>> _filesByDirectory;
    private readonly Dictionary<string, List<string>> _directoriesByParent;
    private bool _disposed;
    
    private ZipFileSystem(ZipArchive archive)
    {
        _archive = archive;
        _filesByDirectory = new Dictionary<string, List<ZipFile>>();
        _directoriesByParent = new Dictionary<string, List<string>>();
        
        // Ensure root directory exists
        _directoriesByParent[""] = new List<string>();
        
        // Index all entries
        foreach (ZipArchiveEntry entry in _archive.Entries)
        {
            // Skip directory entries
            if (string.IsNullOrEmpty(entry.Name))
                continue;
            
            string normalizedPath = entry.FullName.Replace('\\', '/');
            string directory = GetDirectoryPath(normalizedPath);
            
            // Add file to its directory
            if (!_filesByDirectory.TryGetValue(directory, out var files))
            {
                files = new List<ZipFile>();
                _filesByDirectory[directory] = files;
            }
            
            files.Add(new ZipFile(entry));
            
            // Build directory hierarchy
            AddDirectoryToHierarchy(directory);
        }
    }
    
    private void AddDirectoryToHierarchy(string directory)
    {
        if (string.IsNullOrEmpty(directory))
            return;
            
        // Split path into components
        string[] parts = directory.Split('/');
        string currentPath = "";
        
        for (int i = 0; i < parts.Length; i++)
        {
            string parentPath = currentPath;
            
            // Build current path
            if (i > 0)
                currentPath += "/";
                
            currentPath += parts[i];
            
            // Add current directory to parent's children
            if (!_directoriesByParent.TryGetValue(parentPath, out var children))
            {
                children = new List<string>();
                _directoriesByParent[parentPath] = children;
            }
            
            if (!children.Contains(currentPath))
            {
                children.Add(currentPath);
            }
        }
    }
    
    /// <summary>
    /// Creates a new ZipVirtualFileSystem from the specified ZIP file path.
    /// </summary>
    /// <param name="zipPath">Path to the ZIP file</param>
    /// <returns>A new instance of ZipVirtualFileSystem</returns>
    public static ZipFileSystem Create(string zipPath)
    {
        if (string.IsNullOrEmpty(zipPath))
            throw new ArgumentException("Path cannot be null or empty", nameof(zipPath));
        
        if (!File.Exists(zipPath))
            throw new FileNotFoundException("ZIP file not found", zipPath);
        
        ZipArchive archive = System.IO.Compression.ZipFile.OpenRead(zipPath);
        return new ZipFileSystem(archive);
    }
    
    public override ReadOnlySpan<VirtualFile> GetFiles(ReadOnlySpan<char> path)
    {
        ThrowIfDisposed();

        string normalizedPath = NormalizePath(path);

        if (_filesByDirectory.TryGetValue(normalizedPath, out var files))
        {
            // Create an array of VirtualFile and return it as a span
            VirtualFile[] result = new VirtualFile[files.Count];
            for (int i = 0; i < files.Count; i++)
            {
                result[i] = files[i];
            }
            return result;
        }

        return Array.Empty<VirtualFile>();
    }

    public override bool TryGetDirectories(ReadOnlySpan<char> path, out ReadOnlySpan<string> result)
    {
        ThrowIfDisposed();

        string normalizedPath = NormalizePath(path);

        if (_directoriesByParent.TryGetValue(normalizedPath, out var directories))
        {
            // Extract just the directory names (not full paths)
            string[] foundDirectories = new string[directories.Count];

            for (int i = 0; i < directories.Count; i++)
            {
                string fullPath = directories[i];
                int lastSlash = fullPath.LastIndexOf('/');

                if (lastSlash >= 0)
                {
                    // Return just the last segment of the path
                    foundDirectories[i] = fullPath.Substring(lastSlash + 1);
                }
                else
                {
                    // No slashes, it's directly in the root
                    foundDirectories[i] = fullPath;
                }
            }

            result = foundDirectories;
            return true;
        }

        result = Array.Empty<string>();
        return false;
    }

    public override bool TryGetFile(ReadOnlySpan<char> path, [NotNullWhen(true)] out VirtualFile? file)
    {
        ThrowIfDisposed();

        string normalizedPath = NormalizePath(path);

        // Look for the file directly in the archive
        ZipArchiveEntry? entry = _archive.GetEntry(normalizedPath);
        if (entry != null && !string.IsNullOrEmpty(entry.Name))
        {
            file = new ZipFile(entry);
            return true;
        }

        file = null;
        return false;
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _archive.Dispose();
                _filesByDirectory.Clear();
                _directoriesByParent.Clear();
            }
            
            _disposed = true;
        }
    }
    
    public override void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ZipFileSystem));
        }
    }
    
    private static string NormalizePath(ReadOnlySpan<char> path)
    {
        if (path.IsEmpty)
            return string.Empty;

        // Replace backslashes with forward slashes and trim leading/trailing slashes
        return path.ToString().Replace('\\', '/').Trim('/');
    }
    
    private static string GetDirectoryPath(string path)
    {
        int lastSlashIndex = path.LastIndexOf('/');
        if (lastSlashIndex < 0)
            return string.Empty;
            
        return path.Substring(0, lastSlashIndex);
    }
}