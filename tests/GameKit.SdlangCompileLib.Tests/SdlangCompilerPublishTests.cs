using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GameKit.SdlangCompileLib.Tests;

public sealed class SdlangCompilerPublishTests
{
    private const string ShaderContent = """
                                         [shader("vertex")]
                                         float4 main(float3 position : POSITION) : SV_POSITION
                                         {
                                             return float4(position, 1.0);
                                         }
                                         """;

    [Test]
    [NonParallelizable]
    public async Task SingleFileCli_PublishesExternalSlangDistributionAndCompilesShader()
    {
        string repositoryDirectory = GetRepositoryDirectory();
        string temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            "SdlangCompilerPublishTests_" + Guid.NewGuid());
        string publishDirectory = Path.Combine(temporaryDirectory, "publish");
        string shaderPath = Path.Combine(temporaryDirectory, "test.slang");

        Directory.CreateDirectory(temporaryDirectory);

        try
        {
            string projectPath = Path.Combine(
                repositoryDirectory,
                "src",
                "GameKit.SdlangCompilerCli",
                "GameKit.SdlangCompilerCli.csproj");
            string runtimeIdentifier = GetRuntimeIdentifier();

            await RunProcessAsync(
                "dotnet",
                repositoryDirectory,
                "publish",
                projectPath,
                "-c",
                "Release",
                "-r",
                runtimeIdentifier,
                "--self-contained",
                "false",
                "-p:PublishSingleFile=true",
                "-p:TreatWarningsAsErrors=true",
                "-o",
                publishDirectory);

            string executableName = OperatingSystem.IsWindows()
                ? "GameKit.SdlangCompilerCli.exe"
                : "GameKit.SdlangCompilerCli";
            string slangCompilerName = OperatingSystem.IsWindows() ? "slangc.exe" : "slangc";
            string executablePath = Path.Combine(publishDirectory, executableName);
            string dxCompilerPath = OperatingSystem.IsWindows()
                ? Path.Combine(publishDirectory, "bin", "dxcompiler.dll")
                : Path.Combine(
                    publishDirectory,
                    "lib",
                    OperatingSystem.IsMacOS() ? "libdxcompiler.dylib" : "libdxcompiler.so");

            Assert.Multiple(() =>
            {
                Assert.That(File.Exists(executablePath), Is.True);
                Assert.That(File.Exists(Path.Combine(publishDirectory, "bin", slangCompilerName)), Is.True);
                Assert.That(File.Exists(dxCompilerPath), Is.True);
                Assert.That(Directory.Exists(Path.Combine(publishDirectory, "lib")), Is.True);
                Assert.That(
                    File.Exists(Path.Combine(publishDirectory, "GameKit.SdlangCompileLib.dll")),
                    Is.False);
            });

            File.WriteAllText(shaderPath, ShaderContent);

            await RunProcessAsync(
                executablePath,
                temporaryDirectory,
                shaderPath,
                "--force");

            string generatedDirectory = Path.Combine(temporaryDirectory, ".generated");
            Assert.Multiple(() =>
            {
                Assert.That(File.Exists(Path.Combine(generatedDirectory, "test.spv")), Is.True);
                Assert.That(File.Exists(Path.Combine(generatedDirectory, "test.metal")), Is.True);
                Assert.That(File.Exists(Path.Combine(generatedDirectory, "test.metadata.json")), Is.True);
            });
        }
        finally
        {
            if (Directory.Exists(temporaryDirectory))
            {
                Directory.Delete(temporaryDirectory, recursive: true);
            }
        }
    }

    private static string GetRuntimeIdentifier()
    {
        Architecture architecture = RuntimeInformation.OSArchitecture;

        if (OperatingSystem.IsLinux() && architecture == Architecture.X64)
        {
            return "linux-x64";
        }

        if (OperatingSystem.IsLinux() && architecture == Architecture.Arm64)
        {
            return "linux-arm64";
        }

        if (OperatingSystem.IsWindows() && architecture == Architecture.X64)
        {
            return "win-x64";
        }

        if (OperatingSystem.IsMacOS() && architecture == Architecture.X64)
        {
            return "osx-x64";
        }

        if (OperatingSystem.IsMacOS() && architecture == Architecture.Arm64)
        {
            return "osx-arm64";
        }

        throw new PlatformNotSupportedException(
            $"Single-file Slang publishing is not supported on {RuntimeInformation.OSDescription} {architecture}.");
    }

    private static async Task RunProcessAsync(
        string executablePath,
        string workingDirectory,
        params string[] arguments)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo(executablePath)
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

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Failed to start {executablePath}.");
        Task<string> standardOutputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> standardErrorTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        string standardOutput = await standardOutputTask;
        string standardError = await standardErrorTask;

        Assert.That(
            process.ExitCode,
            Is.Zero,
            $"{executablePath} {string.Join(' ', arguments)} failed.{Environment.NewLine}" +
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
}
