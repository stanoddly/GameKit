using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;
using SDL;

namespace GameKit;

public readonly record struct ShaderResources(
    int Samplers = 0,
    int StorageTextures = 0,
    int StorageBuffers = 0,
    int UniformBuffers = 0);

public class ShaderInstance
{
    public required ShaderFormat Format { get; init; }
    public string? Path { get; init; }
    public byte[]? Binary { get; init; }
    public required string EntryPoint { get; init; }
}

public class ShaderPack
{
    public required ShaderStage Stage { get; init; }
    public required ShaderResources Resources { get; init; }
    public required List<ShaderInstance> Shaders { get; init; }
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(ShaderPack))]
public partial class ShaderMetaJsonContext : JsonSerializerContext;

public class ShaderLoader
{
    private readonly GpuDevice _gpuDevice;
    private readonly IFileSystem _fileSystem;
    private readonly ShaderFormats _shaderFormats;

    public ShaderLoader(GpuDevice gpuDevice, IFileSystem? fileSystem=null)
    {
        _gpuDevice = gpuDevice;
        _fileSystem = fileSystem ?? new FileSystem();
        _shaderFormats = _gpuDevice.GetSupportedShaderFormats();
    }

    public Shader Load(string path)
    {
        using (Stream stream = _fileSystem.FileStream.New(path, FileMode.Open))
        {
            return Load(stream);
        }
    }

    public Shader Load(Stream stream, string? parentPath = null)
    {
        // reflection free deserialization
        ShaderPack? shaderMeta = JsonSerializer.Deserialize(stream, ShaderMetaJsonContext.Default.ShaderMetadata);

        if (shaderMeta == null)
        {
            // TODO: improve
            throw new Exception();
        }

        return Load(shaderMeta);
    }

    public Shader Load(ShaderPack shaderPack)
    {
        foreach (ShaderInstance shaderInstance in shaderPack.Shaders)
        {
            if (_shaderFormats.Contains(shaderInstance.Format))
            {
                return CreateShader(shaderInstance, shaderPack.Resources, shaderPack.Stage);
            }
        }

        // TODO: better exception
        throw new Exception();
    }
    
    private Shader CreateShader(ShaderInstance shaderInstance, ShaderResources shaderResources, ShaderStage shaderStage)
    {
        byte[] shaderCode;
        if (shaderInstance.Binary != null)
        {
            shaderCode = shaderInstance.Binary;
        }
        else
        {
            // TODO: throw something reasonable
            throw new NotImplementedException();
        }

        byte[] entryPoint = System.Text.Encoding.UTF8.GetBytes(shaderInstance.EntryPoint);

        unsafe
        {
            fixed (byte* contentPointer = shaderCode)
            fixed (byte* entrypoint = entryPoint)
            {
                SDL_GPUShaderCreateInfo sdlGpuShaderCreateInfo = new() {
                    code = contentPointer,
                    code_size = (nuint)shaderCode.Length,
                    entrypoint = entrypoint,
                    format = (SDL_GPUShaderFormat)shaderInstance.Format,
                    stage = (SDL_GPUShaderStage)shaderStage,
                    num_samplers = (uint)shaderResources.Samplers,
                    num_uniform_buffers = (uint)shaderResources.UniformBuffers,
                    num_storage_buffers = (uint)shaderResources.StorageBuffers,
                    num_storage_textures = (uint)shaderResources.StorageTextures
                };
                
                SDL_GPUShader* sdlGpuShader = SDL3.SDL_CreateGPUShader(_gpuDevice.SdlGpuDevice, &sdlGpuShaderCreateInfo);
                if (sdlGpuShader == null) throw new GameKitInitializationException($"SDL_CreateGPUShader failed: {SDL3.SDL_GetError()}");

                Shader shader = new Shader(sdlGpuShader, shaderStage, shaderResources.Samplers, shaderResources.StorageTextures, shaderResources.StorageBuffers, shaderResources.UniformBuffers);
                return shader;
            }
        }
    }
}
