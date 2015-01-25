using System.Text;
using Newtonsoft.Json;
using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.Tests.Helpers
{
	public class TestItemSerializer : ISerializer
	{
        private static readonly Encoding encoding = Encoding.UTF8;

        public byte[] Serialize(object item)
        {
            var jsonString = JsonConvert.SerializeObject(item);
            return encoding.GetBytes(jsonString);
        }

        public object Deserialize(byte[] serializedObject)
        {
            var jsonString = encoding.GetString(serializedObject);
            return JsonConvert.DeserializeObject(jsonString, typeof(object));
        }

        public T Deserialize<T>(byte[] serializedObject) where T : class
        {
            var jsonString = encoding.GetString(serializedObject);
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
	}
}