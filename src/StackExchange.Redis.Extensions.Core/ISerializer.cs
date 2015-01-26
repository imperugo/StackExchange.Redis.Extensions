namespace StackExchange.Redis.Extensions.Core
{
	public interface ISerializer
	{
		/// <summary>
		/// Serializes the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		byte[] Serialize(object item);

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
		/// <typeparam name="T"></typeparam>
		/// <param name="serializedObject">The serialized object.</param>
		/// <returns>
		/// The instance of the specified Item
		/// </returns>
		T Deserialize<T>(byte[] serializedObject) where T : class;
	}
}