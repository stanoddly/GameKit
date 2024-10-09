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

public class ShaderMeta
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
[JsonSerializable(typeof(ShaderMeta))]
public partial class ShaderMetaJsonContext : JsonSerializerContext;

public class ShaderLoader
{
    private readonly GpuDevice _gpuDevice;
    private readonly IFileSystem _fileSystem;
    private ShaderFormats _shaderFormats;

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

    public Shader Load(Stream stream)
    {
        // reflection free deserialization
        ShaderMeta? shaderMeta = JsonSerializer.Deserialize(stream, ShaderMetaJsonContext.Default.ShaderMeta);

        if (shaderMeta == null)
        {
            // TODO: improve
            throw new Exception();
        }

        return Load(shaderMeta);
    }

    public Shader Load(ShaderMeta shaderMeta)
    {
        foreach (ShaderInstance shaderInstance in shaderMeta.Shaders)
        {
            if (_shaderFormats.Contains(shaderInstance.Format))
            {
                return CreateShader(shaderInstance, shaderMeta.Resources, shaderMeta.Stage);
            }
        }

        // TODO: better exception
        throw new Exception();
    }
    
    private Shader CreateShader(ShaderInstance shaderInstance, ShaderResources shaderResources, ShaderStage shaderStage)
    {
        ShaderFormat shaderFormat = DetermineSupportedShaderFormat();
        _shaderCreateInfo.Format = (SDL_GPUShaderFormat)shaderFormat;

        if (_shaderCreateInfo.Name != null && _shaderCreateInfo.Code != null)
        {
            // TODO: throw something reasonable
            throw new NotImplementedException();
        }

        byte[] shaderCode;
        if (_shaderCreateInfo.Code != null)
        {
            shaderCode = _shaderCreateInfo.Code;
        }
        else if (_shaderCreateInfo.Name != null)
        {
            string filename = _shaderPathResolver.Resolve(_shaderCreateInfo.Stage, shaderFormat, _shaderCreateInfo.Name!);
            shaderCode = File.ReadAllBytes(filename);
        }
        else
        {
            // TODO: throw something reasonable
            throw new NotImplementedException();
        }

        ReadOnlySpan<byte> entryPoint = _shaderCreateInfo.EntryPoint ?? "main"u8;

        SDL_GPUShaderStage sdlGpuShaderStage = (SDL_GPUShaderStage)_shaderCreateInfo.Stage;

        unsafe
        {
            SDL_GPUShaderCreateInfo sdlGpuShaderCreateInfo;
            fixed (byte* contentPointer = shaderCode)
            fixed (byte* entrypoint = entryPoint)
            {
                sdlGpuShaderCreateInfo = new SDL_GPUShaderCreateInfo() {
                    code = contentPointer,
                    code_size = (nuint)shaderCode.Length,
                    entrypoint = entrypoint,
                    format = _shaderCreateInfo.Format,
                    stage = sdlGpuShaderStage,
                    num_samplers = (uint)_shaderCreateInfo.Samplers,
                    num_uniform_buffers = (uint)_shaderCreateInfo.UniformBuffers,
                    num_storage_buffers = (uint)_shaderCreateInfo.StorageBuffers,
                    num_storage_textures = (uint)_shaderCreateInfo.StorageTextures
                };
                
                SDL_GPUShader* sdlGpuShader = SDL3.SDL_CreateGPUShader(_gpuDevice.SdlGpuDevice, &sdlGpuShaderCreateInfo);
                if (sdlGpuShader == null) throw new GameKitInitializationException($"SDL_CreateGPUShader failed: {SDL3.SDL_GetError()}");

                Shader shader = new Shader(sdlGpuShader, _shaderCreateInfo.Stage, _shaderCreateInfo.Samplers, _shaderCreateInfo.StorageTextures, _shaderCreateInfo.StorageBuffers, _shaderCreateInfo.UniformBuffers);
                _shaderCreateInfo = default;

                return shader;
            }
        }
    }
}
