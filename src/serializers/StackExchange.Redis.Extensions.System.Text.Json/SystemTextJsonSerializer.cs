using StackExchange.Redis.Extensions.Core;
using System;
using System.Text.Json;

namespace StackExchange.Redis.Extensions.System.Text.Json
{
    /// <summary>
    /// System.Text.Json implementation of <see cref="ISerializer"/>
    /// </summary>
    public class SystemTextJsonSerializer : ISerializer
    {
        /// <summary>
        /// Deserializes the specified bytes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializedObject">The serialized object.</param>
        /// <returns>
        /// The instance of the specified Item
        /// </returns>
        public T Deserialize<T>(byte[] serializedObject)
        {
            return JsonSerializer.Deserialize<T>(serializedObject, SerializationOptions.Flexible);
        }

        /// <summary>
        /// Serializes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public byte[] Serialize(object item)
        {
            return JsonSerializer.SerializeToUtf8Bytes(item, SerializationOptions.Flexible);
        }
    }
}
