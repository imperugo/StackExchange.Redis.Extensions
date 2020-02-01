using StackExchange.Redis.Extensions.Core;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions.Binary
{
    /// <summary>
    /// Binary implementation of <see cref="ISerializer"/>
    /// </summary>
    public class BinarySerializer : ISerializer
    {
        private readonly BinaryFormatter binaryFormatter = new BinaryFormatter();

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

            return (T)binaryFormatter.Deserialize(ms);
        }

        /// <summary>
        /// Serializes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>

        public byte[] Serialize(object item)
        {
            using var ms = new MemoryStream();

            binaryFormatter.Serialize(ms, item);
            return ms.ToArray();
        }
    }
}
