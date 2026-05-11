using System.Text.Json;
using GameKit.Content;
using GameKit.ShaderCommon;

namespace GameKit.Shaders;

public class ComputeShaderMetadataLoader : IContentLoader<ComputeShaderMetadata>
{
    private readonly VirtualFileSystem _fileSystem;

    public ComputeShaderMetadataLoader(VirtualFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public ComputeShaderMetadata Load(ReadOnlySpan<char> path)
    {
        using Stream stream = _fileSystem.GetFile(path).Open();
        ComputeShaderMetadataDto? dto = JsonSerializer.Deserialize(stream, ShaderMetadataJsonContext.Default.ComputeShaderMetadataDto);

        if (dto == null)
        {
            throw new InvalidOperationException($"Failed to deserialize compute shader metadata from path: {path.ToString()}");
        }
        if (dto.Stage != ShaderStageDto.Compute)
        {
            throw new ArgumentException($"Expected compute shader but got {dto.Stage}");
        }

        return new ComputeShaderMetadata
        {
            BindingLayout = dto.BindingLayout,
            Shaders = GraphicsShaderMetadataLoader.ConvertShaderInstances(dto.Shaders),
            ThreadCountX = dto.ThreadCountX,
            ThreadCountY = dto.ThreadCountY,
            ThreadCountZ = dto.ThreadCountZ
        };
    }
}
