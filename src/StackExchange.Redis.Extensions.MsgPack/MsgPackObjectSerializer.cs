using System;
using System.IO;
using MsgPack.Serialization;
using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.MsgPack
{
    /// <summary>
    /// <see cref="SerializerRepository"/> implementation of <see cref="ISerializer"/>
    /// </summary>
    public class MsgPackObjectSerializer : ISerializer
    {
        private readonly System.Text.Encoding encoding;

        
        /// <summary>
		/// Initializes a new instance of the <see cref="MsgPackObjectSerializer"/> class with the provided setup. 
		/// </summary>
        /// <param name="customSerializerRegistrar">Setup actions to perform on the created <see cref="SerializerRepository"/>.</param>
        /// <param name="encoding">Text encoding to override default <see cref="System.Text.Encoding.UTF8"/></param>
        public MsgPackObjectSerializer(Action<SerializerRepository> customSerializerRegistrar = null, System.Text.Encoding encoding = null)
        {
            customSerializerRegistrar?.Invoke(SerializationContext.Default.Serializers);

            if (encoding == null)
            {
                this.encoding = System.Text.Encoding.UTF8;
            }
        }

        /// <inheritdoc/>
        public T Deserialize<T>(byte[] serializedObject)
        {
            if (typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(encoding.GetString(serializedObject), typeof(T));
            }

            var serializer = MessagePackSerializer.Get<T>();

            using (var byteStream = new MemoryStream(serializedObject))
            {
                return serializer.Unpack(byteStream);
            }
        }

        /// <inheritdoc/>
        public byte[] Serialize(object item)
        {
            if (item is string)
            {
                return encoding.GetBytes(item.ToString());
            }

            var serializer = MessagePackSerializer.Get(item.GetType());

            using (var byteStream = new MemoryStream())
            {
                serializer.Pack(byteStream, item);

                return byteStream.ToArray();
            }
        }

        /// <inheritdoc/>
        public object Deserialize(byte[] serializedObject)
        {
            return Deserialize<object>(serializedObject);
        }
    }
}
