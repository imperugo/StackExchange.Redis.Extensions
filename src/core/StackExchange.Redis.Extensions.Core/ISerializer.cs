// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

namespace StackExchange.Redis.Extensions.Core;

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
    byte[] Serialize<T>(T? item);

    /// <summary>
    /// Deserializes the specified bytes.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="serializedObject">The serialized object.</param>
    /// <returns>
    /// The instance of the specified Item
    /// </returns>
    T? Deserialize<T>(byte[]? serializedObject);
}
