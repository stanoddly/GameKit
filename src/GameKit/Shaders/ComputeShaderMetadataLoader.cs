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
        ShaderMetadataDto dto = GraphicsShaderMetadataLoader.DeserializeDto(_fileSystem, path);

        if (dto.Stage != ShaderStageDto.Compute)
        {
            throw new ArgumentException($"Expected compute shader but got {dto.Stage}");
        }

        return new ComputeShaderMetadata
        {
            BindingLayout = dto.BindingLayout,
            Shaders = GraphicsShaderMetadataLoader.ConvertShaderInstances(dto.Shaders),
            ThreadCountX = dto.ThreadCountX ?? throw new InvalidOperationException("Compute shader metadata missing ThreadCountX"),
            ThreadCountY = dto.ThreadCountY ?? throw new InvalidOperationException("Compute shader metadata missing ThreadCountY"),
            ThreadCountZ = dto.ThreadCountZ ?? throw new InvalidOperationException("Compute shader metadata missing ThreadCountZ")
        };
    }
}
