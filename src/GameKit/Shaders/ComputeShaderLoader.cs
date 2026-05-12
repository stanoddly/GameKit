using GameKit.Content;
using GameKit.Gpu;
using GameKit.ShaderCommon;

namespace GameKit.Shaders;

public class ComputeShaderLoader : IComputeShaderLoader
{
    private const string GeneratedShaderDirectory = ".generated";
    private readonly ShaderFormats _shaderFormats;
    private readonly ComputeShaderMetadataLoader _shaderMetadataLoader;
    private readonly VirtualFileSystem _virtualFileSystem;

    internal ComputeShaderLoader(GpuDevice gpuDevice, ComputeShaderMetadataLoader shaderMetadataLoader, VirtualFileSystem virtualFileSystem)
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
        ComputeShaderMetadata shaderMetadata = _shaderMetadataLoader.Load(metadataFilename);

        foreach (ShaderInstance shaderInstance in shaderMetadata.Shaders)
        {
            if (_shaderFormats.Contains(shaderInstance.Format))
            {
                return CreateComputeShader(generatedDirectoryName, shaderInstance, shaderMetadata);
            }
        }

        throw new NotSupportedException("No compatible shader format found for this GPU.");
    }

    private ComputeShader CreateComputeShader(string directory, ShaderInstance shaderInstance, ComputeShaderMetadata shaderMetadata)
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
