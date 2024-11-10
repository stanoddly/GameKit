using System.Text.Json;
using System.Text.Json.Serialization;
using SDL;

namespace GameKit.Shaders;


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
public partial class ShaderMetaJsonContext: JsonSerializerContext;