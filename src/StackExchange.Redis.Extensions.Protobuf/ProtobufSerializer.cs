using System.IO;
using System.Threading.Tasks;
using ProtoBuf;
using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.Protobuf
{
    /// <summary>
    /// <see cref="ProtoBuf.Serializer"/> implementation of <see cref="ISerializer"/>
    /// </summary>
    public class ProtobufSerializer : ISerializer
    {
        /// <inheritdoc/>
        public byte[] Serialize(object item)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, item);
                return ms.ToArray();
            }
        }

        /// <inheritdoc/>
        public object Deserialize(byte[] serializedObject)
        {
            return Deserialize<object>(serializedObject);
        }

        /// <inheritdoc/>
        public T Deserialize<T>(byte[] serializedObject)
        {
            using (var ms = new MemoryStream(serializedObject))
            {
                return Serializer.Deserialize<T>(ms);
            }
        }
    }
}