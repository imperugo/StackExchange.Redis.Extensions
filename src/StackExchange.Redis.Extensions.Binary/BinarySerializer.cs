using StackExchange.Redis.Extensions.Core;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions.Binary
{
    public class BinarySerializer : ISerializer
    {
        private readonly BinaryFormatter binaryFormatter = new BinaryFormatter();

        public object Deserialize(byte[] serializedObject)
        {
            using (var ms = new MemoryStream(serializedObject))
            {
                return binaryFormatter.Deserialize(ms);
            }
        }

        public T Deserialize<T>(byte[] serializedObject)
        {
            using (var ms = new MemoryStream(serializedObject))
            {
                return (T)binaryFormatter.Deserialize(ms);
            }
        }


        public byte[] Serialize(object item)
        {
            using (var ms = new MemoryStream())
            {
                binaryFormatter.Serialize(ms, item);
                return ms.ToArray();
            }
        }
    }
}
