using System.Diagnostics;
using System.IO.Compression;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

namespace Pixely.Package.Tests;

[Category("PackageIntegration")]
[Explicit("Builds and consumes a local NuGet package and must execute in isolation.")]
[NonParallelizable]
public class PackageIntegrationTests
{
    private const long NuGetPackageSizeLimitBytes = 250_000_000;

    private static readonly string[] RuntimeAssemblies =
    [
        "Pixely.AStar",
        "Pixely.Architecture.Testing",
        "Pixely.Architecture",
        "Pixely.Audio",
        "Pixely.Collections",
        "Pixely.Componentize",
        "Pixely.Core",
        "Pixely.DependencyInjection",
        "Pixely.Events",
        "Pixely.Logging",
        "Pixely.Pencuil",
        "Pixely.ShaderCommon",
        "Pixely.Utils",
        "Pixely"
    ];

    private string _repositoryDirectory = null!;
    private string _testArtifactsDirectory = null!;
    private string _packageDirectory = null!;
    private string _packagesDirectory = null!;
    private string _packageVersion = null!;
    private string _packagePath = null!;
    private string _symbolPackagePath = null!;
    private XDocument _slangBundleConfiguration = null!;

    [OneTimeSetUp]
    public async Task CreatePackage()
    {
        _repositoryDirectory = GetRepositoryDirectory();
        _slangBundleConfiguration = XDocument.Load(Path.Combine(
            _repositoryDirectory,
            "src",
            "Pixely.SdlangCompiler",
            "build",
            "Pixely.SlangBundle.props"));
        _testArtifactsDirectory = Path.Combine(_repositoryDirectory, "artifacts", "package-tests");
        string? suppliedPackageDirectory = Environment.GetEnvironmentVariable("PIXELY_PACKAGE_DIRECTORY");
        _packageDirectory = suppliedPackageDirectory
            ?? Path.Combine(_testArtifactsDirectory, "feed");
        _packagesDirectory = Path.Combine(_testArtifactsDirectory, "restore");
        _packageVersion = Environment.GetEnvironmentVariable("PIXELY_PACKAGE_VERSION")
            ?? "0.0.0-alpha.package-tests";
        _packagePath = Path.Combine(_packageDirectory, $"Pixely.{_packageVersion}.nupkg");
        _symbolPackagePath = Path.Combine(_packageDirectory, $"Pixely.{_packageVersion}.snupkg");

        DeleteDirectory(_testArtifactsDirectory);
        Directory.CreateDirectory(_packagesDirectory);

        if (suppliedPackageDirectory is not null)
        {
            Assert.That(Directory.Exists(_packageDirectory), Is.True);
            Assert.That(File.Exists(_packagePath), Is.True);
            Assert.That(File.Exists(_symbolPackagePath), Is.True);
            return;
        }

        Directory.CreateDirectory(_packageDirectory);

        string projectPath = Path.Combine(
            _repositoryDirectory,
            "packaging",
            "Pixely",
            "Pixely.Package.csproj");
        await RunDotnetAsync(
            _repositoryDirectory,
            "pack",
            projectPath,
            "--configuration",
            "Release",
            "--output",
            _packageDirectory,
            $"--property:PackageVersion={_packageVersion}",
            $"--property:Version={_packageVersion}",
            "--nologo");
    }

    [OneTimeTearDown]
    public void CleanPackageConsumers()
    {
        DeleteConsumerOutputs("ShaderConsumer");
        DeleteConsumerOutputs("ShaderFreeConsumer");
        DeleteDirectory(_testArtifactsDirectory);
    }

    [Test]
    public void PackageArchiveContainsAllCoordinatedAssetsAndMetadata()
    {
        Assert.That(File.Exists(_packagePath), Is.True);
        Assert.That(File.Exists(_symbolPackagePath), Is.True);
        Assert.That(new FileInfo(_packagePath).Length, Is.LessThan(NuGetPackageSizeLimitBytes));

        using ZipArchive package = ZipFile.OpenRead(_packagePath);
        HashSet<string> entries = package.Entries.Select(entry => entry.FullName).ToHashSet(StringComparer.Ordinal);

        foreach (string assembly in RuntimeAssemblies)
        {
            string entryName = $"lib/net10.0/{assembly}.dll";
            Assert.That(entries, Does.Contain(entryName));
            ZipArchiveEntry assemblyEntry = package.GetEntry(entryName)
                ?? throw new InvalidOperationException($"{entryName} is missing from the package.");
            using Stream assemblyStream = assemblyEntry.Open();
            using MemoryStream assemblyBytes = new();
            assemblyStream.CopyTo(assemblyBytes);
            assemblyBytes.Position = 0;
            using PEReader peReader = new(assemblyBytes);
            Version assemblyVersion = peReader.GetMetadataReader().GetAssemblyDefinition().Version;
            string[] versionComponents = _packageVersion.Split('-')[0].Split('.');
            Version expectedAssemblyVersion = new(
                int.Parse(versionComponents[0]),
                int.Parse(versionComponents[1]),
                int.Parse(versionComponents[2]),
                0);
            Assert.That(assemblyVersion, Is.EqualTo(expectedAssemblyVersion));
        }

        Assert.Multiple(() =>
        {
            Assert.That(entries, Does.Contain("analyzers/dotnet/cs/Pixely.DependencyInjection.Generator.dll"));
            Assert.That(entries, Does.Contain("buildTransitive/Pixely.props"));
            Assert.That(entries, Does.Contain("buildTransitive/Pixely.targets"));
            Assert.That(entries, Does.Contain("tools/net10.0/any/Pixely.SdlangCompiler.dll"));
            Assert.That(entries, Does.Contain("tools/net10.0/any/Pixely.ShaderCommon.dll"));
            Assert.That(entries, Does.Contain("tools/net10.0/any/build/Pixely.SdlangCompiler.props"));
            Assert.That(entries, Does.Contain("tools/net10.0/any/build/Pixely.SdlangCompiler.targets"));
            Assert.That(entries, Does.Contain("tools/net10.0/any/build/Pixely.SlangBundle.props"));
            Assert.That(entries, Does.Contain("THIRD-PARTY-NOTICES.md"));
            Assert.That(entries, Does.Not.Contain("lib/net10.0/Pixely.SdlangCompiler.dll"));
            Assert.That(entries, Does.Not.Contain("lib/net10.0/Pixely.DependencyInjection.Generator.dll"));
            Assert.That(entries.Any(entry =>
                entry.StartsWith("tools/slang/", StringComparison.Ordinal) &&
                entry.EndsWith(".zip", StringComparison.Ordinal)), Is.False);
            Assert.That(entries.Any(IsNuGetEmptyFolderPlaceholder), Is.False);
        });

        foreach (string platform in GetSlangPlatforms())
        {
            AssertSlangPlatformDirectory(package, platform);
        }

        string shaderProps = ReadPackageEntry(
            package,
            "tools/net10.0/any/build/Pixely.SdlangCompiler.props");
        string shaderTargets = ReadPackageEntry(
            package,
            "tools/net10.0/any/build/Pixely.SdlangCompiler.targets");
        string slangBundleProps = ReadPackageEntry(
            package,
            "tools/net10.0/any/build/Pixely.SlangBundle.props");
        string pixelyProps = ReadPackageEntry(package, "buildTransitive/Pixely.props");
        Assert.Multiple(() =>
        {
            Assert.That(shaderProps, Does.Not.Contain("SlangDownloadUrl"));
            Assert.That(shaderProps, Does.Not.Contain("SlangZipSha256"));
            Assert.That(shaderTargets, Does.Not.Contain("DownloadFile"));
            Assert.That(shaderTargets, Does.Not.Contain("DownloadSlang"));
            Assert.That(shaderTargets, Does.Not.Contain("Unzip"));
            Assert.That(shaderTargets, Does.Not.Contain("<Copy "));
            Assert.That(shaderTargets, Does.Not.Contain("chmod"));
            Assert.That(shaderProps, Does.Contain("$(SlangToolSourceDirectory)</SlangBaseDir>"));
            Assert.That(slangBundleProps, Does.Not.Contain("ExpectedSha256"));
            Assert.That(slangBundleProps, Does.Not.Contain("Sha256"));
            Assert.That(pixelyProps, Does.Contain("tools\\slang"));
            Assert.That(pixelyProps, Does.Not.Contain("SlangObjDir"));
            Assert.That(pixelyProps, Does.Not.Contain("_OwnsSlangInstallation"));
            Assert.That(pixelyProps, Does.Not.Contain("_DownloadSlangOnlyWhenShaders"));
        });

        ZipArchiveEntry nuspecEntry = package.GetEntry("Pixely.nuspec")
            ?? throw new InvalidOperationException("Pixely.nuspec is missing from the package.");
        using Stream nuspecStream = nuspecEntry.Open();
        using StreamReader nuspecReader = new(nuspecStream);
        string nuspecContents = nuspecReader.ReadToEnd();
        XDocument nuspec = XDocument.Parse(nuspecContents);
        XNamespace ns = nuspec.Root?.Name.Namespace
            ?? throw new InvalidOperationException("Pixely.nuspec has no root element.");
        XElement metadata = nuspec.Root?.Element(ns + "metadata")
            ?? throw new InvalidOperationException("Pixely.nuspec has no metadata element.");
        XElement repository = metadata.Element(ns + "repository")
            ?? throw new InvalidOperationException("Pixely.nuspec has no repository element.");
        string[] expectedDependencies = GetRuntimePackageDependencies();
        string[] dependencies = metadata
            .Descendants(ns + "dependency")
            .Select(dependency => $"{(string?)dependency.Attribute("id")}:{(string?)dependency.Attribute("version")}")
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That((string?)metadata.Element(ns + "id"), Is.EqualTo("Pixely"));
            Assert.That((string?)metadata.Element(ns + "version"), Is.EqualTo(_packageVersion));
            Assert.That((string?)metadata.Element(ns + "authors"), Is.EqualTo("stanoddly"));
            Assert.That((string?)metadata.Element(ns + "readme"), Is.EqualTo("README.md"));
            Assert.That((string?)metadata.Element(ns + "description"), Does.Contain("experimental"));
            Assert.That((string?)metadata.Element(ns + "license"), Is.EqualTo("MIT"));
            Assert.That((string?)repository.Attribute("url"), Is.EqualTo("https://github.com/stanoddly/Pixely"));
            Assert.That((string?)repository.Attribute("commit"), Is.Not.Empty);
            Assert.That(dependencies, Is.EqualTo(expectedDependencies.Order(StringComparer.Ordinal)));
            Assert.That(dependencies.Any(dependency => dependency.StartsWith("Pixely", StringComparison.Ordinal)), Is.False);
            Assert.That(nuspecContents, Does.Not.Contain("Package Description"));
            Assert.That(nuspecContents, Does.Not.Contain("_._"));
        });

        using ZipArchive symbols = ZipFile.OpenRead(_symbolPackagePath);
        HashSet<string> symbolEntries = symbols.Entries
            .Select(entry => entry.FullName)
            .ToHashSet(StringComparer.Ordinal);
        foreach (string assembly in RuntimeAssemblies)
        {
            Assert.That(symbolEntries, Does.Contain($"lib/net10.0/{assembly}.pdb"));
        }

        ZipArchiveEntry sourceLinkEntry = symbols.GetEntry("lib/net10.0/Pixely.pdb")
            ?? throw new InvalidOperationException("Pixely.pdb is missing from the symbol package.");
        using Stream sourceLinkStream = sourceLinkEntry.Open();
        using MemoryStream sourceLinkBytes = new();
        sourceLinkStream.CopyTo(sourceLinkBytes);
        string sourceLinkContents = Encoding.UTF8.GetString(sourceLinkBytes.ToArray());
        Assert.That(
            sourceLinkContents,
            Does.Contain("https://raw.githubusercontent.com/stanoddly/Pixely/"));
    }

    [Test]
    public async Task ConsumerUsesRuntimeAssembliesGeneratorAndShaderBuildAssets()
    {
        string consumerDirectory = GetConsumerDirectory("ShaderConsumer");
        DeleteConsumerOutputs("ShaderConsumer");

        await BuildConsumerAsync(consumerDirectory);
        string outputDirectory = Path.Combine(consumerDirectory, "bin", "Release", "net10.0");
        string generatedDirectory = Path.Combine(consumerDirectory, "Content", "shaders", ".generated");
        string shaderToolDirectory = Path.Combine(consumerDirectory, "obj", "Pixely.SdlangCompiler");
        string slangDirectory = GetRestoredSlangDirectory(GetCurrentSlangPlatform());

        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(Path.Combine(generatedDirectory, "package.vertex.spv")), Is.True);
            Assert.That(File.Exists(Path.Combine(generatedDirectory, "package.vertex.dxil")), Is.True);
            Assert.That(File.Exists(Path.Combine(generatedDirectory, "package.vertex.metal")), Is.True);
            Assert.That(File.Exists(Path.Combine(generatedDirectory, "package.fragment.spv")), Is.True);
            Assert.That(File.Exists(Path.Combine(generatedDirectory, "package.fragment.dxil")), Is.True);
            Assert.That(File.Exists(Path.Combine(generatedDirectory, "package.fragment.metal")), Is.True);
            Assert.That(File.Exists(Path.Combine(generatedDirectory, "package.metadata.json")), Is.True);
            Assert.That(File.Exists(Path.Combine(
                slangDirectory,
                "bin",
                OperatingSystem.IsWindows() ? "slangc.exe" : "slangc")), Is.True);
            Assert.That(Directory.Exists(shaderToolDirectory), Is.False);
            Assert.That(File.Exists(Path.Combine(outputDirectory, "Pixely.SdlangCompiler.dll")), Is.False);
            Assert.That(File.Exists(Path.Combine(outputDirectory, "Microsoft.Build.Framework.dll")), Is.False);
            Assert.That(File.Exists(Path.Combine(outputDirectory, "Microsoft.Build.Utilities.Core.dll")), Is.False);
            Assert.That(File.Exists(Path.Combine(outputDirectory, "Microsoft.NET.StringTools.dll")), Is.False);
            Assert.That(File.Exists(Path.Combine(outputDirectory, "System.Configuration.ConfigurationManager.dll")), Is.False);
            Assert.That(File.Exists(Path.Combine(outputDirectory, "System.Diagnostics.EventLog.dll")), Is.False);
            Assert.That(File.Exists(Path.Combine(outputDirectory, "System.Security.Cryptography.ProtectedData.dll")), Is.False);
        });

        string output = await RunDotnetAsync(
            consumerDirectory,
            Path.Combine(outputDirectory, "ShaderConsumer.dll"));
        Assert.That(output, Does.Contain("Package consumer succeeded."));
    }

    [Test]
    public async Task ShaderFreeConsumerDoesNotInitializeShaderTooling()
    {
        string consumerDirectory = GetConsumerDirectory("ShaderFreeConsumer");
        DeleteConsumerOutputs("ShaderFreeConsumer");

        await BuildConsumerAsync(consumerDirectory);
        string outputDirectory = Path.Combine(consumerDirectory, "bin", "Release", "net10.0");
        string shaderToolDirectory = Path.Combine(consumerDirectory, "obj", "Pixely.SdlangCompiler");

        Assert.Multiple(() =>
        {
            Assert.That(Directory.Exists(shaderToolDirectory), Is.False);
            Assert.That(File.Exists(Path.Combine(outputDirectory, "Pixely.SdlangCompiler.dll")), Is.False);
            Assert.That(File.Exists(Path.Combine(outputDirectory, "Microsoft.Build.Framework.dll")), Is.False);
            Assert.That(File.Exists(Path.Combine(outputDirectory, "Microsoft.Build.Utilities.Core.dll")), Is.False);
            Assert.That(File.Exists(Path.Combine(outputDirectory, "Microsoft.NET.StringTools.dll")), Is.False);
            Assert.That(File.Exists(Path.Combine(outputDirectory, "System.Configuration.ConfigurationManager.dll")), Is.False);
            Assert.That(File.Exists(Path.Combine(outputDirectory, "System.Diagnostics.EventLog.dll")), Is.False);
            Assert.That(File.Exists(Path.Combine(outputDirectory, "System.Security.Cryptography.ProtectedData.dll")), Is.False);
        });

        string output = await RunDotnetAsync(
            consumerDirectory,
            Path.Combine(outputDirectory, "ShaderFreeConsumer.dll"));
        Assert.That(output, Does.Contain("Package consumer succeeded."));
    }

    [Test]
    public async Task ShaderCompilerSelectionUsesBuildHostInsteadOfTargetRuntime()
    {
        string consumerDirectory = GetConsumerDirectory("ShaderConsumer");
        DeleteConsumerOutputs("ShaderConsumer");
        string targetRuntime = OperatingSystem.IsWindows() ? "linux-x64" : "win-x64";

        await BuildConsumerAsync(consumerDirectory, targetRuntime);

        string shaderToolDirectory = Path.Combine(consumerDirectory, "obj", "Pixely.SdlangCompiler");
        string generatedDirectory = Path.Combine(consumerDirectory, "Content", "shaders", ".generated");
        string hostDirectory = GetRestoredSlangDirectory(GetCurrentSlangPlatform());
        Assert.Multiple(() =>
        {
            Assert.That(Directory.Exists(hostDirectory), Is.True);
            Assert.That(Directory.Exists(shaderToolDirectory), Is.False);
            Assert.That(File.Exists(Path.Combine(generatedDirectory, "package.vertex.spv")), Is.True);
            Assert.That(File.Exists(Path.Combine(generatedDirectory, "package.vertex.dxil")), Is.True);
            Assert.That(File.Exists(Path.Combine(generatedDirectory, "package.vertex.metal")), Is.True);
            Assert.That(File.Exists(Path.Combine(generatedDirectory, "package.fragment.spv")), Is.True);
            Assert.That(File.Exists(Path.Combine(generatedDirectory, "package.fragment.dxil")), Is.True);
            Assert.That(File.Exists(Path.Combine(generatedDirectory, "package.fragment.metal")), Is.True);
            Assert.That(File.Exists(Path.Combine(generatedDirectory, "package.metadata.json")), Is.True);
        });
    }

    private async Task BuildConsumerAsync(string consumerDirectory, string? runtimeIdentifier = null)
    {
        string[] projectPaths = Directory.GetFiles(consumerDirectory, "*.csproj");
        Assert.That(projectPaths, Has.Length.EqualTo(1), $"Expected one consumer project in {consumerDirectory}.");
        string projectPath = projectPaths[0];
        string projectContents = File.ReadAllText(projectPath);
        Assert.Multiple(() =>
        {
            Assert.That(projectContents, Does.Not.Contain("ProjectReference"));
            Assert.That(projectContents, Does.Not.Contain("src\\").And.Not.Contain("src/"));
        });
        List<string> restoreArguments =
        [
            "restore",
            projectPath,
            "--source",
            _packageDirectory,
            "--source",
            "https://api.nuget.org/v3/index.json",
            $"--property:PixelyPackageVersion={_packageVersion}",
            $"--property:RestorePackagesPath={_packagesDirectory}",
            "--nologo"
        ];
        List<string> buildArguments =
        [
            "build",
            projectPath,
            "--configuration",
            "Release",
            "--no-restore",
            $"--property:PixelyPackageVersion={_packageVersion}",
            $"--property:RestorePackagesPath={_packagesDirectory}",
            "--nologo"
        ];
        if (runtimeIdentifier is not null)
        {
            restoreArguments.Add($"--property:RuntimeIdentifier={runtimeIdentifier}");
            restoreArguments.Add("--property:UseAppHost=false");
            buildArguments.Add($"--property:RuntimeIdentifier={runtimeIdentifier}");
            buildArguments.Add("--property:UseAppHost=false");
        }

        await RunDotnetAsync(consumerDirectory, restoreArguments.ToArray());
        string buildOutput = await RunDotnetAsync(consumerDirectory, buildArguments.ToArray());
        Assert.That(buildOutput, Does.Not.Contain("Downloading Slang"));
    }

    private string[] GetRuntimePackageDependencies()
    {
        List<string> dependencies = [];
        foreach (string assembly in RuntimeAssemblies)
        {
            string projectPath = Path.Combine(
                _repositoryDirectory,
                "src",
                assembly,
                $"{assembly}.csproj");
            XDocument project = XDocument.Load(projectPath);
            foreach (XElement packageReference in project.Descendants().Where(
                         element => element.Name.LocalName == "PackageReference"))
            {
                string? privateAssets = (string?)packageReference.Attribute("PrivateAssets")
                    ?? packageReference.Elements().SingleOrDefault(
                        element => element.Name.LocalName == "PrivateAssets")?.Value;
                if (string.Equals(privateAssets, "all", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string packageId = (string?)packageReference.Attribute("Include")
                    ?? throw new InvalidOperationException($"{projectPath} contains a PackageReference without Include.");
                string packageVersion = (string?)packageReference.Attribute("Version")
                    ?? packageReference.Elements().SingleOrDefault(
                        element => element.Name.LocalName == "Version")?.Value
                    ?? throw new InvalidOperationException($"{projectPath} contains a PackageReference without Version.");
                dependencies.Add($"{packageId}:{packageVersion}");
            }
        }

        return dependencies.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
    }

    private static bool IsNuGetEmptyFolderPlaceholder(string entry)
    {
        return entry == "_._" || entry.EndsWith("/_._", StringComparison.Ordinal);
    }

    private void AssertSlangPlatformDirectory(ZipArchive package, string platform)
    {
        string platformPrefix = $"tools/slang/{platform}/";
        HashSet<string> entries = package.Entries
            .Select(entry => entry.FullName)
            .Where(entry => entry.StartsWith(platformPrefix, StringComparison.Ordinal))
            .Select(entry => entry[platformPrefix.Length..])
            .Where(entry => !entry.EndsWith("/", StringComparison.Ordinal))
            .ToHashSet(StringComparer.Ordinal);
        string[] requiredEntries = GetRequiredSlangEntries(platform);

        Assert.That(
            entries,
            Is.EquivalentTo(requiredEntries),
            $"The packaged {platform} directory does not match the canonical Slang bundle manifest.");
    }

    private string[] GetSlangPlatforms()
    {
        return _slangBundleConfiguration
            .Descendants()
            .Where(element => element.Name.LocalName == "_SlangBundleDistribution")
            .Select(element => element.Element("Platform")?.Value
                ?? throw new InvalidOperationException("A Slang bundle distribution has no Platform metadata."))
            .ToArray();
    }

    private string[] GetRequiredSlangEntries(string platform)
    {
        XElement distribution = _slangBundleConfiguration
            .Descendants()
            .FirstOrDefault(element =>
                element.Name.LocalName == "_SlangBundleDistribution" &&
                element.Element("Platform")?.Value == platform)
            ?? throw new InvalidOperationException($"The Slang bundle manifest has no {platform} distribution.");
        string requiredFiles = distribution.Element("RequiredFiles")?.Value
            ?? throw new InvalidOperationException($"The {platform} distribution has no RequiredFiles metadata.");
        string slangVersion = _slangBundleConfiguration
            .Descendants()
            .FirstOrDefault(element => element.Name.LocalName == "_SlangPinnedVersion")?.Value
            ?? throw new InvalidOperationException("The Slang bundle manifest has no pinned Slang version.");
        string platformPrefix = $"{platform}/";

        return requiredFiles
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(file => file.Replace("$(SlangVersion)", slangVersion, StringComparison.Ordinal))
            .Select(file => file.StartsWith(platformPrefix, StringComparison.Ordinal)
                ? file[platformPrefix.Length..]
                : throw new InvalidOperationException(
                    $"Required Slang bundle file {file} does not start with {platformPrefix}."))
            .ToArray();
    }

    private static string ReadPackageEntry(ZipArchive package, string entryName)
    {
        ZipArchiveEntry entry = package.GetEntry(entryName)
            ?? throw new InvalidOperationException($"{entryName} is missing from the package.");
        using Stream stream = entry.Open();
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    private static string GetCurrentSlangPlatform()
    {
        Architecture architecture = RuntimeInformation.OSArchitecture;
        if (OperatingSystem.IsLinux() && architecture == Architecture.X64)
        {
            return "linux-x86_64";
        }

        if (OperatingSystem.IsLinux() && architecture == Architecture.Arm64)
        {
            return "linux-aarch64";
        }

        if (OperatingSystem.IsWindows() && architecture == Architecture.X64)
        {
            return "windows-x86_64";
        }

        if (OperatingSystem.IsMacOS() && architecture == Architecture.X64)
        {
            return "macos-x86_64";
        }

        if (OperatingSystem.IsMacOS() && architecture == Architecture.Arm64)
        {
            return "macos-aarch64";
        }

        throw new PlatformNotSupportedException(
            $"Unsupported package-integration host: {RuntimeInformation.OSDescription} {architecture}.");
    }

    private string GetRestoredSlangDirectory(string platform)
    {
        return Path.Combine(
            _packagesDirectory,
            "pixely",
            _packageVersion,
            "tools",
            "slang",
            platform);
    }

    private string GetConsumerDirectory(string name)
    {
        return Path.Combine(_repositoryDirectory, "tests", "Pixely.Package.Tests", "Consumers", name);
    }

    private void DeleteConsumerOutputs(string name)
    {
        if (string.IsNullOrEmpty(_repositoryDirectory))
        {
            return;
        }

        string consumerDirectory = GetConsumerDirectory(name);
        DeleteDirectory(Path.Combine(consumerDirectory, "bin"));
        DeleteDirectory(Path.Combine(consumerDirectory, "obj"));
        DeleteDirectory(Path.Combine(consumerDirectory, "Content", "shaders", ".generated"));
    }

    private static string GetRepositoryDirectory()
    {
        DirectoryInfo? directory = new(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Pixely.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Could not locate the repository directory.");
    }

    private static async Task<string> RunDotnetAsync(string workingDirectory, params string[] arguments)
    {
        ProcessStartInfo startInfo = new("dotnet")
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
            ?? throw new InvalidOperationException("Failed to start dotnet.");
        Task<string> standardOutput = process.StandardOutput.ReadToEndAsync();
        Task<string> standardError = process.StandardError.ReadToEndAsync();
        using CancellationTokenSource timeout = new(TimeSpan.FromMinutes(10));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(true);
            await process.WaitForExitAsync();
            throw new TimeoutException(
                $"dotnet {string.Join(' ', arguments)} exceeded the ten-minute test timeout.");
        }
        string output = await standardOutput;
        string error = await standardError;

        if (process.ExitCode != 0)
        {
            Assert.Fail(
                $"dotnet {string.Join(' ', arguments)} failed with exit code {process.ExitCode}.{Environment.NewLine}{output}{Environment.NewLine}{error}");
        }

        return output + error;
    }

    private static void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}
