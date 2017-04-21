using StackExchange.Redis.Extensions.Core;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions.Binary
{
    public class BinarySerializer : ISerializer
    {
        private readonly BinaryFormatter _binaryFormatter = new BinaryFormatter();

        public object Deserialize(byte[] serializedObject)
        {
            using (var ms = new MemoryStream(serializedObject))
            {
                return _binaryFormatter.Deserialize(ms);
            }
        }

        public T Deserialize<T>(byte[] serializedObject)
        {
            using (var ms = new MemoryStream(serializedObject))
            {
                return (T)_binaryFormatter.Deserialize(ms);
            }
        }

        public Task<object> DeserializeAsync(byte[] serializedObject)
        {
            return Task.Factory.StartNew(() => Deserialize(serializedObject));
        }

        public Task<T> DeserializeAsync<T>(byte[] serializedObject)
        {
            return Task.Factory.StartNew(() => Deserialize<T>(serializedObject));
        }

        public byte[] Serialize(object item)
        {
            using (var ms = new MemoryStream())
            {
                _binaryFormatter.Serialize(ms, item);
                return ms.ToArray();
            }
        }

        public Task<byte[]> SerializeAsync(object item)
        {
            return Task.Factory.StartNew(() => Serialize(item));
        }
    }
}
