using GameKit.Content;
using GameKit.Gpu;
using SDL;

namespace GameKit.Shaders;

public class ShaderLoader: IContentLoader<Shader>
{
    private readonly GpuDevice _gpuDevice;
    private readonly IContentLoader<ShaderMetadata> _shaderMetadataLoader;
    private readonly ShaderFormats _shaderFormats;
    private VirtualFileSystem _virtualFileSystem;

    internal ShaderLoader(GpuDevice gpuDevice, IContentLoader<ShaderMetadata> shaderMetadataLoader, VirtualFileSystem virtualFileSystem)
    {
        _gpuDevice = gpuDevice;
        _shaderMetadataLoader = shaderMetadataLoader;
        _virtualFileSystem = virtualFileSystem;
        _shaderFormats = _gpuDevice.GetSupportedShaderFormats();
    }

    private Shader Load(string directory, ShaderMetadata shaderMetadata)
    {
        foreach (ShaderInstance shaderInstance in shaderMetadata.Shaders)
        {
            if (_shaderFormats.Contains(shaderInstance.Format))
            {
                return CreateShader(directory, shaderInstance, shaderMetadata.Resources, shaderMetadata.Stage);
            }
        }

        // TODO: better exception
        throw new Exception();
    }

    private Shader CreateShader(string directory, ShaderInstance shaderInstance, ShaderResources shaderResources, ShaderStage shaderStage)
    {
        string path = Path.Combine(directory, shaderInstance.Filename);
        VirtualFile file = _virtualFileSystem.GetFile(path);
        using Stream stream = file.Open();
        
        byte[] shaderCode = new byte[stream.Length];

        stream.ReadExactly(shaderCode);
        
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

                Shader shader = new Shader(_gpuDevice, sdlGpuShader, shaderStage, shaderResources.Samplers, shaderResources.StorageTextures, shaderResources.StorageBuffers, shaderResources.UniformBuffers);
                _gpuDevice.RegisterShader(shader);
                return shader;
            }
        }
    }

    public Shader Load(string path)
    {
        string pathWithExtension = Path.Combine(path, "shader.metadata.json");;
        
        ShaderMetadata shaderMetadata = _shaderMetadataLoader.Load(pathWithExtension);

        return Load(path, shaderMetadata);
    }
}
