using System.Text.Json;
using GameKit.Content;
using GameKit.ShaderCommon;

namespace GameKit.Shaders;

public class ShaderMetadataLoader: IContentLoader<ShaderMetadata>
{
    private readonly VirtualFileSystem _fileSystem;

    public ShaderMetadataLoader(VirtualFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public ShaderMetadata Load(string path)
    {
        using Stream stream = _fileSystem.GetFile(path).Open();
        // reflection free deserialization using DTO
        ShaderMetadataDto? dtoMetadata = JsonSerializer.Deserialize(stream, ShaderMetadataJsonContext.Default.ShaderMetadataDto);

        if (dtoMetadata == null)
        {
            throw new InvalidOperationException($"Failed to deserialize shader metadata from path: {path}");
        }

        return ConvertDtoToMetadata(dtoMetadata);
    }

    private ShaderMetadata ConvertDtoToMetadata(ShaderMetadataDto dto)
    {
        var shaders = new List<ShaderInstance>(dto.Shaders.Count);
        foreach (var instanceDto in dto.Shaders)
        {
            shaders.Add(new ShaderInstance
            {
                Format = ConvertShaderFormat(instanceDto.Format),
                Filename = instanceDto.Filename,
                EntryPoint = instanceDto.EntryPoint
            });
        }

        return new ShaderMetadata
        {
            Stage = ConvertShaderStage(dto.Stage),
            BindingLayout = dto.BindingLayout,
            Shaders = shaders
        };
    }

    private static ShaderFormat ConvertShaderFormat(ShaderFormatDto format) => format switch
    {
        ShaderFormatDto.Private => ShaderFormat.Private,
        ShaderFormatDto.SpirV => ShaderFormat.SpirV,
        ShaderFormatDto.Dxbc => ShaderFormat.Dxbc,
        ShaderFormatDto.Dxil => ShaderFormat.Dxil,
        ShaderFormatDto.Msl => ShaderFormat.Msl,
        ShaderFormatDto.MetalLib => ShaderFormat.MetalLib,
        _ => throw new InvalidOperationException($"Unknown shader format: {format}")
    };

    private static ShaderStage ConvertShaderStage(ShaderStageDto stage) => stage switch
    {
        ShaderStageDto.Vertex => ShaderStage.Vertex,
        ShaderStageDto.Fragment => ShaderStage.Fragment,
        _ => throw new InvalidOperationException($"Unknown shader stage: {stage}")
    };
}
