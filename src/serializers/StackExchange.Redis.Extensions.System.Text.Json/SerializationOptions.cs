using StackExchange.Redis.Extensions.System.Text.Json.Converters;
using System.Text.Encodings.Web;
using System.Text.Json;

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
                IgnoreNullValues = true,
                WriteIndented = false,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            Flexible.Converters.Add(new StringToIntCustomConverter());
            Flexible.Converters.Add(new CultureCustomConverter());
            Flexible.Converters.Add(new TimezoneCustomConverter());
            Flexible.Converters.Add(new TimeSpanConverter());
        }

        public static JsonSerializerOptions Flexible { get; private set; }
    }
}
