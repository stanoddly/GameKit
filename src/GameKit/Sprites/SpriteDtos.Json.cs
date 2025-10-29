using System.Text.Json;
using System.Text.Json.Serialization;
using GameKit.Utilities;

namespace GameKit.Sprites;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    Converters = [typeof(ShortRectangleJsonConverter)])]
[JsonSerializable(typeof(SpriteDto))]
[JsonSerializable(typeof(AnimatedSpriteDto))]
internal partial class SpriteDtosJsonContext : JsonSerializerContext;