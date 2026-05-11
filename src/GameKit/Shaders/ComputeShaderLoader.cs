using GameKit.Content;
using GameKit.Gpu;
using GameKit.ShaderCommon;

namespace GameKit.Shaders;

public class ComputeShaderLoader : IContentLoader<ComputeShader>
{
    private const string GeneratedShaderDirectory = ".generated";
    private readonly ShaderFormats _shaderFormats;
    private readonly IContentLoader<ShaderMetadata> _shaderMetadataLoader;
    private readonly VirtualFileSystem _virtualFileSystem;

    internal ComputeShaderLoader(GpuDevice gpuDevice, IContentLoader<ShaderMetadata> shaderMetadataLoader, VirtualFileSystem virtualFileSystem)
    {
        _shaderFormats = gpuDevice.GetSupportedShaderFormats();
        _shaderMetadataLoader = shaderMetadataLoader;
        _virtualFileSystem = virtualFileSystem;
    }

    public ComputeShader Load(ReadOnlySpan<char> path)
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
        ShaderMetadata shaderMetadata = _shaderMetadataLoader.Load(metadataFilename);

        if (shaderMetadata.Stage != ShaderStage.Compute)
        {
            throw new ArgumentException($"Shader '{pathString}' is not a compute shader (stage: {shaderMetadata.Stage})");
        }

        foreach (ShaderInstance shaderInstance in shaderMetadata.Shaders)
        {
            if (_shaderFormats.Contains(shaderInstance.Format))
            {
                return CreateComputeShader(generatedDirectoryName, shaderInstance, shaderMetadata);
            }
        }

        throw new NotSupportedException("No compatible shader format found for this GPU.");
    }

    private ComputeShader CreateComputeShader(string directory, ShaderInstance shaderInstance, ShaderMetadata shaderMetadata)
    {
        string filePath = Path.Combine(directory, shaderInstance.Filename);
        VirtualFile file = _virtualFileSystem.GetFile(filePath);
        using Stream stream = file.Open();

        byte[] code = new byte[stream.Length];
        stream.ReadExactly(code);

        return new ComputeShader(
            code,
            shaderInstance.EntryPoint,
            shaderInstance.Format,
            shaderMetadata.BindingLayout,
            shaderMetadata.ThreadCountX,
            shaderMetadata.ThreadCountY,
            shaderMetadata.ThreadCountZ);
    }
}
