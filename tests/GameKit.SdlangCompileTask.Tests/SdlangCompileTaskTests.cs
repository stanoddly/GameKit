namespace GameKit.SdlangCompileTask.Tests;

public class SdlangCompileTaskTests
{
    private string _outputDir = null!;

    [SetUp]
    public void Setup()
    {
        _outputDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "compiled");
        if (Directory.Exists(_outputDir))
        {
            Directory.Delete(_outputDir, true);
        }
        Directory.CreateDirectory(_outputDir);
    }

    [TearDown]
    public void Cleanup()
    {
        if (Directory.Exists(_outputDir))
        {
            Directory.Delete(_outputDir, true);
        }
    }

    [Test]
    public void CompiledShaderOutputExists()
    {
        // The shader file should be compiled by the build process
        // This test verifies that the compiled output file exists in the compiled/ subdirectory
        for (int i = 1; i < 3; i++)
        {
            string compiledShaderPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestShaders", "compiled", $"test{i}.spv");
            string metadataPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestShaders", "compiled", $"test{i}.metadata.json");

            Assert.That(File.Exists(compiledShaderPath), Is.True, "Compiled shader output file should exist at: " + compiledShaderPath);
            Assert.That(File.Exists(metadataPath), Is.True, "Compiled shader output file should exist at: " + metadataPath);
        }
    }

    [Test]
    public void Execute_WithNullInputFile_ReturnsTrue()
    {
        var task = new SdlangCompileTask { InputFile = null };
        Assert.That(task.Execute(), Is.True);
    }

    [Test]
    public void Execute_WithEmptyInputFile_ReturnsTrue()
    {
        var task = new SdlangCompileTask { InputFile = "" };
        Assert.That(task.Execute(), Is.True);
    }
}
