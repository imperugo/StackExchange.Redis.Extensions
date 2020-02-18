using System;
using System.Buffers;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StackExchange.Redis.Extensions.System.Text.Json.Converters
{
    internal class StringToIntCustomConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;

                if (Utf8Parser.TryParse(span, out int number, out var bytesConsumed) && span.Length == bytesConsumed)
                    return number;

                if (int.TryParse(reader.GetString(), out number))
                    return number;

                return null;
            }

            return reader.GetInt32();
        }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
                return;
            }

            writer.WriteNullValue();
        }
    }
}
