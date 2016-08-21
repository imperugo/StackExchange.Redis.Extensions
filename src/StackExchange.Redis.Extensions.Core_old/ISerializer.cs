using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions.Core
{
	/// <summary>
	/// Contract for Serializer implementation
	/// </summary>
	public interface ISerializer
	{
		/// <summary>
		/// Serializes the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		byte[] Serialize(object item);

		/// <summary>
		/// Serializes the asynchronous.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		Task<byte[]> SerializeAsync(object item);

		/// <summary>
		/// Deserializes the specified bytes.
		/// </summary>
		/// <param name="serializedObject">The serialized object.</param>
		/// <returns>
		/// The instance of the specified Item
		/// </returns>
		object Deserialize(byte[] serializedObject);

		/// <summary>
		/// Deserializes the specified bytes.
		/// </summary>
		/// <param name="serializedObject">The serialized object.</param>
		/// <returns>
		/// The instance of the specified Item
		/// </returns>
		Task<object> DeserializeAsync(byte[] serializedObject);

		/// <summary>
		/// Deserializes the specified bytes.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="serializedObject">The serialized object.</param>
		/// <returns>
		/// The instance of the specified Item
		/// </returns>
		T Deserialize<T>(byte[] serializedObject);

		/// <summary>
		/// Deserializes the specified bytes.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="serializedObject">The serialized object.</param>
		/// <returns>
		/// The instance of the specified Item
		/// </returns>
		Task<T> DeserializeAsync<T>(byte[] serializedObject);
	}
}