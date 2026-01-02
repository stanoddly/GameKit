using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using GameKit.ShaderCommon;

namespace GameKit.SdlangCompileLib;

internal enum ResourceType
{
    SampledTexture,
    StorageTexture,
    StorageBuffer,
    Sampler,
    UniformBuffer
}

internal record struct ResourceBinding(string Name, ResourceType Type, int Space, int Index);

public class ShaderBindingValidationException(string message) : Exception(message);

public class SdlangCompiler
{
    private static readonly string SlangCompilerPath = GetSlangCompilerPath();
    private static readonly string SlangVersion = GetSlangVersion();
    
    private static readonly Dictionary<ShaderFormatDto, string> TargetsWithExtensions = new()
    {
        { ShaderFormatDto.SpirV, "spv" },
        { ShaderFormatDto.Dxil, "dxil" },
        { ShaderFormatDto.Msl, "metal" }
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

    private static string GetSlangVersion()
    {
        var attribute = typeof(SdlangCompiler).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == "SlangVersion");

        return attribute?.Value ?? throw new InvalidOperationException("SlangVersion not found in assembly metadata");
    }

    public void Compile(string[] filenames, bool force)
    {
        if (filenames.Length == 0)
        {
            Console.WriteLine("Error: No filenames provided");
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
                CompileShader(shaderFile, force);
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
                CompileShader(file, force);
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

    private static string GetTargetString(ShaderFormatDto format) => format switch
    {
        ShaderFormatDto.SpirV => "spirv",
        ShaderFormatDto.Dxil => "dxil",
        ShaderFormatDto.Msl => "metal",
        _ => throw new ArgumentException($"Unsupported shader format: {format}")
    };

    private static readonly Dictionary<ShaderFormatDto, List<string>> CommandLineOptions = new()
    {
        { ShaderFormatDto.SpirV, [] },
        { ShaderFormatDto.Dxil, ["-profile", "sm_6_3"] },
        { ShaderFormatDto.Msl, [] }
    };

    private static (FileInfo reflectionFile, List<ShaderInstanceDto> shaderInstances) CompileTargets(
        FileInfo filePath, DirectoryInfo tempDir, DirectoryInfo outputDir, List<ShaderFormatDto> targets)
    {
        string filenameWithoutExt = Path.GetFileNameWithoutExtension(filePath.Name);
        FileInfo reflectionFile = new FileInfo(Path.Combine(tempDir.FullName, "reflection.json"));

        List<string> args = new List<string>
        {
            filePath.FullName,
            "-warnings-disable", "39001,39013,39029",
            "-reflection-json", reflectionFile.FullName
        };

        List<ShaderInstanceDto> shaderInstances = new List<ShaderInstanceDto>();

        // Add all requested targets
        foreach (ShaderFormatDto format in targets)
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
            shaderInstances.Add(new ShaderInstanceDto(format, outputFile.Name, "main"));
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

    private static void ValidateBindings(ShaderStageDto stage, List<ResourceBinding> bindings)
    {
        // Determine expected spaces based on shader stage
        // SDL GPU requires:
        // - Vertex: textures/samplers/buffers in space 0, uniforms in space 1
        // - Fragment: textures/samplers/buffers in space 2, uniforms in space 3
        int expectedResourceSpace = stage == ShaderStageDto.Vertex ? 0 : 2;
        int expectedUniformSpace = stage == ShaderStageDto.Vertex ? 1 : 3;
        string stageName = stage == ShaderStageDto.Vertex ? "vertex" : "fragment";

        // Validate space for each binding
        foreach (var binding in bindings)
        {
            int expectedSpace = binding.Type == ResourceType.UniformBuffer ? expectedUniformSpace : expectedResourceSpace;
            if (binding.Space != expectedSpace)
            {
                throw new ShaderBindingValidationException(
                    $"Parameter '{binding.Name}' in {stageName} shader uses space {binding.Space}, " +
                    $"but SDL GPU requires space {expectedSpace} for {GetResourceTypeName(binding.Type)}");
            }
        }

        // Validate index ordering within the resource space (sampled textures, then storage textures, then storage buffers)
        var resourceBindings = bindings
            .Where(b => b.Type != ResourceType.UniformBuffer && b.Type != ResourceType.Sampler)
            .OrderBy(b => b.Index)
            .ToList();

        if (resourceBindings.Count == 0)
            return;

        // Group by type and verify ordering
        var sampledTextures = resourceBindings.Where(b => b.Type == ResourceType.SampledTexture).ToList();
        var storageTextures = resourceBindings.Where(b => b.Type == ResourceType.StorageTexture).ToList();
        var storageBuffers = resourceBindings.Where(b => b.Type == ResourceType.StorageBuffer).ToList();

        int expectedIndex = 0;

        // Validate sampled textures come first and are contiguous starting at 0
        foreach (var tex in sampledTextures.OrderBy(t => t.Index))
        {
            if (tex.Index != expectedIndex)
            {
                throw new ShaderBindingValidationException(
                    $"Sampled texture '{tex.Name}' has index {tex.Index}, but expected {expectedIndex}. " +
                    $"SDL GPU requires sampled textures at indices 0..N-1");
            }
            expectedIndex++;
        }

        // Validate storage textures come next
        foreach (var tex in storageTextures.OrderBy(t => t.Index))
        {
            if (tex.Index != expectedIndex)
            {
                throw new ShaderBindingValidationException(
                    $"Storage texture '{tex.Name}' has index {tex.Index}, but expected {expectedIndex}. " +
                    $"SDL GPU requires storage textures immediately after sampled textures");
            }
            expectedIndex++;
        }

        // Validate storage buffers come last
        foreach (var buf in storageBuffers.OrderBy(b => b.Index))
        {
            if (buf.Index != expectedIndex)
            {
                throw new ShaderBindingValidationException(
                    $"Storage buffer '{buf.Name}' has index {buf.Index}, but expected {expectedIndex}. " +
                    $"SDL GPU requires storage buffers immediately after storage textures");
            }
            expectedIndex++;
        }
    }

    private static string GetResourceTypeName(ResourceType type) => type switch
    {
        ResourceType.SampledTexture => "sampled textures",
        ResourceType.StorageTexture => "storage textures",
        ResourceType.StorageBuffer => "storage buffers",
        ResourceType.Sampler => "samplers",
        ResourceType.UniformBuffer => "uniform buffers",
        _ => type.ToString()
    };

    private static (string entryPoint, ShaderStageDto stage, ShaderBindingLayout resources) ParseReflectionData(
        FileInfo reflectionFile)
    {
        string json = File.ReadAllText(reflectionFile.FullName);
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;

        string entryPoint = "main";
        ShaderStageDto stage = ShaderStageDto.Vertex;

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
                    "vertex" => ShaderStageDto.Vertex,
                    "fragment" or "pixel" => ShaderStageDto.Fragment,
                    _ => throw new InvalidOperationException($"Unknown shader stage '{stageStr}'")
                };
            }
        }

        ShaderUniformSlotSizes shaderUniformSlots = new();

        byte samplers = 0;
        byte storageTextures = 0;
        byte storageBuffers = 0;

        List<ResourceBinding> resourceBindings = new();

        if (root.TryGetProperty("parameters", out JsonElement parameters))
        {
            foreach (JsonElement param in parameters.EnumerateArray())
            {
                if (param.TryGetProperty("type", out JsonElement paramType) &&
                    paramType.TryGetProperty("kind", out JsonElement kindElement))
                {
                    string? kind = kindElement.GetString();
                    string paramName = param.TryGetProperty("name", out JsonElement nameEl)
                        ? nameEl.GetString() ?? "unknown"
                        : "unknown";
                    (int space, int index) = GetBindingInfo(param);

                    switch (kind)
                    {
                        case "samplerState":
                            samplers++;
                            resourceBindings.Add(new ResourceBinding(paramName, ResourceType.Sampler, space, index));
                            break;
                        case "resource":
                            if (paramType.TryGetProperty("baseShape", out JsonElement baseShapeElement))
                            {
                                string? baseShape = baseShapeElement.GetString();
                                if (baseShape == "structuredBuffer")
                                {
                                    storageBuffers++;
                                    resourceBindings.Add(new ResourceBinding(paramName, ResourceType.StorageBuffer, space, index));
                                }
                                else
                                {
                                    // texture2D and other texture types are sampled textures
                                    resourceBindings.Add(new ResourceBinding(paramName, ResourceType.SampledTexture, space, index));
                                }
                            }
                            break;
                        case "constantBuffer":
                            AdjustUniformBuffers(param, ref shaderUniformSlots);
                            resourceBindings.Add(new ResourceBinding(paramName, ResourceType.UniformBuffer, space, index));
                            break;
                    }
                }
            }
        }

        // Validate bindings conform to SDL GPU requirements
        ValidateBindings(stage, resourceBindings);

        ShaderBindingLayout shaderBindingLayout = new ShaderBindingLayout(new ShaderBindingCounts(samplers, storageTextures, storageBuffers), shaderUniformSlots);
        return (entryPoint, stage, shaderBindingLayout);
    }

    private static (int space, int index) GetBindingInfo(JsonElement param)
    {
        int space = 0;
        int index = 0;

        if (param.TryGetProperty("binding", out JsonElement binding))
        {
            if (binding.TryGetProperty("space", out JsonElement spaceEl))
            {
                space = spaceEl.GetInt32();
            }
            if (binding.TryGetProperty("index", out JsonElement indexEl))
            {
                index = indexEl.GetInt32();
            }
        }

        return (space, index);
    }

    private static void AdjustUniformBuffers(JsonElement param, ref ShaderUniformSlotSizes shaderUniformSlots)
    {
        // For constant buffers, binding information is required
        if (!param.TryGetProperty("binding", out JsonElement binding))
        {
            throw new InvalidOperationException("constantBuffer parameter missing required 'binding' property");
        }

        if (!binding.TryGetProperty("index", out JsonElement indexElement))
        {
            throw new InvalidOperationException("constantBuffer binding missing required 'index' property");
        }

        if (!indexElement.TryGetInt32(out int slotIndex))
        {
            throw new InvalidOperationException("constantBuffer binding 'index' is not a valid integer");
        }

        // Extract size information from type.elementVarLayout.binding
        if (!param.TryGetProperty("type", out JsonElement typeElement))
        {
            throw new InvalidOperationException("constantBuffer parameter missing required 'type' property");
        }

        if (!typeElement.TryGetProperty("elementVarLayout", out JsonElement elementVarLayout))
        {
            throw new InvalidOperationException("constantBuffer type missing required 'elementVarLayout' property");
        }

        if (!elementVarLayout.TryGetProperty("binding", out JsonElement layoutBinding))
        {
            throw new InvalidOperationException("constantBuffer elementVarLayout missing required 'binding' property");
        }

        if (!layoutBinding.TryGetProperty("size", out JsonElement sizeElement))
        {
            throw new InvalidOperationException("constantBuffer layout binding missing required 'size' property");
        }

        if (!sizeElement.TryGetByte(out byte bufferSize))
        {
            throw new InvalidOperationException("constantBuffer layout binding 'size' is not a valid integer");
        }

        // Update the appropriate slot based on the index
        // Valid indices are 0-3 corresponding to Slot1-Slot4
        if (slotIndex < 0 || slotIndex > 3)
        {
            throw new InvalidOperationException($"constantBuffer slot index {slotIndex} is out of valid range [0-3]");
        }

        shaderUniformSlots = slotIndex switch
        {
            0 => shaderUniformSlots with { Slot0 = bufferSize },
            1 => shaderUniformSlots with { Slot1 = bufferSize },
            2 => shaderUniformSlots with { Slot2 = bufferSize },
            3 => shaderUniformSlots with { Slot3 = bufferSize },
            // TODO: error message
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static void WriteMetadata(DirectoryInfo outputDir, string filenameWithoutExt,
        ShaderStageDto stage, ShaderBindingLayout resources, List<ShaderInstanceDto> shaderInstances, string fileHash)
    {
        ShaderMetadataDto metadata = new ShaderMetadataDto(stage, resources, shaderInstances, fileHash, SlangVersion);
        FileInfo metadataFile = new FileInfo(Path.Combine(outputDir.FullName, $"{filenameWithoutExt}.metadata.json"));

        using FileStream stream = metadataFile.Create();
        JsonSerializer.Serialize(stream, metadata, ShaderMetadataJsonContext.Default.ShaderMetadataDto);
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
            ShaderMetadataDto? metadata = JsonSerializer.Deserialize<ShaderMetadataDto>(json);

            if (metadata?.SourceHash == null) return false;

            // Force recompilation if slang version is missing or different
            if (metadata.SlangVersion == null || metadata.SlangVersion != SlangVersion) return false;

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

    private static void CompileShader(FileInfo filePath, bool force = false)
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
            List<ShaderFormatDto> targets = [ShaderFormatDto.SpirV];
            (FileInfo reflectionFile, List<ShaderInstanceDto> shaderInstances) = CompileTargets(filePath, tempDir, outputDir, targets);

            // Step 2: Parse reflection data
            (string entryPoint, ShaderStageDto stage, ShaderBindingLayout bindingLayout) = ParseReflectionData(reflectionFile);

            // Step 3: Update shader instances with correct entry point
            shaderInstances = shaderInstances.Select(instance =>
                new ShaderInstanceDto(instance.Format, instance.Filename, entryPoint)).ToList();

            // Step 4: Write metadata
            WriteMetadata(outputDir, filenameWithoutExt, stage, bindingLayout, shaderInstances, currentHash);
        }
        finally
        {
            tempDir.Delete(true);
        }
    }


}