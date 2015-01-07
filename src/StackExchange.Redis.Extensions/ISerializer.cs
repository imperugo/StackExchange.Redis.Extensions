namespace StackExchange.Redis.Extensions
{
	public interface ISerializer
	{
		byte[] Serialize(object item);
		object Deserialize(byte[] bytes);
		T Deserialize<T>(byte[] bytes) where T : class;
	}
}