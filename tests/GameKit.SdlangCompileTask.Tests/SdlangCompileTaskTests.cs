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
        // This test verifies that the compiled output file exists
        string compiledShaderPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.slang.out");

        Assert.That(File.Exists(compiledShaderPath),
            Is.True,
            "Compiled shader output file should exist at: " + compiledShaderPath);
    }
}
