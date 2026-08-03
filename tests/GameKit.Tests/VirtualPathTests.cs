using GameKit.Content;

namespace GameKit.Tests;

public class VirtualPathTests
{
    [TestCase("", "file.txt", "file.txt")]
    [TestCase("shaders", ".generated", "shaders/.generated")]
    [TestCase("shaders/", ".generated", "shaders/.generated")]
    [TestCase("shaders", "/.generated", "shaders/.generated")]
    [TestCase("shaders/", "/.generated", "shaders/.generated")]
    public void Combine_ReturnsForwardSlashSeparatedPath(string first, string second, string expected)
    {
        string result = VirtualPath.Combine(first, second);

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("shader", null)]
    [TestCase("shaders/shader", "shaders")]
    [TestCase("shaders/nested/shader", "shaders/nested")]
    public void GetDirectoryName_ReturnsForwardSlashDirectory(string path, string? expected)
    {
        string? result = VirtualPath.GetDirectoryName(path);

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("shader", "shader")]
    [TestCase("shaders/shader", "shader")]
    [TestCase("shaders/nested/shader", "shader")]
    public void GetFileName_ReturnsFinalPathSegment(string path, string expected)
    {
        string result = VirtualPath.GetFileName(path);

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("shader", ".generated/shader.metadata.json")]
    [TestCase("shaders/shader", "shaders/.generated/shader.metadata.json")]
    [TestCase("shaders/nested/shader", "shaders/nested/.generated/shader.metadata.json")]
    public void GeneratedShaderMetadataPath_UsesVirtualSeparators(string path, string expected)
    {
        string name = VirtualPath.GetFileName(path);
        string? directory = VirtualPath.GetDirectoryName(path);
        string generatedDirectory = directory == null
            ? ".generated"
            : VirtualPath.Combine(directory, ".generated");
        string metadataPath = VirtualPath.Combine(generatedDirectory, $"{name}.metadata.json");

        Assert.That(metadataPath, Is.EqualTo(expected));
    }
}
