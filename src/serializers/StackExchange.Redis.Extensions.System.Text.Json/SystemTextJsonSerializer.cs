using System;
using System.Text.Json;

using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.System.Text.Json
{
    /// <summary>
    /// System.Text.Json implementation of <see cref="ISerializer"/>
    /// </summary>
    public class SystemTextJsonSerializer : ISerializer
    {
        /// <inheritdoc/>
        public T Deserialize<T>(byte[] serializedObject)
        {
            return JsonSerializer.Deserialize<T>(serializedObject, SerializationOptions.Flexible);
        }

        /// <inheritdoc/>
        public object Deserialize(byte[] serializedObject, Type returnType)
        {
            return JsonSerializer.Deserialize(serializedObject, returnType, SerializationOptions.Flexible);
        }

        /// <inheritdoc/>
        public byte[] Serialize(object item)
        {
            return JsonSerializer.SerializeToUtf8Bytes(item, SerializationOptions.Flexible);
        }
    }
}
