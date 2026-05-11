using GameKit.Content;
using GameKit.Shaders;
using GameKit.ShaderCommon;
using SDL;

namespace GameKit.Gpu;

public class ComputePipelineBuilder
{
    private const string GeneratedShaderDirectory = ".generated";
    private readonly GpuDevice _gpuDevice;
    private readonly IContentLoader<ShaderMetadata> _shaderMetadataLoader;
    private readonly ShaderFormats _shaderFormats;
    private readonly VirtualFileSystem _virtualFileSystem;

    internal ComputePipelineBuilder(GpuDevice gpuDevice, IContentLoader<ShaderMetadata> shaderMetadataLoader, VirtualFileSystem virtualFileSystem)
    {
        _gpuDevice = gpuDevice;
        _shaderMetadataLoader = shaderMetadataLoader;
        _virtualFileSystem = virtualFileSystem;
        _shaderFormats = _gpuDevice.GetSupportedShaderFormats();
    }

    public ComputePipeline Build(string shaderPath)
    {
        string name = shaderPath.Split('/')[^1];
        string? directoryName = Path.GetDirectoryName(shaderPath);

        string generatedDirectoryName;
        if (directoryName == null)
        {
            generatedDirectoryName = GeneratedShaderDirectory;
        }
        else
        {
            generatedDirectoryName = Path.Combine(directoryName, GeneratedShaderDirectory);
        }

        string metadataFilename = Path.Combine(generatedDirectoryName, $"{name}.metadata.json");
        ShaderMetadata shaderMetadata = _shaderMetadataLoader.Load(metadataFilename);

        if (shaderMetadata.Stage != ShaderStage.Compute)
        {
            throw new ArgumentException($"Shader '{shaderPath}' is not a compute shader (stage: {shaderMetadata.Stage})");
        }

        foreach (ShaderInstance shaderInstance in shaderMetadata.Shaders)
        {
            if (_shaderFormats.Contains(shaderInstance.Format))
            {
                return CreateComputePipeline(generatedDirectoryName, shaderInstance, shaderMetadata);
            }
        }

        throw new NotSupportedException("No compatible shader format found for this GPU.");
    }

    private ComputePipeline CreateComputePipeline(string directory, ShaderInstance shaderInstance, ShaderMetadata shaderMetadata)
    {
        string path = Path.Combine(directory, shaderInstance.Filename);
        VirtualFile file = _virtualFileSystem.GetFile(path);
        using Stream stream = file.Open();

        byte[] shaderCode = new byte[stream.Length];
        stream.ReadExactly(shaderCode);

        byte[] entryPoint = System.Text.Encoding.UTF8.GetBytes(shaderInstance.EntryPoint);

        ShaderBindingLayout bindingLayout = shaderMetadata.BindingLayout;

        unsafe
        {
            fixed (byte* shaderCodePointer = shaderCode)
            fixed (byte* entryPointPointer = entryPoint)
            {
                SDL_GPUComputePipelineCreateInfo createInfo = new()
                {
                    code = shaderCodePointer,
                    code_size = (nuint)shaderCode.Length,
                    entrypoint = entryPointPointer,
                    format = (SDL_GPUShaderFormat)shaderInstance.Format,
                    num_samplers = (uint)bindingLayout.NumSamplers(),
                    num_readonly_storage_textures = (uint)bindingLayout.BindingCounts.NumStorageTextures,
                    num_readonly_storage_buffers = (uint)bindingLayout.BindingCounts.NumStorageBuffers,
                    num_readwrite_storage_textures = (uint)bindingLayout.BindingCounts.NumReadWriteStorageTextures,
                    num_readwrite_storage_buffers = (uint)bindingLayout.BindingCounts.NumReadWriteStorageBuffers,
                    num_uniform_buffers = (uint)bindingLayout.NumUniformBuffers(),
                    threadcount_x = shaderMetadata.ThreadCountX,
                    threadcount_y = shaderMetadata.ThreadCountY,
                    threadcount_z = shaderMetadata.ThreadCountZ
                };

                SDL_GPUComputePipeline* pipeline = SDL3.SDL_CreateGPUComputePipeline(_gpuDevice.SdlGpuDevice, &createInfo);
                if (pipeline == null)
                {
                    throw new GameKitInitializationException($"SDL_CreateGPUComputePipeline failed: {SDL3.SDL_GetError()}");
                }

                ComputePipeline computePipeline = new ComputePipeline(_gpuDevice, pipeline, bindingLayout);
                _gpuDevice.RegisterComputePipeline(computePipeline);
                return computePipeline;
            }
        }
    }
}
