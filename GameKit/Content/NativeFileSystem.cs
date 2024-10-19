using System.Diagnostics.CodeAnalysis;

namespace GameKit.Content;

public sealed class NativeFile: VirtualFile
{
    private readonly string _filename;
    private readonly string _nativeFilename;

    public NativeFile(string filename, string nativeFilename)
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

public sealed class NativeFileSystem: VirtualFileSystem
{
    public static readonly bool NativeDirSeparatorIsSlash = Path.DirectorySeparatorChar == '/'; 
    private readonly string _rootPath;

    public NativeFileSystem(string rootPath)
    {
        _rootPath = Path.GetFullPath(rootPath);
    }

    private string FromVirtualToNativePath(string path)
    {
        string relativePath = path;
        if (!NativeDirSeparatorIsSlash)
        {
            relativePath = path.Replace(Path.DirectorySeparatorChar, '/');
        }
        
        string almostReadAbsolutePath = Path.Combine(_rootPath, relativePath);
        // if there was a dot it may lead to something like: a/./b
        return Path.GetFullPath(almostReadAbsolutePath);

    }

    private string FromRelativeToVirtualPath(string path)
    {
        if (NativeDirSeparatorIsSlash)
        {
            return path;
        }

        return path.Replace(Path.DirectorySeparatorChar, '/');
    }
    
    private string FromAbsoluteToVirtualPath(string path)
    {
        string relativePath = Path.GetRelativePath(_rootPath, path);
        
        if (NativeDirSeparatorIsSlash)
        {
            return relativePath;
        }

        return relativePath.Replace(Path.DirectorySeparatorChar, '/');
    }

    public override ReadOnlySpan<VirtualFile> GetFiles(string path)
    {
        string nativePath = FromVirtualToNativePath(path);

        string[] filenames = Directory.GetFiles(nativePath);
        VirtualFile[] result = new VirtualFile[filenames.Length];

        for (int i = 0; i < filenames.Length; i++)
        {
            string relativeFilename = Path.GetRelativePath(_rootPath, filenames[i]);
            string virtualPath = FromRelativeToVirtualPath(relativeFilename);
            result[i] = new NativeFile(virtualPath, filenames[i]);
        }

        return result;
    }

    public override ReadOnlySpan<string> GetDirectories(string path)
    {
        string nativePath = FromVirtualToNativePath(path);

        string[] filenames = Directory.GetDirectories(nativePath);
        string[] result = new string[filenames.Length];

        for (int i = 0; i < filenames.Length; i++)
        {
            string relativeFilename = Path.GetRelativePath(_rootPath, filenames[i]);
            result[i] = FromRelativeToVirtualPath(relativeFilename);
        }

        return result;
    }

    public override bool TryGetFile(string path, [NotNullWhen(true)] out VirtualFile? file)
    {
        string nativePath = FromVirtualToNativePath(path);
        
        file = new NativeFile(path, nativePath);
        return true;
    }
}
