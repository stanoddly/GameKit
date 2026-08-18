using System.Text.Json;
using System.Text.Json.Serialization;
using Pixely.Utilities;

namespace Pixely.Sprites;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    Converters = [typeof(ShortRectangleJsonConverter), typeof(Vector2JsonConverter), typeof(JsonStringEnumConverter<SpriteFlip>)])]
[JsonSerializable(typeof(SpriteDto))]
[JsonSerializable(typeof(AnimatedSpriteDto))]
internal partial class SpriteDtosJsonContext : JsonSerializerContext;
