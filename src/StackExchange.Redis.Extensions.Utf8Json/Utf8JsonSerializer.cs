using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.Utf8Json
{
    /// <summary>
    /// <see cref="global::Utf8Json.JsonSerializer"/> implementation of <see cref="ISerializer"/>
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
        /// Deserializes the specified serialized object.
        /// </summary>
        /// <param name="serializedObject">The serialized object.</param>
        /// <returns></returns>
        public object Deserialize(byte[] serializedObject)
        {
	        return global::Utf8Json.JsonSerializer.Deserialize<object>(serializedObject);
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
    }
}