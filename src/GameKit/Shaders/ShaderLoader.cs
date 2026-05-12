using GameKit.Content;
using GameKit.Gpu;
using GameKit.ShaderCommon;
using SDL;

namespace GameKit.Shaders;

public class ShaderLoader : IShaderLoader
{
    private const string GeneratedShaderDirectory = ".generated";
    private readonly GpuDevice _gpuDevice;
    private readonly GraphicsShaderMetadataLoader _shaderMetadataLoader;
    private readonly ShaderFormats _shaderFormats;
    private VirtualFileSystem _virtualFileSystem;

    internal ShaderLoader(GpuDevice gpuDevice, GraphicsShaderMetadataLoader shaderMetadataLoader, VirtualFileSystem virtualFileSystem)
    {
        _gpuDevice = gpuDevice;
        _shaderMetadataLoader = shaderMetadataLoader;
        _virtualFileSystem = virtualFileSystem;
        _shaderFormats = _gpuDevice.GetSupportedShaderFormats();
    }

    private GraphicsShader Load(string directory, GraphicsShaderMetadata shaderMetadata)
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

    private GraphicsShader CreateShader(string directory, ShaderInstance shaderInstance, ShaderBindingLayout shaderBindingLayout, ShaderStage shaderStage)
    {
        string path = Path.Combine(directory, shaderInstance.Filename);
        VirtualFile file = _virtualFileSystem.GetFile(path);
        using Stream stream = file.Open();
        
        byte[] shaderCode = new byte[stream.Length];

        stream.ReadExactly(shaderCode);
        
        byte[] entryPoint = System.Text.Encoding.UTF8.GetBytes(shaderInstance.EntryPoint + "\0");

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

                GraphicsShader shader = shaderStage switch
                {
                    ShaderStage.Vertex => new VertexShader(_gpuDevice, sdlGpuShader, shaderBindingLayout),
                    ShaderStage.Fragment => new FragmentShader(_gpuDevice, sdlGpuShader, shaderBindingLayout),
                    _ => throw new InvalidOperationException($"Unsupported graphics shader stage: {shaderStage}")
                };
                _gpuDevice.RegisterShader(shader);
                return shader;
            }
        }
    }

    public VertexShader LoadVertexShader(ReadOnlySpan<char> path)
    {
        GraphicsShader shader = Load(path, ShaderStage.Vertex);
        if (shader is not VertexShader vertexShader)
        {
            throw new ArgumentException($"Expected vertex shader but got {shader.Stage}");
        }

        return vertexShader;
    }

    public FragmentShader LoadFragmentShader(ReadOnlySpan<char> path)
    {
        GraphicsShader shader = Load(path, ShaderStage.Fragment);
        if (shader is not FragmentShader fragmentShader)
        {
            throw new ArgumentException($"Expected fragment shader but got {shader.Stage}");
        }

        return fragmentShader;
    }

    private GraphicsShader Load(ReadOnlySpan<char> path, ShaderStage expectedStage)
    {
        string pathString = path.ToString();
        string name = pathString.Split('/')[^1];
        string? directoryName = Path.GetDirectoryName(pathString);

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

        GraphicsShaderMetadata shaderMetadata = _shaderMetadataLoader.Load(metadataFilename);
        if (shaderMetadata.Stage != expectedStage)
        {
            throw new ArgumentException($"Expected {expectedStage} shader metadata but got {shaderMetadata.Stage}");
        }

        return Load(generatedDirectoryName, shaderMetadata);
    }
}
