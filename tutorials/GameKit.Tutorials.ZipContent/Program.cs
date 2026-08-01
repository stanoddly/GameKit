using GameKit.Content;

namespace GameKit.Tutorials.ZipContent;

static class Program
{
    private const string ShaderPath = "shaders/.generated/tutorial_vertex.spv";

    static int Main()
    {
        using VirtualFileSystem fileSystem = new FileSystemBuilder()
            .AddContentFromZipPattern("Content.pk3")
            .AddContentFromDirectoryPattern("Content")
            .Create();
        using Stream shaderStream = fileSystem.OpenStream(ShaderPath);

        Console.WriteLine($"Loaded distributed shader '{ShaderPath}' ({shaderStream.Length} bytes).");
        return 0;
    }
}
