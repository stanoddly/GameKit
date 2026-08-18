using Pixely.Content;

namespace Pixely.Tutorials.EmbeddedContent;

static class Program
{
    private const string ShaderPath = "shaders/nested/.generated/tutorial.vertex.spv";

    static int Main()
    {
        using VirtualFileSystem fileSystem = EmbeddedFileSystem.Create(typeof(Program).Assembly);
        using Stream shaderStream = fileSystem.OpenStream(ShaderPath);

        Console.WriteLine($"Loaded embedded shader '{ShaderPath}' ({shaderStream.Length} bytes).");
        return 0;
    }
}
