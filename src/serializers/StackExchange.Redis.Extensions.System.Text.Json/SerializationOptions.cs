using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

using StackExchange.Redis.Extensions.System.Text.Json.Converters;

namespace StackExchange.Redis.Extensions.System.Text.Json
{
    internal static class SerializationOptions
    {
        static SerializationOptions()
        {
            Flexible = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            Flexible.Converters.Add(new StringToIntCustomConverter());
            Flexible.Converters.Add(new CultureCustomConverter());
            Flexible.Converters.Add(new TimezoneCustomConverter());
            Flexible.Converters.Add(new TimeSpanConverter());
            Flexible.Converters.Add(new TimeSpanNullableConverter());
        }

        public static JsonSerializerOptions Flexible { get; }
    }
}
