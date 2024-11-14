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
    
    internal static int ValidatedGetInt32(this ref Utf8JsonReader reader)
    {;
        if (!reader.Read())
        {
            throw new JsonException("Failed to read next token");
        }

        reader.ValidateJsonTokenType(JsonTokenType.Number);

        return reader.GetInt32();
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

public class RectangleJsonConverter : JsonConverter<Rectangle>
{
    public override Rectangle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.ValidateJsonTokenType(JsonTokenType.StartArray);
        int x = reader.ValidatedGetInt32();
        int y = reader.ValidatedGetInt32();
        int width = reader.ValidatedGetInt32();
        int height = reader.ValidatedGetInt32();
        reader.ValidatedRead(JsonTokenType.EndArray);
        
        return new Rectangle(x, y, width, height);
    }

    public override void Write(Utf8JsonWriter writer, Rectangle value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteEndArray();
    }
}
