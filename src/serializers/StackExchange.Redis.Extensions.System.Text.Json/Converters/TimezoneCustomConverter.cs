using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StackExchange.Redis.Extensions.System.Text.Json.Converters
{
    internal class TimezoneCustomConverter : JsonConverter<TimeZoneInfo>
    {
        public override TimeZoneInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var name = reader.GetString();

            return TimeZoneInfo.FindSystemTimeZoneById(name);
        }

        public override void Write(Utf8JsonWriter writer, TimeZoneInfo value, JsonSerializerOptions options)
        {
            var text = value.Id;

            writer.WriteStringValue(text);
        }
    }
}
