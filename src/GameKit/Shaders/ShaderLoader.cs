using GameKit.Content;
using GameKit.Gpu;
using GameKit.ShaderCommon;
using SDL;

namespace GameKit.Shaders;

public class ShaderLoader: IContentLoader<Shader>
{
    private const string CompiledShaderDirectory = "compiled";
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
                return CreateShader(directory, shaderInstance, shaderMetadata.BindingLayout, shaderMetadata.Stage);
            }
        }

        throw new NotSupportedException("No compatible shader format found for this GPU.");
    }

    private Shader CreateShader(string directory, ShaderInstance shaderInstance, ShaderBindingLayout shaderBindingLayout, ShaderStage shaderStage)
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
                    num_samplers = (uint)shaderBindingLayout.NumSamplers(),
                    num_uniform_buffers = (uint)shaderBindingLayout.NumUniformBuffers(),
                    num_storage_buffers = (uint)shaderBindingLayout.NumStorageBuffers(),
                    num_storage_textures = (uint)shaderBindingLayout.NumStorageTextures()
                };

                SDL_GPUShader* sdlGpuShader = SDL3.SDL_CreateGPUShader(_gpuDevice.SdlGpuDevice, &sdlGpuShaderCreateInfo);
                if (sdlGpuShader == null) throw new GameKitInitializationException($"SDL_CreateGPUShader failed: {SDL3.SDL_GetError()}");

                Shader shader = new Shader(_gpuDevice, sdlGpuShader, shaderStage, shaderBindingLayout);
                _gpuDevice.RegisterShader(shader);
                return shader;
            }
        }
    }

    public Shader Load(ReadOnlySpan<char> path)
    {
        string pathString = path.ToString();
        string name = pathString.Split('/')[^1];
        string? directoryName = Path.GetDirectoryName(pathString);

        string compiledDirectoryName;
        if (directoryName == null)
        {
            compiledDirectoryName = CompiledShaderDirectory;
        }
        else
        {
            compiledDirectoryName = Path.Combine(directoryName, CompiledShaderDirectory);
        }

        string metadataFilename = Path.Combine(compiledDirectoryName, $"{name}.metadata.json");

        ShaderMetadata shaderMetadata = _shaderMetadataLoader.Load(metadataFilename);

        return Load(compiledDirectoryName, shaderMetadata);
    }
}
