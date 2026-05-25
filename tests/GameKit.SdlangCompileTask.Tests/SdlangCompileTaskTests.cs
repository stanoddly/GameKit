namespace GameKit.SdlangCompileTask.Tests;

public class SdlangCompileTaskTests
{
    [Test]
    public void CompiledShaderOutputExists()
    {
        string outputDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestShaders", ".generated");

        // The shader file should be compiled by the build process
        // This test verifies that the compiled output file exists in the .generated/ subdirectory
        for (int i = 1; i < 3; i++)
        {
            string generatedShaderPath = Path.Combine(outputDir, $"test{i}.spv");
            string metadataPath = Path.Combine(outputDir, $"test{i}.metadata.json");

            Assert.That(File.Exists(generatedShaderPath), Is.True, "Compiled shader output file should exist at: " + generatedShaderPath);
            Assert.That(File.Exists(metadataPath), Is.True, "Compiled shader output file should exist at: " + metadataPath);
        }
    }

    [Test]
    public void Execute_WithNullInputFile_ReturnsTrue()
    {
        SdlangCompileTask task = new SdlangCompileTask { InputFile = null };
        Assert.That(task.Execute(), Is.True);
    }

    [Test]
    public void Execute_WithEmptyInputFile_ReturnsTrue()
    {
        SdlangCompileTask task = new SdlangCompileTask { InputFile = "" };
        Assert.That(task.Execute(), Is.True);
    }
}
