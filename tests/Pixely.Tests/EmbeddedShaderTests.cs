using Pixely.Content;
using Pixely.Pencuil;

namespace Pixely.Tests;

public class EmbeddedShaderTests
{
    [TestCase("shaders/.generated/pencuil_color.metadata.json")]
    [TestCase("shaders/.generated/pencuil_color.vertex.spv")]
    [TestCase("shaders/.generated/pencuil_color.fragment.spv")]
    [TestCase("shaders/.generated/pencuil_texture.metadata.json")]
    [TestCase("shaders/.generated/pencuil_texture.vertex.spv")]
    [TestCase("shaders/.generated/pencuil_texture.fragment.spv")]
    public void PencuilGeneratedShader_CanBeOpened(string path)
    {
        using VirtualFileSystem fileSystem = EmbeddedFileSystem.Create(typeof(PencuilExtensions).Assembly);

        using Stream stream = fileSystem.OpenStream(path);

        Assert.That(stream.Length, Is.GreaterThan(0));
    }
}
