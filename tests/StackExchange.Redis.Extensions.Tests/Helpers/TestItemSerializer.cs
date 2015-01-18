using Newtonsoft.Json;
using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.Tests.Helpers
{
	public class TestItemSerializer : ISerializer
	{
		public string Serialize(object item)
		{
			return JsonConvert.SerializeObject(item);
		}

		public object Deserialize(string serializedObject)
		{
			return JsonConvert.DeserializeObject(serializedObject, typeof(object));
		}

		public T Deserialize<T>(string serializedObject) where T : class
		{
			return JsonConvert.DeserializeObject<T>(serializedObject);
		}
	}
}