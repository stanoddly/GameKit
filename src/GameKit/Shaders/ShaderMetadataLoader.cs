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
        string json = ReadJson(_fileSystem, path);
        ShaderMetadataHeaderDto header = DeserializeHeader(json, path);
        return header.Stage switch
        {
            ShaderStageDto.Vertex => ConvertDtoToMetadata(DeserializeVertexDto(json, path)),
            ShaderStageDto.Fragment => ConvertDtoToMetadata(DeserializeFragmentDto(json, path)),
            ShaderStageDto.Compute => throw new ArgumentException("Cannot load compute shader metadata as graphics shader metadata"),
            _ => throw new InvalidOperationException($"Unknown shader stage: {header.Stage}")
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
        ShaderMetadataHeaderDto? dtoMetadata = JsonSerializer.Deserialize(json, ShaderMetadataJsonContext.Default.ShaderMetadataHeaderDto);
        if (dtoMetadata == null)
        {
            throw new InvalidOperationException($"Failed to deserialize shader metadata from path: {path.ToString()}");
        }
        return dtoMetadata;
    }

    internal static VertexShaderMetadataDto DeserializeVertexDto(string json, ReadOnlySpan<char> path)
    {
        VertexShaderMetadataDto? dtoMetadata = JsonSerializer.Deserialize(json, ShaderMetadataJsonContext.Default.VertexShaderMetadataDto);
        if (dtoMetadata == null)
        {
            throw new InvalidOperationException($"Failed to deserialize vertex shader metadata from path: {path.ToString()}");
        }
        if (dtoMetadata.Stage != ShaderStageDto.Vertex)
        {
            throw new ArgumentException($"Expected vertex shader but got {dtoMetadata.Stage}");
        }

        return dtoMetadata;
    }

    internal static FragmentShaderMetadataDto DeserializeFragmentDto(string json, ReadOnlySpan<char> path)
    {
        FragmentShaderMetadataDto? dtoMetadata = JsonSerializer.Deserialize(json, ShaderMetadataJsonContext.Default.FragmentShaderMetadataDto);
        if (dtoMetadata == null)
        {
            throw new InvalidOperationException($"Failed to deserialize fragment shader metadata from path: {path.ToString()}");
        }
        if (dtoMetadata.Stage != ShaderStageDto.Fragment)
        {
            throw new ArgumentException($"Expected fragment shader but got {dtoMetadata.Stage}");
        }

        return dtoMetadata;
    }

    private GraphicsShaderMetadata ConvertDtoToMetadata(VertexShaderMetadataDto dto)
    {
        return new GraphicsShaderMetadata
        {
            Stage = ShaderStage.Vertex,
            BindingLayout = dto.BindingLayout,
            Shaders = ConvertShaderInstances(dto.Shaders)
        };
    }

    private GraphicsShaderMetadata ConvertDtoToMetadata(FragmentShaderMetadataDto dto)
    {
        return new GraphicsShaderMetadata
        {
            Stage = ShaderStage.Fragment,
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

}
