using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameKit.Shaders;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(ShaderPack))]
public partial class ShaderMetaJsonContext: JsonSerializerContext;