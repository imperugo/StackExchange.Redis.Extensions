using System;
using System.Text;
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
		public NewtonsoftSerializer() : this(null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NewtonsoftSerializer"/> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public NewtonsoftSerializer(JsonSerializerSettings settings)
        {
            this.settings = settings ?? new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };
        }

        /// <summary>
        /// Serializes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public byte[] Serialize(object item)
        {
            var type = item?.GetType();
            
            if (type == typeof(string))
            {
                return encoding.GetBytes((string)item);
            }

            var jsonString = JsonConvert.SerializeObject(item, type, settings);
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
            return JsonConvert.DeserializeObject(jsonString, typeof(object));
        }


        /// <summary>
        /// Deserializes the specified serialized object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializedObject">The serialized object.</param>
        /// <returns></returns>
        public T Deserialize<T>(byte[] serializedObject)
        {
            string jsonString = encoding.GetString(serializedObject);
            if (typeof(T) == typeof(string))
            {
                return (T)((object)jsonString);
            }
            return JsonConvert.DeserializeObject<T>(jsonString, settings);
        }

        public object Deserialize(byte[] serializedObject, Type t)
        {
            var jsonString = encoding.GetString(serializedObject);
            if (t == typeof(string))
            {
                return jsonString;
            }
            return JsonConvert.DeserializeObject(jsonString, t, settings);
        }
    }
}
