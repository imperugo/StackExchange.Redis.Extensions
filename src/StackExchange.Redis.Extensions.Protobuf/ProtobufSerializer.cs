using System.IO;
using System.Threading.Tasks;
using ProtoBuf;
using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.Protobuf
{
    public class ProtobufSerializer : ISerializer
    {
        public byte[] Serialize(object item)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, item);
                return ms.ToArray();
            }
        }

        public object Deserialize(byte[] serializedObject)
        {
            return Deserialize<object>(serializedObject);
        }

        public T Deserialize<T>(byte[] serializedObject)
        {
            using (var ms = new MemoryStream(serializedObject))
            {
                return Serializer.Deserialize<T>(ms);
            }
        }
    }
}