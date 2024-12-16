using GameKit.Content;
using GameKit.Gpu;
using SDL;

namespace GameKit.Shaders;

public class ShaderLoader: IContentLoader<Shader>
{
    private readonly GpuDevice _gpuDevice;
    private readonly IContentLoader<ShaderPack> _shaderPackLoader;
    private readonly ShaderFormats _shaderFormats;

    public ShaderLoader(GpuDevice gpuDevice, IContentLoader<ShaderPack> shaderPackLoader)
    {
        _gpuDevice = gpuDevice;
        _shaderPackLoader = shaderPackLoader;
        _shaderFormats = _gpuDevice.GetSupportedShaderFormats();
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

    public Shader Load(string path)
    {
        ShaderPack shaderPack = _shaderPackLoader.Load(path);
        return Load(shaderPack);
    }
}
