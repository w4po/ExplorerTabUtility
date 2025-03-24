using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExplorerTabUtility.Helpers;

public class IntPtrConverter : JsonConverter<nint>
{
    public override nint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return new IntPtr(reader.GetInt64());

        throw new JsonException($"Unexpected token type: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, nint value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}