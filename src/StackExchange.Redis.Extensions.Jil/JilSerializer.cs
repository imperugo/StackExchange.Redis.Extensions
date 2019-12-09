using System.Text;
using System.Threading.Tasks;
using Jil;
using StackExchange.Redis.Extensions.Core;
using System;

namespace StackExchange.Redis.Extensions.Jil
{
    /// <summary>
    /// Jil implementation of <see cref="ISerializer"/>
    /// </summary>
    public class JilSerializer : ISerializer
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

        /// <summary>
        /// Default constructor for Jil serializer.
        /// </summary>
        /// This constructor uses default serialization options.
        public JilSerializer()
            : this(new Options(prettyPrint: true,
                excludeNulls: false,
                jsonp: false,
                dateFormat:
                DateTimeFormat.ISO8601,
                includeInherited: true,
                unspecifiedDateTimeKindBehavior: UnspecifiedDateTimeKindBehavior.IsLocal))
        {

        }

        /// <summary>
        /// Constructor for Jil serializer.
        /// </summary>
        public JilSerializer(Options options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            JSON.SetDefaultOptions(options);
        }
        /// <summary>
        /// Serializes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public byte[] Serialize(object item)
        {
            var jsonString = JSON.Serialize(item);
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
            return JSON.Deserialize(jsonString, typeof(object));
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
            return JSON.Deserialize<T>(jsonString);
        }

    }
}