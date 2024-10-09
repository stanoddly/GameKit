using System.IO.Abstractions;
using System.Text;
using SDL;

namespace GameKit;

public enum ShaderStage
{
    Vertex = SDL_GPUShaderStage.SDL_GPU_SHADERSTAGE_VERTEX,
    Fragment = SDL_GPUShaderStage.SDL_GPU_SHADERSTAGE_FRAGMENT
}


public enum ShaderFormat: uint
{
    None = 0,
    Private = SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_PRIVATE,
    SpirV = SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV,
    Dxbc = SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_DXBC,
    Dxil = SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_DXIL,
    Msl = SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_MSL,
    MetalLib = SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_METALLIB
}

public readonly struct ShaderFormats
{
    private readonly uint _flags;

    public ShaderFormats(uint flags)
    {
        _flags = flags;
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


public interface IShaderPathResolver
{
    string Resolve(ShaderStage shaderStage, ShaderFormat shaderFormat, string shaderName);
}

public class SubDirectoryShaderPathResolver: IShaderPathResolver
{
    public string Resolve(ShaderStage shaderStage, ShaderFormat shaderFormat, string shaderName)
    {
        return shaderFormat switch
        {
            ShaderFormat.SpirV => $"shaders/vulkan/{shaderName}.spv",
            ShaderFormat.Dxil or ShaderFormat.Dxbc => $"shaders/directx/{shaderName}.cso",
            ShaderFormat.Msl => $"shaders/metal/{shaderName}.msl",
            ShaderFormat.MetalLib => $"shaders/metal/{shaderName}.metal",
            // TODO: better exception type and message
            _ => throw new NotImplementedException("Shader format not supported"),
        };
    }
}

// TODO: release shaders!
//SDL3.SDL_ReleaseGPUShader(SdlGpuDevice, vertexShader);
//SDL3.SDL_ReleaseGPUShader(SdlGpuDevice, fragmentShader);
public class ShaderBuilder
{
    private record struct ShaderCreateInfo(
        string? Name,
        byte[]? Code,
        byte[]? EntryPoint,
        SDL_GPUShaderFormat Format,
        ShaderStage Stage,
        int Samplers,
        int StorageTextures,
        int StorageBuffers,
        int UniformBuffers);
    
    private readonly GpuDevice _gpuDevice;
    private readonly IShaderPathResolver _shaderPathResolver;
    private ShaderCreateInfo _shaderCreateInfo = default;

    public ShaderBuilder(GpuDevice gpuDevice)
    {
        _gpuDevice = gpuDevice;
        _shaderPathResolver = new SubDirectoryShaderPathResolver();
    }
    
    public ShaderBuilder(GpuDevice gpuDevice, IShaderPathResolver shaderPathResolver)
    {
        _gpuDevice = gpuDevice;
        _shaderPathResolver = shaderPathResolver;
    }

    public ShaderBuilder UseVertexStage()
    {
        _shaderCreateInfo.Stage = ShaderStage.Vertex;
        return this;
    }
    
    public ShaderBuilder UseFragmentStage()  
    {
        _shaderCreateInfo.Stage = ShaderStage.Fragment;
        return this;
    }
    
    public ShaderBuilder FromFileBasedOnFormat(string name)
    {
        _shaderCreateInfo.Name = name;

        return this;
    }

    public ShaderBuilder WithShaderResources(int samplersCount, int uniformBuffers, int storageBuffers,
        int storageTextures)
    {
        _shaderCreateInfo.Samplers = samplersCount;
        _shaderCreateInfo.UniformBuffers = uniformBuffers;
        _shaderCreateInfo.StorageBuffers = storageBuffers;
        _shaderCreateInfo.StorageTextures = storageTextures;
        return this;
    }

    public Shader Build()
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
    
    private ShaderFormat DetermineSupportedShaderFormat()
    {
        SDL_GPUShaderFormat formats;
        unsafe
        {
            formats = SDL3.SDL_GetGPUShaderFormats(_gpuDevice.SdlGpuDevice);
        }
        
        if ((formats & SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV) != 0)
            return ShaderFormat.SpirV;
        if ((formats & SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_DXIL) != 0)
            return ShaderFormat.Dxil;
        if ((formats & SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_DXBC) != 0)
            return ShaderFormat.Dxbc;
        if ((formats & SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_MSL) != 0)
            return ShaderFormat.Msl;

        throw new NotImplementedException("Shader format not supported");
    }
}
