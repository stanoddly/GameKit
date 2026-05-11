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
    UniformBuffer,
    ReadWriteStorageTexture,
    ReadWriteStorageBuffer
}

internal record struct ResourceBinding(string Name, ResourceType Type, int Space, int Index);

public class ShaderCompilationException(string message) : Exception(message);

public class ShaderBindingValidationException(string message) : Exception(message);

public class SdlangCompiler
{
    private const string GeneratedShaderDirectory = ".generated";
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
        string slangExe = OperatingSystem.IsWindows() ? "slangc.exe" : "slangc";
        string slangPath = Path.Combine(assemblyDir, "bin", slangExe);

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
            throw new ShaderCompilationException("No filenames provided");
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
                    throw new ShaderCompilationException($"File {shaderFile.FullName} does not exist");
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
                    throw new ShaderCompilationException($"File {file.FullName} does not exist");
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

        // slangc reads from stdin even when given a file argument. Without redirecting and closing
        // stdin, it inherits the parent's stdin and blocks indefinitely when stdin is a pipe.
        Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = SlangCompilerPath,
                Arguments = string.Join(" ", args.Select(arg => arg.Contains(' ') ? $"\"{arg}\"" : arg)),
                RedirectStandardInput = true,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = false
            }
        };

        process.Start();
        process.StandardInput.Close();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new ShaderCompilationException($"Shader compilation failed with exit code {process.ExitCode}");
        }

        return (reflectionFile, shaderInstances);
    }

    private static void ValidateBindings(ShaderStageDto stage, List<ResourceBinding> bindings)
    {
        // Determine expected spaces based on shader stage.
        // SDL GPU Vulkan backend descriptor set layout:
        // - Vertex: readonly resources in space 0, uniforms in space 1
        // - Fragment: readonly resources in space 2, uniforms in space 3
        // - Compute: readonly resources in space 0, readwrite resources in space 1, uniforms in space 2
        int expectedReadOnlyResourceSpace;
        int expectedReadWriteResourceSpace;
        int expectedUniformSpace;
        string stageName;

        switch (stage)
        {
            case ShaderStageDto.Vertex:
                expectedReadOnlyResourceSpace = 0;
                expectedReadWriteResourceSpace = -1;
                expectedUniformSpace = 1;
                stageName = "vertex";
                break;
            case ShaderStageDto.Fragment:
                expectedReadOnlyResourceSpace = 2;
                expectedReadWriteResourceSpace = -1;
                expectedUniformSpace = 3;
                stageName = "fragment";
                break;
            case ShaderStageDto.Compute:
                expectedReadOnlyResourceSpace = 0;
                expectedReadWriteResourceSpace = 1;
                expectedUniformSpace = 2;
                stageName = "compute";
                break;
            default:
                throw new InvalidOperationException($"Unknown shader stage: {stage}");
        }

        foreach (ResourceBinding binding in bindings)
        {
            int expectedSpace;
            if (binding.Type == ResourceType.UniformBuffer)
            {
                expectedSpace = expectedUniformSpace;
            }
            else if (binding.Type == ResourceType.ReadWriteStorageTexture || binding.Type == ResourceType.ReadWriteStorageBuffer)
            {
                expectedSpace = expectedReadWriteResourceSpace;
            }
            else
            {
                expectedSpace = expectedReadOnlyResourceSpace;
            }

            if (binding.Space != expectedSpace)
            {
                throw new ShaderBindingValidationException(
                    $"Parameter '{binding.Name}' in {stageName} shader uses space {binding.Space}, " +
                    $"but SDL GPU requires space {expectedSpace} for {GetResourceTypeName(binding.Type)}");
            }
        }

        // Validate index ordering within the resource space
        // Read-only resources: sampled textures, then storage textures, then storage buffers
        // Read-write resources (compute only): separate index space — readwrite storage textures, then readwrite storage buffers
        List<ResourceBinding> readOnlyResourceBindings = bindings
            .Where(b => b.Type != ResourceType.UniformBuffer && b.Type != ResourceType.Sampler
                && b.Type != ResourceType.ReadWriteStorageTexture && b.Type != ResourceType.ReadWriteStorageBuffer)
            .OrderBy(b => b.Index)
            .ToList();

        List<ResourceBinding> sampledTextures = readOnlyResourceBindings.Where(b => b.Type == ResourceType.SampledTexture).ToList();
        List<ResourceBinding> storageTextures = readOnlyResourceBindings.Where(b => b.Type == ResourceType.StorageTexture).ToList();
        List<ResourceBinding> storageBuffers = readOnlyResourceBindings.Where(b => b.Type == ResourceType.StorageBuffer).ToList();

        int expectedIndex = 0;

        // Validate sampled textures come first and are contiguous starting at 0
        foreach (ResourceBinding tex in sampledTextures.OrderBy(t => t.Index))
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
        foreach (ResourceBinding tex in storageTextures.OrderBy(t => t.Index))
        {
            if (tex.Index != expectedIndex)
            {
                throw new ShaderBindingValidationException(
                    $"Storage texture '{tex.Name}' has index {tex.Index}, but expected {expectedIndex}. " +
                    $"SDL GPU requires storage textures immediately after sampled textures");
            }
            expectedIndex++;
        }

        // Validate storage buffers come after storage textures
        foreach (ResourceBinding buf in storageBuffers.OrderBy(b => b.Index))
        {
            if (buf.Index != expectedIndex)
            {
                throw new ShaderBindingValidationException(
                    $"Storage buffer '{buf.Name}' has index {buf.Index}, but expected {expectedIndex}. " +
                    $"SDL GPU requires storage buffers immediately after storage textures");
            }
            expectedIndex++;
        }

        // Read-write resources use a separate index space starting from 0
        List<ResourceBinding> readWriteStorageTextures = bindings.Where(b => b.Type == ResourceType.ReadWriteStorageTexture).ToList();
        List<ResourceBinding> readWriteStorageBuffers = bindings.Where(b => b.Type == ResourceType.ReadWriteStorageBuffer).ToList();

        int rwExpectedIndex = 0;

        // Validate read-write storage textures start at index 0 and are contiguous
        foreach (ResourceBinding tex in readWriteStorageTextures.OrderBy(t => t.Index))
        {
            if (tex.Index != rwExpectedIndex)
            {
                throw new ShaderBindingValidationException(
                    $"Read-write storage texture '{tex.Name}' has index {tex.Index}, but expected {rwExpectedIndex}.");
            }
            rwExpectedIndex++;
        }

        // Validate read-write storage buffers come after read-write storage textures
        foreach (ResourceBinding buf in readWriteStorageBuffers.OrderBy(b => b.Index))
        {
            if (buf.Index != rwExpectedIndex)
            {
                throw new ShaderBindingValidationException(
                    $"Read-write storage buffer '{buf.Name}' has index {buf.Index}, but expected {rwExpectedIndex}.");
            }
            rwExpectedIndex++;
        }
    }

    private static string GetResourceTypeName(ResourceType type) => type switch
    {
        ResourceType.SampledTexture => "sampled textures",
        ResourceType.StorageTexture => "storage textures",
        ResourceType.StorageBuffer => "storage buffers",
        ResourceType.Sampler => "samplers",
        ResourceType.UniformBuffer => "uniform buffers",
        ResourceType.ReadWriteStorageTexture => "read-write storage textures",
        ResourceType.ReadWriteStorageBuffer => "read-write storage buffers",
        _ => type.ToString()
    };

    private static (string entryPoint, ShaderStageDto stage, ShaderBindingLayout resources, uint threadCountX, uint threadCountY, uint threadCountZ) ParseReflectionData(
        FileInfo reflectionFile)
    {
        string json = File.ReadAllText(reflectionFile.FullName);
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;

        string entryPoint = "main";
        ShaderStageDto stage = ShaderStageDto.Vertex;
        uint threadCountX = 1;
        uint threadCountY = 1;
        uint threadCountZ = 1;

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
                    "compute" => ShaderStageDto.Compute,
                    _ => throw new InvalidOperationException($"Unknown shader stage '{stageStr}'")
                };
            }

            if (firstEntry.TryGetProperty("threadGroupSize", out JsonElement threadGroupSize))
            {
                JsonElement.ArrayEnumerator enumerator = threadGroupSize.EnumerateArray();
                if (enumerator.MoveNext())
                {
                    threadCountX = enumerator.Current.GetUInt32();
                }
                if (enumerator.MoveNext())
                {
                    threadCountY = enumerator.Current.GetUInt32();
                }
                if (enumerator.MoveNext())
                {
                    threadCountZ = enumerator.Current.GetUInt32();
                }
            }
        }

        ShaderUniformSlotSizes shaderUniformSlots = new();

        byte samplers = 0;
        byte storageTextures = 0;
        byte storageBuffers = 0;
        byte readWriteStorageTextures = 0;
        byte readWriteStorageBuffers = 0;

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
                                bool isReadWrite = paramType.TryGetProperty("access", out JsonElement accessElement)
                                    && accessElement.GetString() == "readWrite";

                                if (baseShape == "structuredBuffer")
                                {
                                    if (isReadWrite)
                                    {
                                        readWriteStorageBuffers++;
                                        resourceBindings.Add(new ResourceBinding(paramName, ResourceType.ReadWriteStorageBuffer, space, index));
                                    }
                                    else
                                    {
                                        storageBuffers++;
                                        resourceBindings.Add(new ResourceBinding(paramName, ResourceType.StorageBuffer, space, index));
                                    }
                                }
                                else
                                {
                                    if (isReadWrite)
                                    {
                                        readWriteStorageTextures++;
                                        resourceBindings.Add(new ResourceBinding(paramName, ResourceType.ReadWriteStorageTexture, space, index));
                                    }
                                    else
                                    {
                                        // texture2D and other texture types are sampled textures
                                        resourceBindings.Add(new ResourceBinding(paramName, ResourceType.SampledTexture, space, index));
                                    }
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

        ShaderBindingLayout shaderBindingLayout = new ShaderBindingLayout(
            new ShaderBindingCounts(samplers, storageTextures, storageBuffers, readWriteStorageTextures, readWriteStorageBuffers),
            shaderUniformSlots);
        return (entryPoint, stage, shaderBindingLayout, threadCountX, threadCountY, threadCountZ);
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
        ShaderStageDto stage, ShaderBindingLayout resources, List<ShaderInstanceDto> shaderInstances, string fileHash,
        uint threadCountX = 0, uint threadCountY = 0, uint threadCountZ = 0)
    {
        FileInfo metadataFile = new FileInfo(Path.Combine(outputDir.FullName, $"{filenameWithoutExt}.metadata.json"));

        using FileStream stream = metadataFile.Create();
        switch (stage)
        {
            case ShaderStageDto.Vertex:
                VertexShaderMetadataDto vertexMetadata = new VertexShaderMetadataDto
                {
                    BindingLayout = resources,
                    Shaders = shaderInstances,
                    SourceHash = fileHash,
                    SlangVersion = SlangVersion
                };
                JsonSerializer.Serialize(stream, vertexMetadata, ShaderMetadataJsonContext.Default.VertexShaderMetadataDto);
                break;
            case ShaderStageDto.Fragment:
                FragmentShaderMetadataDto fragmentMetadata = new FragmentShaderMetadataDto
                {
                    BindingLayout = resources,
                    Shaders = shaderInstances,
                    SourceHash = fileHash,
                    SlangVersion = SlangVersion
                };
                JsonSerializer.Serialize(stream, fragmentMetadata, ShaderMetadataJsonContext.Default.FragmentShaderMetadataDto);
                break;
            case ShaderStageDto.Compute:
                ComputeShaderMetadataDto computeMetadata = new ComputeShaderMetadataDto
                {
                    BindingLayout = resources,
                    Shaders = shaderInstances,
                    SourceHash = fileHash,
                    SlangVersion = SlangVersion,
                    ThreadCountX = threadCountX,
                    ThreadCountY = threadCountY,
                    ThreadCountZ = threadCountZ
                };
                JsonSerializer.Serialize(stream, computeMetadata, ShaderMetadataJsonContext.Default.ComputeShaderMetadataDto);
                break;
            default:
                throw new InvalidOperationException($"Unknown shader stage: {stage}");
        }
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
            ShaderMetadataHeaderDto? metadata = JsonSerializer.Deserialize(json, ShaderMetadataJsonContext.Default.ShaderMetadataHeaderDto);

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
        DirectoryInfo outputDir = new DirectoryInfo(Path.Combine(parentDir.FullName, GeneratedShaderDirectory));

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
            (string entryPoint, ShaderStageDto stage, ShaderBindingLayout bindingLayout, uint threadCountX, uint threadCountY, uint threadCountZ) = ParseReflectionData(reflectionFile);

            // Step 3: Update shader instances with correct entry point
            shaderInstances = shaderInstances.Select(instance =>
                new ShaderInstanceDto(instance.Format, instance.Filename, entryPoint)).ToList();

            // Step 4: Write metadata
            WriteMetadata(outputDir, filenameWithoutExt, stage, bindingLayout, shaderInstances, currentHash, threadCountX, threadCountY, threadCountZ);
        }
        finally
        {
            tempDir.Delete(true);
        }
    }


}
