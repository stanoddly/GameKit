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
    
    internal static T ValidatedGet<T>(this ref Utf8JsonReader reader) where T: unmanaged, INumberBase<T>
    {
        if (!reader.Read())
        {
            throw new JsonException("Failed to read next token");
        }

        reader.ValidateJsonTokenType(JsonTokenType.Number);

        // these branches should be optimized by JIT or NativeAOT
        if (typeof(T) == typeof(int))
        {
            return T.CreateChecked(reader.GetInt32());
        }
    
        if (typeof(T) == typeof(short))
        {
            return T.CreateChecked(reader.GetInt16());
        }
    
        if (typeof(T) == typeof(uint))
        {
            return T.CreateChecked(reader.GetUInt32());
        }
    
        if (typeof(T) == typeof(ushort))
        {
            return T.CreateChecked(reader.GetUInt16());
        }
    
        if (typeof(T) == typeof(long))
        {
            return T.CreateChecked(reader.GetInt64());
        }
    
        if (typeof(T) == typeof(ulong))
        {
            return T.CreateChecked(reader.GetUInt64());
        }
    
        if (typeof(T) == typeof(byte))
        {
            return T.CreateChecked(reader.GetByte());
        }
    
        if (typeof(T) == typeof(sbyte))
        {
            return T.CreateChecked(reader.GetSByte());
        }
    
        if (typeof(T) == typeof(float))
        {
            return T.CreateChecked(reader.GetSingle());
        }
    
        if (typeof(T) == typeof(double))
        {
            return T.CreateChecked(reader.GetDouble());
        }
    
        if (typeof(T) == typeof(decimal))
        {
            return T.CreateChecked(reader.GetDecimal());
        }

        throw new JsonException($"The specified numeric type {typeof(T).Name} is not supported");
    }
    
    internal static int ValidatedGetInt32(this ref Utf8JsonReader reader)
    {
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

public class RectangleJsonConverter<TType> : JsonConverter<Rectangle<TType>> where TType : unmanaged, INumberBase<TType>
{
    public override Rectangle<TType> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.ValidateJsonTokenType(JsonTokenType.StartArray);
        TType x = reader.ValidatedGet<TType>();
        TType y = reader.ValidatedGet<TType>();
        TType width = reader.ValidatedGet<TType>();
        TType height = reader.ValidatedGet<TType>();
        reader.ValidatedRead(JsonTokenType.EndArray);

        return new Rectangle<TType>(x, y, width, height);
    }

    public override void Write(Utf8JsonWriter writer, Rectangle<TType> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(double.CreateChecked(value.X));
        writer.WriteNumberValue(double.CreateChecked(value.Y));
        writer.WriteNumberValue(double.CreateChecked(value.Width));
        writer.WriteNumberValue(double.CreateChecked(value.Height));
        writer.WriteEndArray();
    }
}

public class ShortRectangleJsonConverter : JsonConverter<ShortRectangle>
{
    public override ShortRectangle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.ValidateJsonTokenType(JsonTokenType.StartArray);
        short x = reader.ValidatedGet<short>();
        short y = reader.ValidatedGet<short>();
        ushort width = reader.ValidatedGet<ushort>();
        ushort height = reader.ValidatedGet<ushort>();
        reader.ValidatedRead(JsonTokenType.EndArray);

        return new ShortRectangle(x, y, width, height);
    }

    public override void Write(Utf8JsonWriter writer, ShortRectangle value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteNumberValue(value.Width);
        writer.WriteNumberValue(value.Height);
        writer.WriteEndArray();
    }
}
