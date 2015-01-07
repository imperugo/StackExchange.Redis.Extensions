namespace StackExchange.Redis.Extensions
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
		/// <param name="bytes">The bytes.</param>
		/// <returns>The instance of the specified Item</returns>
		object Deserialize(byte[] bytes);

		/// <summary>
		/// Deserializes the specified bytes.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="bytes">The bytes.</param>
		/// <returns>The instance of the specified Item</returns>
		T Deserialize<T>(byte[] bytes) where T : class;
	}
}