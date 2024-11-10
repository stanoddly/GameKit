using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameKit.Utilities;

internal static class JsonReaderExtensions
{
    internal static void ValidateJsonTokenType(this ref Utf8JsonReader reader, JsonTokenType jsonTokenType)
    {
        if (reader.TokenType != jsonTokenType)
            throw new JsonException($"Expected {jsonTokenType}, got {reader.TokenType}.");
    }

    internal static bool ValidatedRead(this ref Utf8JsonReader reader, JsonTokenType jsonTokenType)
    {
        var result = reader.Read();
        if (!result)
            throw new JsonException("Failed to read next token");

        reader.ValidateJsonTokenType(jsonTokenType);
        return result;
    }
}

public class Vector2JsonConverter : JsonConverter<Vector2>
{
    public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Vector2 result = default;
        reader.ValidateJsonTokenType(JsonTokenType.StartArray);
        reader.ValidatedRead(JsonTokenType.Number);
        result.X = (float)reader.GetDouble();
        reader.ValidatedRead(JsonTokenType.Number);
        result.Y = (float)reader.GetDouble();
        reader.ValidatedRead(JsonTokenType.EndArray);
        return result;
    }

    public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteEndArray();
    }
}
