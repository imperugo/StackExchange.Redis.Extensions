using System;

namespace StackExchange.Redis.Extensions.Core
{
    /// <summary>
    /// Contract for Serializer implementation
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Serializes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>Return the serialized object</returns>
        byte[] Serialize(object item);

        /// <summary>
        /// Deserializes the specified bytes.
        /// </summary>
        /// <typeparam name="T">The type of the expected object.</typeparam>
        /// <param name="serializedObject">The serialized object.</param>
        /// <returns>
        /// The instance of the specified Item
        /// </returns>
        T Deserialize<T>(byte[] serializedObject);

        /// <summary>
        /// Deserializes the specified bytes.
        /// </summary>
        /// <param name="serializedObject">The serialized object.</param>
        /// <param name="returnType">The type of the object to convert to and return.</param>
        /// <returns>
        /// The instance of the specified Item
        /// </returns>
        object Deserialize(byte[] serializedObject, Type returnType);
    }
}
