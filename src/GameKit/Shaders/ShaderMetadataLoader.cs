using System.Text.Json;
using GameKit.Content;
using GameKit.ShaderCommon;

namespace GameKit.Shaders;

public class GraphicsShaderMetadataLoader : IContentLoader<GraphicsShaderMetadata>
{
    private readonly VirtualFileSystem _fileSystem;

    public GraphicsShaderMetadataLoader(VirtualFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public GraphicsShaderMetadata Load(ReadOnlySpan<char> path)
    {
        ShaderMetadataDto dto = DeserializeDto(_fileSystem, path);
        return ConvertDtoToMetadata(dto);
    }

    internal static ShaderMetadataDto DeserializeDto(VirtualFileSystem fileSystem, ReadOnlySpan<char> path)
    {
        using Stream stream = fileSystem.GetFile(path).Open();
        ShaderMetadataDto? dtoMetadata = JsonSerializer.Deserialize(stream, ShaderMetadataJsonContext.Default.ShaderMetadataDto);
        if (dtoMetadata == null)
        {
            throw new InvalidOperationException($"Failed to deserialize shader metadata from path: {path.ToString()}");
        }
        return dtoMetadata;
    }

    private GraphicsShaderMetadata ConvertDtoToMetadata(ShaderMetadataDto dto)
    {
        if (dto.Stage == ShaderStageDto.Compute)
        {
            throw new ArgumentException("Cannot load compute shader metadata as graphics shader metadata");
        }

        return new GraphicsShaderMetadata
        {
            Stage = ConvertShaderStage(dto.Stage),
            BindingLayout = dto.BindingLayout,
            Shaders = ConvertShaderInstances(dto.Shaders)
        };
    }

    internal static ShaderFormat ConvertShaderFormat(ShaderFormatDto format) => format switch
    {
        ShaderFormatDto.Private => ShaderFormat.Private,
        ShaderFormatDto.SpirV => ShaderFormat.SpirV,
        ShaderFormatDto.Dxbc => ShaderFormat.Dxbc,
        ShaderFormatDto.Dxil => ShaderFormat.Dxil,
        ShaderFormatDto.Msl => ShaderFormat.Msl,
        ShaderFormatDto.MetalLib => ShaderFormat.MetalLib,
        _ => throw new InvalidOperationException($"Unknown shader format: {format}")
    };

    internal static List<ShaderInstance> ConvertShaderInstances(List<ShaderInstanceDto> dtos)
    {
        List<ShaderInstance> shaders = new List<ShaderInstance>(dtos.Count);
        foreach (ShaderInstanceDto instanceDto in dtos)
        {
            shaders.Add(new ShaderInstance
            {
                Format = ConvertShaderFormat(instanceDto.Format),
                Filename = instanceDto.Filename,
                EntryPoint = instanceDto.EntryPoint
            });
        }
        return shaders;
    }

    private static ShaderStage ConvertShaderStage(ShaderStageDto stage) => stage switch
    {
        ShaderStageDto.Vertex => ShaderStage.Vertex,
        ShaderStageDto.Fragment => ShaderStage.Fragment,
        _ => throw new InvalidOperationException($"Unknown shader stage: {stage}")
    };
}
