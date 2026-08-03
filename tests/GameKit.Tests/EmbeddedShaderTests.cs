using GameKit.Content;
using GameKit.Pencuil;

namespace GameKit.Tests;

public class EmbeddedShaderTests
{
    [TestCase("shaders/.generated/pencuil_vertex.metadata.json")]
    [TestCase("shaders/.generated/pencuil_vertex.spv")]
    [TestCase("shaders/.generated/pencuil_color_fragment.metadata.json")]
    [TestCase("shaders/.generated/pencuil_color_fragment.spv")]
    public void PencuilGeneratedShader_CanBeOpened(string path)
    {
        using VirtualFileSystem fileSystem = EmbeddedFileSystem.Create(typeof(PencuilExtensions).Assembly);

        using Stream stream = fileSystem.OpenStream(path);

        Assert.That(stream.Length, Is.GreaterThan(0));
    }
}
