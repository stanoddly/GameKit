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
    
    internal static float GetFloat(this ref Utf8JsonReader reader)
    {
        var value = reader.GetDouble();
        if (value < float.MinValue || value > float.MaxValue)
            throw new JsonException($"X value {value} is outside the valid range for float ({float.MinValue}, {float.MaxValue})");

        return (float)value;
    }
}

public class Vector2JsonConverter : JsonConverter<Vector2>
{
    public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Vector2 result = default;
        reader.ValidateJsonTokenType(JsonTokenType.StartArray);
        reader.ValidatedRead(JsonTokenType.Number);
        result.X = reader.GetFloat();
        reader.ValidatedRead(JsonTokenType.Number);
        result.Y = reader.GetFloat();
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
