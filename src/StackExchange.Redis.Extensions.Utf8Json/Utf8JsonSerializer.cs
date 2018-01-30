using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.Utf8Json
{
    /// <summary>
    /// JSon.Net implementation of <see cref="ISerializer"/>
    /// </summary>
    public class Utf8JsonSerializer : ISerializer
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="Utf8JsonSerializer"/> class.
		/// </summary>
		public Utf8JsonSerializer()
	    {
	    }

	    /// <summary>
		/// Serializes the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		public byte[] Serialize(object item)
        {
			return global::Utf8Json.JsonSerializer.Serialize(item);
        }

        /// <summary>
        /// Serializes the asynchronous.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public Task<byte[]> SerializeAsync(object item)
        {
	        return Task.Factory.StartNew(() => global::Utf8Json.JsonSerializer.Serialize(item));
        }

        /// <summary>
        /// Deserializes the specified serialized object.
        /// </summary>
        /// <param name="serializedObject">The serialized object.</param>
        /// <returns></returns>
        public object Deserialize(byte[] serializedObject)
        {
	        return global::Utf8Json.JsonSerializer.Deserialize<object>(serializedObject);
        }

        /// <summary>
        /// Deserializes the asynchronous.
        /// </summary>
        /// <param name="serializedObject">The serialized object.</param>
        /// <returns></returns>
        public Task<object> DeserializeAsync(byte[] serializedObject)
        {
	        return Task.Factory.StartNew(() =>  global::Utf8Json.JsonSerializer.Deserialize<object>(serializedObject));
        }

        /// <summary>
        /// Deserializes the specified serialized object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializedObject">The serialized object.</param>
        /// <returns></returns>
        public T Deserialize<T>(byte[] serializedObject)
        {
	        return global::Utf8Json.JsonSerializer.Deserialize<T>(serializedObject);
        }

        /// <summary>
        /// Deserializes the asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializedObject">The serialized object.</param>
        /// <returns></returns>
        public Task<T> DeserializeAsync<T>(byte[] serializedObject)
        {
	        return Task.Factory.StartNew(() =>  global::Utf8Json.JsonSerializer.Deserialize<T>(serializedObject));
		}
    }
}