using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameKit.ShaderCommon;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(ShaderMetadataHeaderDto))]
[JsonSerializable(typeof(GraphicsShaderProgramMetadataDto))]
[JsonSerializable(typeof(GraphicsVertexShaderStageMetadataDto))]
[JsonSerializable(typeof(GraphicsShaderStageMetadataDto))]
[JsonSerializable(typeof(VertexShaderMetadataDto))]
[JsonSerializable(typeof(FragmentShaderMetadataDto))]
[JsonSerializable(typeof(ComputeShaderMetadataDto))]
[JsonSerializable(typeof(ShaderStageDto))]
[JsonSerializable(typeof(ShaderKindDto))]
[JsonSerializable(typeof(ShaderFormatDto))]
[JsonSerializable(typeof(ShaderInstanceDto))]
[JsonSerializable(typeof(ShaderSystemValueInputs))]
[JsonSerializable(typeof(StorageBufferElementSizes))]
public partial class ShaderMetadataJsonContext: JsonSerializerContext;
