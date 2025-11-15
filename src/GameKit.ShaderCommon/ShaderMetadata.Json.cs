using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameKit.ShaderCommon;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(ShaderMetadataDto))]
[JsonSerializable(typeof(ShaderStageDto))]
[JsonSerializable(typeof(ShaderFormatDto))]
[JsonSerializable(typeof(ShaderInstanceDto))]
public partial class ShaderMetadataJsonContext: JsonSerializerContext;
