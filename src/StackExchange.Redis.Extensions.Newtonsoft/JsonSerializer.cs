using System.Text;
using Newtonsoft.Json;
using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.Newtonsoft
{
	public class JsonSerializer : ISerializer
	{
		// TODO: May make this configurable in the future.
		/// <summary>
		/// Encoding to use to convert string to byte[] and the other way around.
		/// </summary>
		/// <remarks>
		/// StackExchange.Redis uses Encoding.UTF8 to convert strings to bytes,
		/// hence we do same here.
		/// </remarks>
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