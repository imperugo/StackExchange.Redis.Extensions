// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;

using MemoryPack;

using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.MemoryPack;

/// <summary>
/// JSon.Net implementation of <see cref="ISerializer"/>
/// </summary>
public class MemoryPackSerializer: ISerializer
{
    /// <inheritdoc/>
    public byte[] Serialize<T>(T? item)
    {
        return item == null
            ? Array.Empty<byte>()
            : global::MemoryPack.MemoryPackSerializer.Serialize(item);
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(byte[] serializedObject)
    {
        return global::MemoryPack.MemoryPackSerializer.Deserialize<T>(serializedObject);
    }
}
