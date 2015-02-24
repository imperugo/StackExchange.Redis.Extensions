using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.Newtonsoft
{
	public class NewtonsoftSerializer : ISerializer
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

		public async Task<byte[]> SerializeAsync(object item)
		{
			var jsonString = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(item));
			return encoding.GetBytes(jsonString);
		}

		public object Deserialize(byte[] serializedObject)
		{
			var jsonString = encoding.GetString(serializedObject);
			return JsonConvert.DeserializeObject(jsonString, typeof(object));
		}

		public Task<object> DeserializeAsync(byte[] serializedObject)
		{
			return Task.Factory.StartNew(() => Deserialize(serializedObject));
		}

		public T Deserialize<T>(byte[] serializedObject) where T : class
		{
			var jsonString = encoding.GetString(serializedObject);
			return JsonConvert.DeserializeObject<T>(jsonString);
		}

		public Task<T> DeserializeAsync<T>(byte[] serializedObject) where T : class
		{
			return Task.Factory.StartNew(() => Deserialize<T>(serializedObject));
		}
	}
}