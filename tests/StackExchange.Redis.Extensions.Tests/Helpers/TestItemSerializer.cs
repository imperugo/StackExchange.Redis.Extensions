using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace StackExchange.Redis.Extensions.Tests.Helpers
{
	public class TestItemSerializer : ISerializer
	{
		public byte[] Serialize(object item)
		{
			var formatter = new BinaryFormatter();

			byte[] itemBytes;

			using (var ms = new MemoryStream())
			{
				formatter.Serialize(ms, item);
				itemBytes = ms.ToArray();
			}

			return itemBytes;
		}

		public object Deserialize(byte[] bytes)
		{
			var formatter = new BinaryFormatter();

			object item;
			using (var ms = new MemoryStream(bytes))
			{
				item = formatter.Deserialize(ms);
			}

			return item;
		}

		public T Deserialize<T>(byte[] bytes) where T : class
		{
			return Deserialize(bytes) as T;
		}
	}
}