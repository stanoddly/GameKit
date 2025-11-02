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
        var assemblyDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        var slangPath = Path.Combine(assemblyDir, "bin", "slangc");

        if (!File.Exists(slangPath))
        {
            throw new FileNotFoundException($"slangc compiler not found at {slangPath}");
        }

        return slangPath;
    }

    public void Compile(string[] filenames, bool onlySpirv, bool force)
    {
        var config = LoadConfigDefaults();
        
        // Apply config defaults if no command line args provided
        filenames = filenames.Length > 0 ? filenames : config.Filenames?.ToArray() ?? [];
        onlySpirv = onlySpirv || config.OnlySpirv;
        force = force || config.Force;

        if (filenames.Length == 0)
        {
            Console.WriteLine("Error: No filenames provided and none found in .spak.json");
            Environment.Exit(1);
        }

        var paths = filenames.Select(f => new FileInfo(f)).ToList();
        var directories = paths.Where(p => Directory.Exists(p.FullName)).ToList();
        var files = paths.Where(p => !Directory.Exists(p.FullName)).ToList();

        if (directories.Count > 0)
        {
            if (files.Count > 0)
            {
                Console.WriteLine("Warning: Ignoring files on command line because directories are present:");
                foreach (var file in files)
                {
                    Console.WriteLine($"  Ignored: {file.FullName}");
                }
            }

            foreach (var dir in directories)
            {
                var shaderFile = new FileInfo(Path.Combine(dir.FullName, "shader.slang"));
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
            foreach (var file in files)
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
        using var sha256 = SHA256.Create();
        using var stream = filePath.OpenRead();
        var hash = sha256.ComputeHash(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string GetTargetString(ShaderFormat format) => format switch
    {
        ShaderFormat.SpirV => "spirv",
        ShaderFormat.Dxil => "dxil",
        ShaderFormat.Msl => "metal",
        _ => throw new ArgumentException($"Unsupported shader format: {format}")
    };

    private static (FileInfo spirvFile, FileInfo reflectionFile) CompileSpirvWithReflection(
        FileInfo filePath, DirectoryInfo tempDir)
    {
        var filenameWithoutExt = Path.GetFileNameWithoutExtension(filePath.Name);
        var parentDir = filePath.Directory!;
        var reflectionFile = new FileInfo(Path.Combine(tempDir.FullName, "reflection.json"));
        var spirvOutputFile = new FileInfo(Path.Combine(parentDir.FullName, $"{filenameWithoutExt}.spv"));

        var args = new List<string>
        {
            filePath.FullName,
            "-warnings-disable", "39013,39001",
            "-reflection-json", reflectionFile.FullName,
            "-target", "spirv",
            "-capability", "glsl_spirv_1_0",
            "-emit-spirv-via-glsl",
            "-entry", "main",
            "-o", spirvOutputFile.FullName
        };

        // Add spak.conf options
        var spakConfOptions = LoadSpakConfOptions();
        args.AddRange(spakConfOptions);

        Console.WriteLine($"Executing SPIRV compilation: {SlangCompilerPath} {string.Join(" ", args)}");
        
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = SlangCompilerPath,
                Arguments = string.Join(" ", args.Select(arg => arg.Contains(' ') ? $"\"{arg}\"" : arg)),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };

        process.Start();
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Console.WriteLine($"Error compiling SPIRV shader: {stderr}");
            Environment.Exit(1);
        }

        if (!reflectionFile.Exists)
        {
            Console.WriteLine($"Error: Reflection file not generated at {reflectionFile.FullName}");
            Environment.Exit(1);
        }

        return (spirvOutputFile, reflectionFile);
    }

    private static (string entryPoint, ShaderStage stage, ShaderResources resources) ParseReflectionData(
        FileInfo reflectionFile)
    {
        var json = File.ReadAllText(reflectionFile.FullName);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        var entryPoint = "main";
        var stage = ShaderStage.Vertex;
        
        if (root.TryGetProperty("entryPoints", out var entryPoints) && entryPoints.GetArrayLength() > 0)
        {
            var firstEntry = entryPoints[0];
            if (firstEntry.TryGetProperty("name", out var nameElement))
            {
                entryPoint = nameElement.GetString() ?? "main";
            }
            
            if (firstEntry.TryGetProperty("stage", out var stageElement))
            {
                var stageStr = stageElement.GetString()?.ToLower();
                stage = stageStr switch
                {
                    "vertex" => ShaderStage.Vertex,
                    "fragment" or "pixel" => ShaderStage.Fragment,
                    _ => ShaderStage.Vertex
                };
                
                if (stageStr != "vertex" && stageStr != "fragment" && stageStr != "pixel")
                {
                    Console.WriteLine($"Warning: Unknown shader stage '{stageStr}', defaulting to vertex");
                }
            }
        }

        var samplers = 0;
        var storageTextures = 0;
        var storageBuffers = 0;
        var uniformBuffers = 0;

        if (root.TryGetProperty("parameters", out var parameters))
        {
            foreach (var param in parameters.EnumerateArray())
            {
                if (param.TryGetProperty("type", out var paramType) && 
                    paramType.TryGetProperty("kind", out var kindElement))
                {
                    var kind = kindElement.GetString();
                    switch (kind)
                    {
                        case "samplerState":
                            samplers++;
                            break;
                        case "resource":
                            // TODO: In C# this is currently unhandled (see TODO in original code)
                            // We could potentially check baseShape to determine the resource type
                            break;
                        case "constantBuffer":
                            uniformBuffers++;
                            break;
                    }
                }
            }
        }

        var resources = new ShaderResources(samplers, storageTextures, storageBuffers, uniformBuffers);
        return (entryPoint, stage, resources);
    }

    private static List<ShaderInstance> CompileOtherTargets(
        FileInfo filePath, DirectoryInfo tempDir, string entryPoint)
    {
        var filenameWithoutExt = Path.GetFileNameWithoutExtension(filePath.Name);
        var parentDir = filePath.Directory!;
        var shaderInstances = new List<ShaderInstance>();

        foreach (var (format, extension) in TargetsWithExtensions)
        {
            if (format == ShaderFormat.SpirV) continue;

            var target = GetTargetString(format);
            var outputFile = new FileInfo(Path.Combine(parentDir.FullName, $"{filenameWithoutExt}.{extension}"));
            var args = new List<string>
            {
                filePath.FullName,
                "-warnings-disable", "39013,39001",
                "-target", target
            };

            if (format == ShaderFormat.Dxil)
            {
                args.AddRange(["-profile", "sm_6_3"]);
            }

            args.AddRange(["-o", outputFile.FullName]);

            // Add spak.conf options
            var spakConfOptions = LoadSpakConfOptions();
            args.AddRange(spakConfOptions);

            Console.WriteLine($"Executing {target} compilation: {SlangCompilerPath} {string.Join(" ", args)}");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = SlangCompilerPath,
                    Arguments = string.Join(" ", args.Select(arg => arg.Contains(' ') ? $"\"{arg}\"" : arg)),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };

            process.Start();
            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Console.WriteLine($"Error compiling {target} shader: {stderr}");
                continue;
            }

            shaderInstances.Add(new ShaderInstance(format.ToString(), outputFile.Name, entryPoint));
            Console.WriteLine($"Wrote {outputFile.FullName}");
        }

        return shaderInstances;
    }

    private static void WriteMetadata(DirectoryInfo parentDir, string filenameWithoutExt, 
        ShaderStage stage, ShaderResources resources, List<ShaderInstance> shaderInstances, string fileHash)
    {
        var metadata = new ShaderMetadata(stage.ToString(), resources, shaderInstances, fileHash);
        var metadataFile = new FileInfo(Path.Combine(parentDir.FullName, $"{filenameWithoutExt}.metadata.json"));
        
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(metadata, options);
        File.WriteAllText(metadataFile.FullName, json);

        Console.WriteLine($"Metadata written to {metadataFile.FullName}");
        Console.WriteLine($"Stage: {stage}");
    }

    private static bool ShouldSkipCompilation(FileInfo filePath, bool force)
    {
        if (force) return false;

        var parentDir = filePath.Directory!;
        var filenameWithoutExt = Path.GetFileNameWithoutExtension(filePath.Name);
        var metadataFile = new FileInfo(Path.Combine(parentDir.FullName, $"{filenameWithoutExt}.metadata.json"));

        if (!metadataFile.Exists) return false;

        try
        {
            var json = File.ReadAllText(metadataFile.FullName);
            var metadata = JsonSerializer.Deserialize<ShaderMetadata>(json);
            
            if (metadata?.SourceHash == null) return false;

            var currentHash = CalculateFileHash(filePath);
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
        Console.WriteLine($"Processing file: {filePath.FullName}");

        var currentHash = CalculateFileHash(filePath);

        if (ShouldSkipCompilation(filePath, force))
        {
            Console.WriteLine($"Skipping {filePath.FullName} (unchanged)");
            return;
        }

        var parentDir = filePath.Directory!;
        Console.WriteLine($"Result directory: {parentDir.FullName}");

        var filenameWithoutExt = Path.GetFileNameWithoutExtension(filePath.Name);

        var tempDir = Directory.CreateTempSubdirectory("ShaderPack_");
        try
        {
            Console.WriteLine($"Intermediate results written to: {tempDir.FullName}");

            // Step 1: Compile SPIRV with reflection information
            var (spirvOutputFile, reflectionFile) = CompileSpirvWithReflection(filePath, tempDir);

            // Step 2: Parse reflection data
            var (entryPoint, stage, resources) = ParseReflectionData(reflectionFile);

            // Step 3: Add SPIRV to shader instances
            var shaderInstances = new List<ShaderInstance>
            {
                new(ShaderFormat.SpirV.ToString(), spirvOutputFile.Name, entryPoint)
            };

            // Step 4: Compile other targets and add to shader instances
            if (!spirvOnly)
            {
                var otherInstances = CompileOtherTargets(filePath, tempDir, entryPoint);
                shaderInstances.AddRange(otherInstances);
            }

            // Step 5: Write metadata
            WriteMetadata(parentDir, filenameWithoutExt, stage, resources, shaderInstances, currentHash);
        }
        finally
        {
            tempDir.Delete(true);
        }
    }

    private static FileInfo? FindSpakConf()
    {
        var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        
        while (currentDir != null)
        {
            var confFile = new FileInfo(Path.Combine(currentDir.FullName, "spak.conf"));
            if (confFile.Exists)
            {
                return confFile;
            }
            currentDir = currentDir.Parent;
        }
        
        return null;
    }

    private static List<string> LoadSpakConfOptions()
    {
        var confFile = FindSpakConf();
        if (confFile == null) return new List<string>();

        try
        {
            var lines = File.ReadAllLines(confFile.FullName);
            var options = new List<string>();
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith('#'))
                    continue;
                    
                // Split line into option and value parts
                var parts = trimmedLine.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                options.AddRange(parts);
            }
            
            return options;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Warning: Could not read spak.conf: {ex.Message}");
            return new List<string>();
        }
    }

    private static SpakConfig LoadConfigDefaults()
    {
        var configFile = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), ".spak.json"));
        if (!configFile.Exists) return new SpakConfig();

        try
        {
            var json = File.ReadAllText(configFile.FullName);
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