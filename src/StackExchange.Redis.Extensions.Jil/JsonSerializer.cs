using System.Text;
using Jil;
using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.Jil
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
			var jsonString = JSON.Serialize(item);
		    return encoding.GetBytes(jsonString);
		}

		public object Deserialize(byte[] serializedObject)
		{
		    var jsonString = encoding.GetString(serializedObject);
			return JSON.Deserialize(jsonString, typeof (object));
		}

		public T Deserialize<T>(byte[] serializedObject) where T : class
		{
            var jsonString = encoding.GetString(serializedObject);
            return JSON.Deserialize<T>(jsonString);
		}
	}
}