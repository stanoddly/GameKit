using System.Diagnostics.CodeAnalysis;

namespace GameKit.Content;

public sealed class NativeFileSystem: FileSystem
{
    private string _rootPath;

    public NativeFileSystem(string rootPath)
    {
        _rootPath = rootPath;
    }

    private string ToNativePath(string path)
    {
        if (OperatingSystem.IsWindows())
        {
            return path.Replace(Path.DirectorySeparatorChar, '/');
        }
        return path;
    }

    private string ToVirtualPath(string path)
    {
        if (Path.DirectorySeparatorChar == '/')
        {
            return path;
        }

        return path.Replace(Path.DirectorySeparatorChar, '/');
    }

    public override ReadOnlySpan<string> GetFiles(string path)
    {
        string nativePath = ToNativePath(path);

        string[] filenames = Directory.GetFiles(nativePath);
        string[] result = new string[filenames.Length];

        for (int i = 0; i < filenames.Length; i++)
        {
            string relativeFilename = Path.GetRelativePath(_rootPath, filenames[i]);
            result[i] = ToVirtualPath(relativeFilename);
        }

        return result;
    }

    public override ReadOnlySpan<string> GetDirectories(string path)
    {
        string nativePath = ToNativePath(path);

        string[] filenames = Directory.GetDirectories(nativePath);
        string[] result = new string[filenames.Length];

        for (int i = 0; i < filenames.Length; i++)
        {
            string relativeFilename = Path.GetRelativePath(_rootPath, filenames[i]);
            result[i] = ToVirtualPath(relativeFilename);
        }

        return result;
    }

    public override  bool TryReadFile(string path, [NotNullWhen(true)] out Stream? stream)
    {
        throw new NotImplementedException();
    }
}