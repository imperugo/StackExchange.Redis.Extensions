using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.Newtonsoft
{
	/// <summary>
	/// JSon.Net implementation of <see cref="ISerializer"/>
	/// </summary>
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

	    private readonly JsonSerializerSettings settings;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="NewtonsoftSerializer"/> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
	    public NewtonsoftSerializer(JsonSerializerSettings settings = null)
	    {
	        this.settings = settings ?? new JsonSerializerSettings();
	    }

		/// <summary>
		/// Serializes the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		public byte[] Serialize(object item)
		{
			var co = new CachedObject<object>(item);
			var jsonString = JsonConvert.SerializeObject(co, settings);
			return encoding.GetBytes(jsonString);
		}

		/// <summary>
		/// Serializes the asynchronous.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		public async Task<byte[]> SerializeAsync(object item)
		{
			var co = new CachedObject<object>(item);
			var jsonString = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(co, settings));
			return encoding.GetBytes(jsonString);
		}

		/// <summary>
		/// Deserializes the specified serialized object.
		/// </summary>
		/// <param name="serializedObject">The serialized object.</param>
		/// <returns></returns>
		public object Deserialize(byte[] serializedObject)
		{
			var jsonString = encoding.GetString(serializedObject);
			return ((CachedObject<object>)JsonConvert.DeserializeObject(jsonString, typeof(CachedObject<object>))).CachedValue;
		}

		/// <summary>
		/// Deserializes the asynchronous.
		/// </summary>
		/// <param name="serializedObject">The serialized object.</param>
		/// <returns></returns>
		public Task<object> DeserializeAsync(byte[] serializedObject)
		{
			return Task.Factory.StartNew(() => Deserialize(serializedObject));
		}

		/// <summary>
		/// Deserializes the specified serialized object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="serializedObject">The serialized object.</param>
		/// <returns></returns>
		public T Deserialize<T>(byte[] serializedObject)
		{
			var jsonString = encoding.GetString(serializedObject);
			return JsonConvert.DeserializeObject<CachedObject<T>>(jsonString, settings).CachedValue;
		}

		/// <summary>
		/// Deserializes the asynchronous.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="serializedObject">The serialized object.</param>
		/// <returns></returns>
		public Task<T> DeserializeAsync<T>(byte[] serializedObject)
		{
			return Task.Factory.StartNew(() => Deserialize<T>(serializedObject));
		}
	}
}