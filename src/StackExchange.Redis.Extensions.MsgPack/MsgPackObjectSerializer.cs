using System;
using System.IO;
using MsgPack.Serialization;

namespace StackExchange.Redis.Extensions.MsgPack
{
    public class MsgPackObjectSerializer : IMsgPackSerializer
    {
        public MsgPackObjectSerializer(Action<SerializerRepository> customSerializerRegistrar = null)
        {
            if (customSerializerRegistrar != null)
            {
                customSerializerRegistrar(SerializationContext.Default.Serializers);
            }
        }
        public byte[] Serialize<T>(object item)
        {
            var serializer = MessagePackSerializer.Get<T>();

            using (var byteStream = new MemoryStream())
            {
                serializer.Pack(byteStream, item);
                return byteStream.ToArray();
            }
        }

        public T Deserialize<T>(byte[] bytes)
        {
            var serializer = MessagePackSerializer.Get<T>();
            using (var byteStream = new MemoryStream(bytes as byte[]))
            {
                return serializer.Unpack(byteStream);
            }
        }
    }
}
