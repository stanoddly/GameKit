using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace GameKit.SdlangCompileTask.Tests;

[NonParallelizable]
public class SdlangBuildIntegrationTests
{
    [Test]
    public async Task CleanBuildAndPublishRouteGeneratedShaders()
    {
        string repositoryDirectory = GetRepositoryDirectory();
        string projectDirectory = Path.Combine(
            repositoryDirectory,
            "tests",
            "GameKit.SdlangCompileTask.Tests",
            "BuildIntegration");
        string projectPath = Path.Combine(projectDirectory, "BuildIntegration.csproj");
        string generatedDirectory = Path.Combine(projectDirectory, "Content", "shaders", ".generated");
        string externalGeneratedDirectory = Path.Combine(
            projectDirectory,
            "..",
            "ExternalShaders",
            ".generated");
        string outputDirectory = Path.Combine(projectDirectory, "bin");
        string intermediateDirectory = Path.Combine(projectDirectory, "obj");

        DeleteDirectory(generatedDirectory);
        DeleteDirectory(externalGeneratedDirectory);
        DeleteDirectory(outputDirectory);
        DeleteDirectory(intermediateDirectory);

        await RunDotnetAsync(projectDirectory, "build", projectPath, "--nologo");

        string buildGeneratedDirectory = Path.Combine(
            outputDirectory,
            "Debug",
            "net10.0",
            "Content",
            "shaders",
            ".generated");
        AssertGeneratedShadersExist(buildGeneratedDirectory);
        AssertExternalGeneratedShadersExist(buildGeneratedDirectory);
        AssertExternalGeneratedShadersDoNotExist(Path.Combine(outputDirectory, "Debug", "net10.0"));
        AssertEmbeddedShadersAreNotCopied(buildGeneratedDirectory);
        AssertEmbeddedShadersExist(Path.Combine(outputDirectory, "Debug", "net10.0", "BuildIntegration.dll"));

        DeleteDirectory(generatedDirectory);
        DeleteDirectory(externalGeneratedDirectory);
        DeleteDirectory(outputDirectory);
        DeleteDirectory(intermediateDirectory);

        string publishDirectory = Path.Combine(outputDirectory, "publish");
        await RunDotnetAsync(
            projectDirectory,
            "publish",
            projectPath,
            "--nologo",
            "--output",
            publishDirectory);

        string publishGeneratedDirectory = Path.Combine(
            publishDirectory,
            "Content",
            "shaders",
            ".generated");
        AssertGeneratedShadersExist(publishGeneratedDirectory);
        AssertExternalGeneratedShadersExist(publishGeneratedDirectory);
        AssertExternalGeneratedShadersDoNotExist(publishDirectory);
        AssertEmbeddedShadersAreNotCopied(publishGeneratedDirectory);
        AssertEmbeddedShadersExist(Path.Combine(publishDirectory, "BuildIntegration.dll"));

        string noBuildPublishDirectory = Path.Combine(outputDirectory, "publish-no-build");
        await RunDotnetAsync(
            projectDirectory,
            "publish",
            projectPath,
            "--nologo",
            "--no-build",
            "--property:RejectUnexpectedShaderCompilation=true",
            "--output",
            noBuildPublishDirectory);

        string noBuildGeneratedDirectory = Path.Combine(
            noBuildPublishDirectory,
            "Content",
            "shaders",
            ".generated");
        AssertGeneratedShadersExist(noBuildGeneratedDirectory);
        AssertExternalGeneratedShadersExist(noBuildGeneratedDirectory);
        AssertExternalGeneratedShadersDoNotExist(noBuildPublishDirectory);
        AssertEmbeddedShadersAreNotCopied(noBuildGeneratedDirectory);
        AssertEmbeddedShadersExist(Path.Combine(noBuildPublishDirectory, "BuildIntegration.dll"));
    }

    private static void AssertGeneratedShadersExist(string generatedDirectory)
    {
        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(Path.Combine(generatedDirectory, "copy_output.spv")), Is.True);
            Assert.That(File.Exists(Path.Combine(generatedDirectory, "copy_output.metal")), Is.True);
            Assert.That(File.Exists(Path.Combine(generatedDirectory, "copy_output.metadata.json")), Is.True);
        });
    }

    private static void AssertEmbeddedShadersAreNotCopied(string generatedDirectory)
    {
        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(Path.Combine(generatedDirectory, "embedded_output.spv")), Is.False);
            Assert.That(File.Exists(Path.Combine(generatedDirectory, "embedded_output.metal")), Is.False);
            Assert.That(File.Exists(Path.Combine(generatedDirectory, "embedded_output.metadata.json")), Is.False);
        });
    }

    private static void AssertExternalGeneratedShadersExist(string generatedDirectory)
    {
        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(Path.Combine(generatedDirectory, "external_output.spv")), Is.True);
            Assert.That(File.Exists(Path.Combine(generatedDirectory, "external_output.metal")), Is.True);
            Assert.That(File.Exists(Path.Combine(generatedDirectory, "external_output.metadata.json")), Is.True);
        });
    }

    private static void AssertExternalGeneratedShadersDoNotExist(string outputDirectory)
    {
        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(Path.Combine(outputDirectory, "external_output.spv")), Is.False);
            Assert.That(File.Exists(Path.Combine(outputDirectory, "external_output.metal")), Is.False);
            Assert.That(File.Exists(Path.Combine(outputDirectory, "external_output.metadata.json")), Is.False);
        });
    }

    private static void AssertEmbeddedShadersExist(string assemblyPath)
    {
        using FileStream assemblyStream = File.OpenRead(assemblyPath);
        using PEReader peReader = new PEReader(assemblyStream);
        MetadataReader metadataReader = peReader.GetMetadataReader();
        string[] resourceNames = metadataReader.ManifestResources
            .Select(handle => metadataReader.GetString(metadataReader.GetManifestResource(handle).Name))
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(resourceNames, Contains.Item("shaders/.generated/embedded_output.spv"));
            Assert.That(resourceNames, Contains.Item("shaders/.generated/embedded_output.metal"));
            Assert.That(resourceNames, Contains.Item("shaders/.generated/embedded_output.metadata.json"));
        });
    }

    private static async Task RunDotnetAsync(string workingDirectory, params string[] arguments)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        startInfo.Environment["MSBUILDDISABLENODEREUSE"] = "1";

        using Process process = Process.Start(startInfo)!;
        Task<string> standardOutputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> standardErrorTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        string standardOutput = await standardOutputTask;
        string standardError = await standardErrorTask;

        Assert.That(
            process.ExitCode,
            Is.Zero,
            $"dotnet {string.Join(' ', arguments)} failed.{Environment.NewLine}" +
            standardOutput +
            standardError);
    }

    private static string GetRepositoryDirectory()
    {
        DirectoryInfo? directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "GameKit.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the GameKit repository.");
    }

    private static void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}
