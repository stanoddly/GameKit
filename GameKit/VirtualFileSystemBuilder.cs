using System.Collections.Frozen;

namespace GameKit;

public class VirtualFileSystemBuilder
{
    private static readonly char[] Separators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];
    private readonly List<string> _directories = new();
    private readonly List<string> _zipFiles = new();
    
    public VirtualFileSystemBuilder AddContentFromZip(string filename)
    {
        throw new NotImplementedException();
        return this;
    }
    
    public VirtualFileSystemBuilder AddContentFromDirectory(string directory)
    {
        throw new NotImplementedException();
        return this;
    }

    public VirtualFileSystemBuilder AddContentFromProjectDirectory(string? subdirectory = null)
    {
        string currentDir = Directory.GetCurrentDirectory();
        string[] pathParts = currentDir.Split(Separators);

        if (pathParts.Length < 3 || !pathParts[^1].StartsWith("net") || pathParts[^3] != "bin")
        {
            return this;
        }
        
        if (subdirectory != null)
        {
            string[] parts = new string[pathParts.Length - 2];

            for (int i = 0; i < parts.Length - 3; i++)
            {
                parts[i] = pathParts[i];
            }
            parts[^1] = subdirectory;
            
            string directoryName = Path.Combine(parts);
            
            _directories.Add(directoryName);
        }
        else
        {
            string[] parts = new string[pathParts.Length - 3];

            for (int i = 0; i < parts.Length - 3; i++)
            {
                parts[i] = pathParts[i];
            }

            string directoryName = Path.Combine(parts);
            
            _directories.Add(directoryName);
        }
        

        return this;
    }
    
    
    
    static void ListAllItems(string path)
    {
        string root = Path.GetRelativePath(Directory.GetCurrentDirectory(), path);

        

        // Process all files directly under this folder
        foreach (string filename in Directory.GetFiles(path))
        {
            root = root.Replace(Path.DirectorySeparatorChar, '/');
        }

        // Recurse into subfolders of this folder
        foreach (string subDir in Directory.GetDirectories(path))
        {
            Console.WriteLine($"Directory: {subDir}");
            ListAllItems(subDir);
        }
    }
    
    VirtualFileSystem Create()
    {
        Dictionary<string, FileEntry> files = new();
        
        
    }
}