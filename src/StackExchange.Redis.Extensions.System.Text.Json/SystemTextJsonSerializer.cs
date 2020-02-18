using StackExchange.Redis.Extensions.Core;
using System;
using System.Text.Json;

namespace StackExchange.Redis.Extensions.System.Text.Json
{
    public class SystemTextJsonSerializer : ISerializer
    {
        public object Deserialize(byte[] serializedObject)
        {
            return JsonSerializer.Deserialize<object>(serializedObject, SerializationOptions.Flexible);
        }

        public T Deserialize<T>(byte[] serializedObject)
        {
            return JsonSerializer.Deserialize<T>(serializedObject, SerializationOptions.Flexible);
        }

        public byte[] Serialize(object item)
        {
            return JsonSerializer.SerializeToUtf8Bytes(item, SerializationOptions.Flexible);
        }
    }
}
