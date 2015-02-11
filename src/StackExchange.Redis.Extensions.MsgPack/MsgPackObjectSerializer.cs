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

        public T Deserialize<T>(byte[] serializedObject) where T : class
        {
            if (typeof(T) == typeof(string))
            {
                return serializedObject as T;
            }
            var serializer = MessagePackSerializer.Get<T>();

            using (var byteStream = new MemoryStream(serializedObject))
            {
                return serializer.Unpack(byteStream);
            }
        }

        public byte[] Serialize(object item)
        {
            if (item is string)
            {
                return _encoding.GetBytes(item.ToString());
            }

            var serializer = MessagePackSerializer.Get(item.GetType());

            using (var byteStream = new MemoryStream())
            {
                serializer.Pack(byteStream, item);

                return byteStream.ToArray();
            }
        }

        public object Deserialize(byte[] serializedObject)
        {
            return Deserialize<object>(serializedObject);
        }
    }
}
