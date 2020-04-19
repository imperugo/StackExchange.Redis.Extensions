using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StackExchange.Redis.Extensions.System.Text.Json.Converters
{
    internal class CultureCustomConverter : JsonConverter<CultureInfo>
    {
        public override CultureInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var name = reader.GetString();

            return new CultureInfo(name);
        }

        public override void Write(Utf8JsonWriter writer, CultureInfo value, JsonSerializerOptions options)
        {
            var text = value.Name;

            writer.WriteStringValue(text);
        }
    }
}
