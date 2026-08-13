using System.Text.Json;
using GameKit.Content;
using GameKit.ShaderCommon;

namespace GameKit.Shaders;

internal sealed class GraphicsShaderProgramMetadataLoader
{
    private readonly VirtualFileSystem _fileSystem;

    public GraphicsShaderProgramMetadataLoader(VirtualFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public GraphicsShaderProgramMetadata Load(ReadOnlySpan<char> path)
    {
        string json = ReadJson(_fileSystem, path);
        ShaderMetadataHeaderDto header = DeserializeHeader(json, path);
        if (header.Kind != ShaderKindDto.Graphics)
        {
            throw new ArgumentException("Cannot load non-graphics metadata as a graphics shader program.");
        }

        GraphicsShaderProgramMetadataDto? dto = JsonSerializer.Deserialize(
            json,
            ShaderMetadataJsonContext.Default.GraphicsShaderProgramMetadataDto);
        if (dto == null)
        {
            throw new InvalidOperationException(
                $"Failed to deserialize graphics shader program metadata from path: {path.ToString()}");
        }

        return new GraphicsShaderProgramMetadata
        {
            Vertex = new GraphicsShaderStageMetadata
            {
                BindingLayout = dto.Vertex.BindingLayout,
                SystemValueInputs = dto.Vertex.SystemValueInputs,
                Shaders = ConvertShaderInstances(dto.Vertex.Shaders)
            },
            Fragment = new GraphicsShaderStageMetadata
            {
                BindingLayout = dto.Fragment.BindingLayout,
                Shaders = ConvertShaderInstances(dto.Fragment.Shaders)
            }
        };
    }

    private static string ReadJson(VirtualFileSystem fileSystem, ReadOnlySpan<char> path)
    {
        using Stream stream = fileSystem.GetFile(path).Open();
        using StreamReader reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    internal static ShaderMetadataHeaderDto DeserializeHeader(string json, ReadOnlySpan<char> path)
    {
        ShaderMetadataHeaderDto? metadata = JsonSerializer.Deserialize(
            json,
            ShaderMetadataJsonContext.Default.ShaderMetadataHeaderDto);
        if (metadata == null)
        {
            throw new InvalidOperationException(
                $"Failed to deserialize shader metadata from path: {path.ToString()}");
        }

        return metadata;
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
        foreach (ShaderInstanceDto dto in dtos)
        {
            shaders.Add(new ShaderInstance
            {
                Format = ConvertShaderFormat(dto.Format),
                Filename = dto.Filename,
                EntryPoint = dto.EntryPoint
            });
        }

        return shaders;
    }
}
