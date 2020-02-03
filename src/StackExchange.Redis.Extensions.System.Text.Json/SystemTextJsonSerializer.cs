using StackExchange.Redis.Extensions.Core;
using System;
using System.Text.Json;

namespace StackExchange.Redis.Extensions.System.Text.Json
{
    /// <summary>
    /// <see cref="JsonSerializer"/> implementation of <see cref="ISerializer"/> utilizing options from <see cref="SerializationOptions"/>.
    /// </summary>
    public class SystemTextJsonSerializer : ISerializer
    {
        /// <inheritdoc/>
        public object Deserialize(byte[] serializedObject)
        {
            return JsonSerializer.Deserialize<object>(serializedObject, SerializationOptions.Flexible);
        }

        /// <inheritdoc/>
        public T Deserialize<T>(byte[] serializedObject)
        {
            return JsonSerializer.Deserialize<T>(serializedObject, SerializationOptions.Flexible);
        }

        /// <inheritdoc/>
        public byte[] Serialize(object item)
        {
            return JsonSerializer.SerializeToUtf8Bytes(item, SerializationOptions.Flexible);
        }
    }
}
