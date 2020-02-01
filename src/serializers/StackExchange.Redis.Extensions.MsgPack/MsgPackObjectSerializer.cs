using System;
using System.IO;
using MsgPack.Serialization;
using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.MsgPack
{
    /// <summary>
    /// MsgPac implementation of <see cref="ISerializer"/>
    /// </summary>
    public class MsgPackObjectSerializer : ISerializer
    {
        private readonly System.Text.Encoding encoding;

        /// <summary>
        /// Create an insta of <see cref="MsgPackObjectSerializer" />.
        /// </summary>
        /// <param name="customSerializerRegistrar"></param>
        /// <param name="encoding"></param>
        public MsgPackObjectSerializer(Action<SerializerRepository> customSerializerRegistrar = null, System.Text.Encoding encoding = null)
        {
            customSerializerRegistrar?.Invoke(SerializationContext.Default.Serializers);

            if (encoding == null)
            {
                this.encoding = System.Text.Encoding.UTF8;
            }
        }

        /// <summary>
        /// Deserializes the specified bytes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializedObject">The serialized object.</param>
        /// <returns>
        /// The instance of the specified Item
        /// </returns>

        public T Deserialize<T>(byte[] serializedObject)
        {
            if (typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(encoding.GetString(serializedObject), typeof(T));
            }

            var serializer = MessagePackSerializer.Get<T>();

            using var byteStream = new MemoryStream(serializedObject);

            return serializer.Unpack(byteStream);
        }

        /// <summary>
        /// Serializes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public byte[] Serialize(object item)
        {
            if (item is string)
            {
                return encoding.GetBytes(item.ToString());
            }

            var serializer = MessagePackSerializer.Get(item.GetType());

            using var byteStream = new MemoryStream();
            serializer.Pack(byteStream, item);

            return byteStream.ToArray();
        }
    }
}
