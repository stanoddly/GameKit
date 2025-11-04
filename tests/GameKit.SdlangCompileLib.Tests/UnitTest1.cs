namespace GameKit.SdlangCompileLib.Tests;

public class SdlangCompilerTests
{
    private const string ShaderContent = """

                                          [shader("vertex")]
                                          float4 main(float3 position : POSITION) : SV_POSITION
                                          {
                                              return float4(position, 1.0);
                                          }
                                          """;

    [Test]
    public void CompileShader_CreatesMetadataFile()
    {
        // Arrange
        string testDir = Path.Combine(Path.GetTempPath(), "SdlangCompilerTests_" + Guid.NewGuid());
        Directory.CreateDirectory(testDir);

        try
        {
            string shaderPath = Path.Combine(testDir, "test_shader.slang");
            File.WriteAllText(shaderPath, ShaderContent);

            SdlangCompiler compiler = new SdlangCompiler();

            // Act
            compiler.Compile([shaderPath], onlySpirv: true, force: true);

            // Assert
            string metadataPath = Path.Combine(testDir, "test_shader.metadata.json");
            Assert.That(File.Exists(metadataPath), Is.True, "Metadata file should be created");

            string json = File.ReadAllText(metadataPath);
            ShaderMetadata? metadata = System.Text.Json.JsonSerializer.Deserialize<ShaderMetadata>(json);

            Assert.That(metadata, Is.Not.Null);
            Assert.That(metadata.Stage, Is.EqualTo("Vertex"));
            Assert.That(metadata.Shaders.Count, Is.GreaterThan(0));
            Assert.That(metadata.SourceHash, Is.Not.Empty);
        }
        finally
        {
            if (Directory.Exists(testDir))
                Directory.Delete(testDir, recursive: true);
        }
    }
}