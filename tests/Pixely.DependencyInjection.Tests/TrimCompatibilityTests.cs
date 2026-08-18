using System.Diagnostics;

namespace Pixely.DependencyInjection.Tests;

public sealed class TrimCompatibilityTests
{
    [Test]
    [NonParallelizable]
    public async Task DependencyInjection_BuildsWithoutTrimWarnings()
    {
        string repositoryDirectory = GetRepositoryDirectory();
        string projectPath = Path.Combine(
            repositoryDirectory,
            "src",
            "Pixely.DependencyInjection",
            "Pixely.DependencyInjection.csproj");

        ProcessStartInfo startInfo = new("dotnet")
        {
            WorkingDirectory = repositoryDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        startInfo.ArgumentList.Add("build");
        startInfo.ArgumentList.Add(projectPath);
        startInfo.ArgumentList.Add("-c");
        startInfo.ArgumentList.Add("Release");
        startInfo.ArgumentList.Add("-p:EnableTrimAnalyzer=true");
        startInfo.ArgumentList.Add("-p:TreatWarningsAsErrors=true");
        startInfo.ArgumentList.Add("--no-restore");
        startInfo.ArgumentList.Add("--no-incremental");
        startInfo.Environment["MSBUILDDISABLENODEREUSE"] = "1";

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start the dotnet process.");
        Task<string> standardOutputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> standardErrorTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        string standardOutput = await standardOutputTask;
        string standardError = await standardErrorTask;

        Assert.That(
            process.ExitCode,
            Is.Zero,
            $"Trim compatibility build failed.{Environment.NewLine}" +
            standardOutput +
            standardError);
    }

    private static string GetRepositoryDirectory()
    {
        DirectoryInfo? directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Pixely.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the Pixely repository.");
    }
}
