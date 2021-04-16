using System.Text.Json;

using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.System.Text.Json
{
    /// <summary>
    /// System.Text.Json implementation of <see cref="ISerializer"/>
    /// </summary>
    public class SystemTextJsonSerializer : ISerializer
    {
        private readonly JsonSerializerOptions serializerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemTextJsonSerializer"/> class.
        /// </summary>
        public SystemTextJsonSerializer()
        {
            this.serializerOptions = SerializationOptions.Flexible;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemTextJsonSerializer"/> class.
        /// </summary>
        public SystemTextJsonSerializer(JsonSerializerOptions serializerOptions)
        {
            this.serializerOptions = serializerOptions;
        }

        /// <inheritdoc/>
        public T Deserialize<T>(byte[] serializedObject)
        {
            return JsonSerializer.Deserialize<T>(serializedObject, serializerOptions);
        }

        /// <inheritdoc/>
        public byte[] Serialize(object item)
        {
            return JsonSerializer.SerializeToUtf8Bytes(item, serializerOptions);
        }
    }
}
