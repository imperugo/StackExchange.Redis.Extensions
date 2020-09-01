using System.IO;

using ProtoBuf;

using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.Protobuf
{
    /// <summary>
    /// Protobuf-net implementation of <see cref="ISerializer"/>
    /// </summary>
    public class ProtobufSerializer : ISerializer
    {
        /// <inheritdoc/>
        public byte[] Serialize(object item)
        {
            using var ms = new MemoryStream();

            Serializer.Serialize(ms, item);

            return ms.ToArray();
        }

        /// <inheritdoc/>
        public T Deserialize<T>(byte[] serializedObject)
        {
            using var ms = new MemoryStream(serializedObject);

            return Serializer.Deserialize<T>(ms);
        }
    }
}
