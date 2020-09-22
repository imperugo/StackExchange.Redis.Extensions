using System;
using System.Text;
using System.Threading.Tasks;

using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.Utf8Json
{
    /// <summary>
    /// JSon.Net implementation of <see cref="ISerializer"/>
    /// </summary>
    public class Utf8JsonSerializer : ISerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Utf8JsonSerializer"/> class.
        /// </summary>
        public Utf8JsonSerializer()
        {
        }

        /// <inheritdoc/>
        public byte[] Serialize(object item)
        {
            return global::Utf8Json.JsonSerializer.Serialize(item);
        }

        /// <inheritdoc/>
        public T Deserialize<T>(byte[] serializedObject)
        {
            return global::Utf8Json.JsonSerializer.Deserialize<T>(serializedObject);
        }

        /// <inheritdoc/>
        public object Deserialize(byte[] serializedObject, Type returnType)
        {
            return global::Utf8Json.JsonSerializer.NonGeneric.Deserialize(returnType, serializedObject);
        }
    }
}
