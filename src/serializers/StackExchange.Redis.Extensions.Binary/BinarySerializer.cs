using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.Binary
{
    /// <summary>
    /// Binary implementation of <see cref="ISerializer"/>
    /// </summary>
    public class BinarySerializer : ISerializer
    {
        private readonly BinaryFormatter binaryFormatter = new BinaryFormatter();

        /// <inheritdoc/>
        public T Deserialize<T>(byte[] serializedObject)
        {
            using var ms = new MemoryStream(serializedObject);

            return (T)binaryFormatter.Deserialize(ms);
        }

        /// <inheritdoc/>
        public byte[] Serialize(object item)
        {
            using var ms = new MemoryStream();

            binaryFormatter.Serialize(ms, item);
            return ms.ToArray();
        }
    }
}
