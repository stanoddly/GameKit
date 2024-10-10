using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;
using SDL;

namespace GameKit;


public enum ShaderStage
{
    Vertex = SDL_GPUShaderStage.SDL_GPU_SHADERSTAGE_VERTEX,
    Fragment = SDL_GPUShaderStage.SDL_GPU_SHADERSTAGE_FRAGMENT
}


public enum ShaderFormat: uint
{
    Private = SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_PRIVATE,
    SpirV = SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV,
    Dxbc = SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_DXBC,
    Dxil = SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_DXIL,
    Msl = SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_MSL,
    MetalLib = SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_METALLIB
}

public readonly struct ShaderFormats
{
    public static readonly ShaderFormats BinaryFormats = new ShaderFormats([ShaderFormat.Private, ShaderFormat.SpirV, ShaderFormat.Dxbc, ShaderFormat.Dxil, ShaderFormat.MetalLib]);
    public static readonly ShaderFormats TextFormats = new ShaderFormats([ShaderFormat.Msl]);

    private readonly uint _flags;

    public ShaderFormats(uint flags)
    {
        _flags = flags;
    }
    
    // TODO: params
    public ShaderFormats(Span<ShaderFormat> formats)
    {
        foreach (var format in formats)
        {
            _flags |= (uint)format;
        }
    }

    public static ShaderFormats operator &(ShaderFormats a, ShaderFormat b)
    {
        return new ShaderFormats(a._flags & (uint)b);
    }
    
    public static ShaderFormats operator |(ShaderFormats a, ShaderFormat b)
    {
        return new ShaderFormats(a._flags | (uint)b);
    }

    public bool Contains(ShaderFormat format)
    {
        return (_flags & (uint)format) == (uint)format;
    }
}

public readonly record struct ShaderResources(
    int Samplers = 0,
    int StorageTextures = 0,
    int StorageBuffers = 0,
    int UniformBuffers = 0);

public class ShaderInstance
{
    public required ShaderFormat Format { get; init; }
    public required string Content { get; init; }
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

    public Shader Load(Stream stream)
    {
        // reflection free deserialization
        ShaderPack? shaderMeta = JsonSerializer.Deserialize(stream, ShaderMetaJsonContext.Default.ShaderPack);

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

    private byte[] DecodeContent(ShaderFormat shaderFormat, string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            // TODO:
            throw new Exception();
        }
        
        if (ShaderFormats.BinaryFormats.Contains(shaderFormat))
        {
            return Convert.FromBase64String(content);
        }
        return System.Text.Encoding.UTF8.GetBytes(content);
    }
    
    private Shader CreateShader(ShaderInstance shaderInstance, ShaderResources shaderResources, ShaderStage shaderStage)
    {
        byte[] shaderCode = DecodeContent(shaderInstance.Format, shaderInstance.Content);
        byte[] entryPoint = System.Text.Encoding.UTF8.GetBytes(shaderInstance.EntryPoint);

        unsafe
        {
            fixed (byte* shaderCodePointer = shaderCode)
            fixed (byte* entryPointPointer = entryPoint)
            {
                SDL_GPUShaderCreateInfo sdlGpuShaderCreateInfo = new() {
                    code = shaderCodePointer,
                    code_size = (nuint)shaderCode.Length,
                    entrypoint = entryPointPointer,
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
