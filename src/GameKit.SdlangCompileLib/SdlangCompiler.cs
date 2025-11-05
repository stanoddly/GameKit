using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

namespace GameKit.SdlangCompileLib;

public enum ShaderStage
{
    Vertex,
    Fragment
}

public enum ShaderFormat
{
    SpirV,
    Dxil,
    Msl
}

public record ShaderResources(
    int Samplers,
    int StorageTextures,
    int StorageBuffers,
    int UniformBuffers
);

public record ShaderInstance(
    string Format,
    string Filename,
    string EntryPoint
);

public record ShaderMetadata(
    string Stage,
    ShaderResources Resources,
    List<ShaderInstance> Shaders,
    string SourceHash
);

public record SpakConfig(
    List<string>? Filenames = null,
    bool OnlySpirv = false,
    bool Force = false
);

public class SdlangCompiler
{
    private static readonly string SlangCompilerPath = GetSlangCompilerPath();
    
    private static readonly Dictionary<ShaderFormat, string> TargetsWithExtensions = new()
    {
        { ShaderFormat.SpirV, "spv" },
        { ShaderFormat.Dxil, "dxil" },
        { ShaderFormat.Msl, "metal" }
    };

    private static string GetSlangCompilerPath()
    {
        string? assemblyDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        if (string.IsNullOrEmpty(assemblyDir))
        {
            throw new InvalidOperationException("Unable to determine assembly directory");
        }
        string slangPath = Path.Combine(assemblyDir, "bin", "slangc");

        if (!File.Exists(slangPath))
        {
            throw new FileNotFoundException($"slangc compiler not found at {slangPath}");
        }

        return slangPath;
    }

    public void Compile(string[] filenames, bool onlySpirv, bool force)
    {
        SpakConfig config = LoadConfigDefaults();
        
        // Apply config defaults if no command line args provided
        filenames = filenames.Length > 0 ? filenames : config.Filenames?.ToArray() ?? [];
        onlySpirv = onlySpirv || config.OnlySpirv;
        force = force || config.Force;

        if (filenames.Length == 0)
        {
            Console.WriteLine("Error: No filenames provided and none found in .spak.json");
            Environment.Exit(1);
        }

        List<FileInfo> paths = filenames.Select(f => new FileInfo(f)).ToList();
        List<FileInfo> directories = paths.Where(p => Directory.Exists(p.FullName)).ToList();
        List<FileInfo> files = paths.Where(p => !Directory.Exists(p.FullName)).ToList();

        if (directories.Count > 0)
        {
            if (files.Count > 0)
            {
                Console.WriteLine("Warning: Ignoring files on command line because directories are present:");
                foreach (FileInfo file in files)
                {
                    Console.WriteLine($"  Ignored: {file.FullName}");
                }
            }

            foreach (FileInfo dir in directories)
            {
                FileInfo shaderFile = new FileInfo(Path.Combine(dir.FullName, "shader.slang"));
                if (!shaderFile.Exists)
                {
                    Console.WriteLine($"Error: File {shaderFile.FullName} does not exist");
                    Environment.Exit(1);
                }
                CompileShader(shaderFile, onlySpirv, force);
            }
        }
        else
        {
            foreach (FileInfo file in files)
            {
                if (!file.Exists)
                {
                    Console.WriteLine($"Error: File {file.FullName} does not exist");
                    Environment.Exit(1);
                }
                CompileShader(file, onlySpirv, force);
            }
        }
    }

    private static string CalculateFileHash(FileInfo filePath)
    {
        using SHA256 sha256 = SHA256.Create();
        using FileStream stream = filePath.OpenRead();
        byte[] hash = sha256.ComputeHash(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string GetTargetString(ShaderFormat format) => format switch
    {
        ShaderFormat.SpirV => "spirv",
        ShaderFormat.Dxil => "dxil",
        ShaderFormat.Msl => "metal",
        _ => throw new ArgumentException($"Unsupported shader format: {format}")
    };

    private static readonly Dictionary<ShaderFormat, List<string>> CommandLineOptions = new()
    {
        { ShaderFormat.SpirV, ["-capability", "glsl_spirv_1_0", "-emit-spirv-via-glsl"] },
        { ShaderFormat.Dxil, ["-profile", "sm_6_3"] },
        { ShaderFormat.Msl, [] }
    };

    private static (FileInfo reflectionFile, List<ShaderInstance> shaderInstances) CompileTargets(
        FileInfo filePath, DirectoryInfo tempDir, DirectoryInfo outputDir, List<ShaderFormat> targets)
    {
        string filenameWithoutExt = Path.GetFileNameWithoutExtension(filePath.Name);
        FileInfo reflectionFile = new FileInfo(Path.Combine(tempDir.FullName, "reflection.json"));

        List<string> args = new List<string>
        {
            filePath.FullName,
            "-warnings-disable", "39013,39001",
            "-reflection-json", reflectionFile.FullName
        };

        List<ShaderInstance> shaderInstances = new List<ShaderInstance>();

        // Add all requested targets
        foreach (ShaderFormat format in targets)
        {
            string target = GetTargetString(format);
            string extension = TargetsWithExtensions[format];
            FileInfo outputFile = new FileInfo(Path.Combine(outputDir.FullName, $"{filenameWithoutExt}.{extension}"));

            args.AddRange(["-target", target]);

            // Add format-specific options
            List<string> options = CommandLineOptions[format];
            args.AddRange(options);

            args.AddRange(["-entry", "main"]);
            args.AddRange(["-o", outputFile.FullName]);
            shaderInstances.Add(new ShaderInstance(format.ToString(), outputFile.Name, "main"));
        }

        Console.WriteLine($"Executing shader compilation: {SlangCompilerPath} {string.Join(" ", args)}");

        Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = SlangCompilerPath,
                Arguments = string.Join(" ", args.Select(arg => arg.Contains(' ') ? $"\"{arg}\"" : arg)),
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = false
            }
        };

        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Console.WriteLine($"Error compiling shader");
            Environment.Exit(1);
        }

        return (reflectionFile, shaderInstances);
    }

    private static (string entryPoint, ShaderStage stage, ShaderResources resources) ParseReflectionData(
        FileInfo reflectionFile)
    {
        string json = File.ReadAllText(reflectionFile.FullName);
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;

        string entryPoint = "main";
        ShaderStage stage = ShaderStage.Vertex;

        if (root.TryGetProperty("entryPoints", out JsonElement entryPoints) && entryPoints.GetArrayLength() > 0)
        {
            JsonElement firstEntry = entryPoints[0];
            if (firstEntry.TryGetProperty("name", out JsonElement nameElement))
            {
                entryPoint = nameElement.GetString() ?? "main";
            }

            if (firstEntry.TryGetProperty("stage", out JsonElement stageElement))
            {
                string? stageStr = stageElement.GetString()?.ToLower();
                stage = stageStr switch
                {
                    "vertex" => ShaderStage.Vertex,
                    "fragment" or "pixel" => ShaderStage.Fragment,
                    _ => throw new InvalidOperationException($"Unknown shader stage '{stageStr}'")
                };
            }
        }

        int samplers = 0;
        int storageTextures = 0;
        int storageBuffers = 0;
        int uniformBuffers = 0;

        if (root.TryGetProperty("parameters", out JsonElement parameters))
        {
            foreach (JsonElement param in parameters.EnumerateArray())
            {
                if (param.TryGetProperty("type", out JsonElement paramType) &&
                    paramType.TryGetProperty("kind", out JsonElement kindElement))
                {
                    string? kind = kindElement.GetString();
                    switch (kind)
                    {
                        case "samplerState":
                            samplers++;
                            break;
                        case "resource":
                            // We could potentially check baseShape to determine the resource type
                            break;
                        case "constantBuffer":
                            uniformBuffers++;
                            break;
                    }
                }
            }
        }

        ShaderResources resources = new ShaderResources(samplers, storageTextures, storageBuffers, uniformBuffers);
        return (entryPoint, stage, resources);
    }

    private static void WriteMetadata(DirectoryInfo outputDir, string filenameWithoutExt,
        ShaderStage stage, ShaderResources resources, List<ShaderInstance> shaderInstances, string fileHash)
    {
        ShaderMetadata metadata = new ShaderMetadata(stage.ToString(), resources, shaderInstances, fileHash);
        FileInfo metadataFile = new FileInfo(Path.Combine(outputDir.FullName, $"{filenameWithoutExt}.metadata.json"));

        JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(metadata, options);
        File.WriteAllText(metadataFile.FullName, json);
    }

    private static bool ShouldSkipCompilation(FileInfo filePath, DirectoryInfo outputDir, bool force)
    {
        if (force) return false;

        string filenameWithoutExt = Path.GetFileNameWithoutExtension(filePath.Name);
        FileInfo metadataFile = new FileInfo(Path.Combine(outputDir.FullName, $"{filenameWithoutExt}.metadata.json"));

        if (!metadataFile.Exists) return false;

        try
        {
            string json = File.ReadAllText(metadataFile.FullName);
            ShaderMetadata? metadata = JsonSerializer.Deserialize<ShaderMetadata>(json);

            if (metadata?.SourceHash == null) return false;

            string currentHash = CalculateFileHash(filePath);
            return metadata.SourceHash == currentHash;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }

    private static void CompileShader(FileInfo filePath, bool spirvOnly = false, bool force = false)
    {
        DirectoryInfo parentDir = filePath.Directory!;
        DirectoryInfo outputDir = new DirectoryInfo(Path.Combine(parentDir.FullName, "compiled"));

        string currentHash = CalculateFileHash(filePath);

        if (ShouldSkipCompilation(filePath, outputDir, force))
        {
            Console.WriteLine($"Skipping {filePath.FullName} (unchanged)");
            return;
        }

        Console.WriteLine($"Result directory: {outputDir.FullName}");

        string filenameWithoutExt = Path.GetFileNameWithoutExtension(filePath.Name);

        // Ensure output directory exists
        outputDir.Create();

        DirectoryInfo tempDir = Directory.CreateTempSubdirectory("ShaderPack_");
        try
        {
            Console.WriteLine($"Intermediate results written to: {tempDir.FullName}");

            // Step 1: Compile all targets in a single slangc invocation
            List<ShaderFormat> targets = spirvOnly
                ? [ShaderFormat.SpirV]
                : [ShaderFormat.SpirV, ShaderFormat.Dxil, ShaderFormat.Msl];
            (FileInfo reflectionFile, List<ShaderInstance> shaderInstances) = CompileTargets(filePath, tempDir, outputDir, targets);

            // Step 2: Parse reflection data
            (string entryPoint, ShaderStage stage, ShaderResources resources) = ParseReflectionData(reflectionFile);

            // Step 3: Update shader instances with correct entry point
            shaderInstances = shaderInstances.Select(instance =>
                new ShaderInstance(instance.Format, instance.Filename, entryPoint)).ToList();

            // Step 4: Write metadata
            WriteMetadata(outputDir, filenameWithoutExt, stage, resources, shaderInstances, currentHash);
        }
        finally
        {
            tempDir.Delete(true);
        }
    }


    private static SpakConfig LoadConfigDefaults()
    {
        FileInfo configFile = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), ".spak.json"));
        if (!configFile.Exists) return new SpakConfig();

        try
        {
            string json = File.ReadAllText(configFile.FullName);
            return JsonSerializer.Deserialize<SpakConfig>(json) ?? new SpakConfig();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Warning: Could not parse .spak.json: {ex.Message}");
            return new SpakConfig();
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Warning: Could not read .spak.json: {ex.Message}");
            return new SpakConfig();
        }
    }
}