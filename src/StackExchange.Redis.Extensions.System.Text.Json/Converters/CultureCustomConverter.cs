using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StackExchange.Redis.Extensions.System.Text.Json.Converters
{
        /// <inheritdoc/>
    public class CultureCustomConverter : JsonConverter<CultureInfo>
    {
        /// <inheritdoc/>
        public override CultureInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var name = reader.GetString();

            return new CultureInfo(name);
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, CultureInfo value, JsonSerializerOptions options)
        {
            var text = value.Name;

            writer.WriteStringValue(text);
        }
    }
}
