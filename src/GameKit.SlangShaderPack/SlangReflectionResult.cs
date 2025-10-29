using System.Text.Json;
using System.Text.Json.Serialization;
using GameKit.Shaders;

namespace GameKit.SlangShaderPack;

public record SlangReflectionTypeDto(string Kind, string? BaseShape = null);

public record SlangReflectionParameterDto(string Name, SlangReflectionTypeDto Type);

public class ReflectionEntryPointDto
{
    public required string Name { get; init; }
    [JsonPropertyName("stage")]
    public required string Stage { get; init; }
}

public class SlangReflectionDto
{
    public required List<ReflectionEntryPointDto> EntryPoints { get; init; }
    public required List<SlangReflectionParameterDto> Parameters { get; init; }
}

/*
{
   "name": "Texture",
   "binding": {"kind": "descriptorTableSlot", "space": 2, "index": 0},
   "type": {
       "kind": "resource",
       "baseShape": "texture2D"
   }
},
{
   "name": "Sampler",
   "binding": {"kind": "descriptorTableSlot", "space": 2, "index": 0},
   "type": {
       "kind": "samplerState"
   }
}
*/
internal record SlangReflectionInfo(string EntryPointName, ShaderStage Stage, ShaderResources Resources);

internal static class SlangReflectionInfoLoader
{
    private static ShaderStage SlangStageToGameKitShaderStage(string stage)
    {
        return stage switch
        {
            "vertex" => ShaderStage.Vertex,
            "fragment" => ShaderStage.Fragment,
            _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, null)
        };
    }

    private static ShaderResources ProcessShaderResources(SlangReflectionTypeDto reflectionTypeDto, ShaderResources shaderResources)
    {
        string kind = reflectionTypeDto.Kind;
        return kind switch
        {
            "samplerState" => shaderResources with { Samplers = shaderResources.Samplers + 1 },
            // TODO: how to deal with resource?
            "resource" => shaderResources,
            "constantBuffer" => shaderResources with { UniformBuffers = shaderResources.UniformBuffers + 1 },
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }
    
    public static SlangReflectionInfo Load(string filename)
    {
        using Stream stream = File.OpenRead(filename);
        // reflection free deserialization
        SlangReflectionDto? slangReflectionDto = (SlangReflectionDto?)JsonSerializer.Deserialize(stream, SlangReflectionDtoJsonContext.Default.SlangReflectionDto);

        if (slangReflectionDto == null)
        {
            // TODO: improve
            throw new Exception();
        }

        if (slangReflectionDto.EntryPoints.Count != 1)
        {
            throw new Exception("expected one entry point");
        }

        ReflectionEntryPointDto reflectionEntryPointDto = slangReflectionDto.EntryPoints[0];
        ShaderStage stage = SlangStageToGameKitShaderStage(reflectionEntryPointDto.Stage);


        ShaderResources shaderResources = new();
        foreach (var parameterDto in slangReflectionDto.Parameters)
        {
            shaderResources = ProcessShaderResources(parameterDto.Type, shaderResources);
        }
        
        SlangReflectionInfo slangReflectionInfo = new SlangReflectionInfo(reflectionEntryPointDto.Name, stage, shaderResources);

        return slangReflectionInfo;
    }
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(SlangReflectionDto))]
internal partial class SlangReflectionDtoJsonContext: JsonSerializerContext;
