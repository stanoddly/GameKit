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
        const string metadataSuffix = ".metadata.json";

        int lastSlashIndex = path.LastIndexOf('/');

        ReadOnlySpan<char> name;
        ReadOnlySpan<char> directoryName;

        if (lastSlashIndex < 0)
        {
            name = path;
            directoryName = ReadOnlySpan<char>.Empty;
        }
        else
        {
            name = path.Slice(lastSlashIndex + 1);
            directoryName = path.Slice(0, lastSlashIndex);
        }

        // Build compiledDirectoryName: directoryName + "/" + "compiled" (or just "compiled" if no directory)
        int compiledDirLen = directoryName.IsEmpty
            ? CompiledShaderDirectory.Length
            : directoryName.Length + 1 + CompiledShaderDirectory.Length;

        Span<char> compiledDirectoryBuffer = stackalloc char[compiledDirLen];
        int pos = 0;

        if (!directoryName.IsEmpty)
        {
            directoryName.CopyTo(compiledDirectoryBuffer);
            pos = directoryName.Length;
            compiledDirectoryBuffer[pos++] = '/';
        }
        CompiledShaderDirectory.AsSpan().CopyTo(compiledDirectoryBuffer.Slice(pos));

        ReadOnlySpan<char> compiledDirectoryName = compiledDirectoryBuffer;

        // Build metadataFilename: compiledDirectoryName + "/" + name + ".metadata.json"
        int metadataLen = compiledDirLen + 1 + name.Length + metadataSuffix.Length;
        Span<char> metadataBuffer = stackalloc char[metadataLen];

        compiledDirectoryName.CopyTo(metadataBuffer);
        pos = compiledDirLen;
        metadataBuffer[pos++] = '/';
        name.CopyTo(metadataBuffer.Slice(pos));
        pos += name.Length;
        metadataSuffix.AsSpan().CopyTo(metadataBuffer.Slice(pos));

        ReadOnlySpan<char> metadataFilename = metadataBuffer;

        ShaderMetadata shaderMetadata = _shaderMetadataLoader.Load(metadataFilename);

        return Load(compiledDirectoryName.ToString(), shaderMetadata);
    }
}
