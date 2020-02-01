using System.IO;
using System.Threading.Tasks;
using ProtoBuf;
using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.Protobuf
{
    /// <summary>
    /// Protobuf-net implementation of <see cref="ISerializer"/>
    /// </summary>
    public class ProtobufSerializer : ISerializer
    {
        /// <summary>
        /// Serializes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public byte[] Serialize(object item)
        {
            using var ms = new MemoryStream();

            Serializer.Serialize(ms, item);

            return ms.ToArray();
        }

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
            using var ms = new MemoryStream(serializedObject);

            return Serializer.Deserialize<T>(ms);
        }
    }
}
