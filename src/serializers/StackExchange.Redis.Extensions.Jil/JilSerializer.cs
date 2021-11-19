using System;
using System.Text;

using Jil;

using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.Jil
{
    /// <summary>
    /// Jil implementation of <see cref="ISerializer"/>
    /// </summary>
    public class JilSerializer : ISerializer
    {
        private static readonly Encoding encoding = Encoding.UTF8;

        /// <summary>
        /// Initializes a new instance of the <see cref="JilSerializer"/> class.
        /// </summary>
        public JilSerializer()
            : this(new Options(
                prettyPrint: true,
                excludeNulls: false,
                jsonp: false,
                dateFormat:
                DateTimeFormat.ISO8601,
                includeInherited: true,
                unspecifiedDateTimeKindBehavior: UnspecifiedDateTimeKindBehavior.IsLocal))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JilSerializer"/> class.
        /// </summary>
        public JilSerializer(Options options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            JSON.SetDefaultOptions(options);
        }

        /// <inheritdoc/>
        public byte[] Serialize(object item)
        {
            var jsonString = JSON.Serialize(item);
            return encoding.GetBytes(jsonString);
        }

        /// <inheritdoc/>
        public T Deserialize<T>(byte[] serializedObject)
        {
            if (serializedObject.Length == 0)
                return default(T);

            var jsonString = encoding.GetString(serializedObject);
            return JSON.Deserialize<T>(jsonString);
        }
    }
}
