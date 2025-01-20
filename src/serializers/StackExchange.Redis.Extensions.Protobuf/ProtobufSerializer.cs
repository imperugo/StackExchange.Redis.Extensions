// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.IO;

using ProtoBuf;

using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.Protobuf;

/// <summary>
/// Protobuf-net implementation of <see cref="ISerializer"/>
/// </summary>
public class ProtobufSerializer : ISerializer
{
    /// <inheritdoc/>
    public byte[] Serialize<T>(T? item)
    {
        if (item == null)
            return [];

        using var ms = new MemoryStream();

        Serializer.Serialize(ms, item);

        return ms.ToArray();
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(byte[]? serializedObject)
    {
        if (serializedObject == null)
            return default;

        using var ms = new MemoryStream(serializedObject);

        return Serializer.Deserialize<T>(ms);
    }
}
