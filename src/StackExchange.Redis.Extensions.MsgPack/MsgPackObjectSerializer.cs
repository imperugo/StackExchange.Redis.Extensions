using System;
using System.IO;
using MsgPack.Serialization;
using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.MsgPack
{
    public class MsgPackObjectSerializer : ISerializer
    {
        private readonly System.Text.Encoding _encoding;
        public MsgPackObjectSerializer(Action<SerializerRepository> customSerializerRegistrar = null, System.Text.Encoding encoding = null)
        {
            if (customSerializerRegistrar != null)
            {
                customSerializerRegistrar(SerializationContext.Default.Serializers);
            }

            if (encoding == null)
            {
                _encoding = System.Text.Encoding.UTF8;
            }
        }

        public T Deserialize<T>(string serializedObject) where T : class
        {
            var serializer = MessagePackSerializer.Get<T>();

            using (var byteStream = new MemoryStream(_encoding.GetBytes(serializedObject)))
            {
                return serializer.Unpack(byteStream);
            }
        }

        public string Serialize(object item)
        {
            var serializer = MessagePackSerializer.Get(item.GetType());

            using (var byteStream = new MemoryStream())
            {
                serializer.Pack(byteStream, item);

                return _encoding.GetString(byteStream.ToArray());
            }
        }

        public object Deserialize(string serializedObject)
        {
            return Deserialize<object>(serializedObject);
        }
    }
}
