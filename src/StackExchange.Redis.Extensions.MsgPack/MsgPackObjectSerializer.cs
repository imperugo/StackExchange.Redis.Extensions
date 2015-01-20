using System;
using System.IO;
using MsgPack.Serialization;
using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.MsgPack
{
    public class MsgPackObjectSerializer : ISerializer
    {
        public T DeserializeFromByteArray<T>(byte[] raw)
        {
            using (var stream = new MemoryStream(raw))
            {
                MessagePackSerializer<T> packer = MessagePackSerializer.Get<T>();
                T unpack = packer.Unpack(stream);
                return unpack;
            }
        }

        public string Serialize(object item)
        {
            throw new NotImplementedException();
        }

        public object Deserialize(string serializedObject)
        {
            throw new NotImplementedException();
        }

        public T Deserialize<T>(string content) where T : class
        {
            throw new NotImplementedException();
        }

        public string Serialize<T>(T objectTree)
        {
            throw new NotImplementedException();
        }

        public byte[] SerializeToByteArray<T>(T objectTree)
        {
            using (var stream = new MemoryStream())
            {
                MessagePackSerializer<T> packer = MessagePackSerializer.Get<T>();
                packer.Pack(stream, objectTree);
                return stream.ToArray();
            }
        }
    }
}
