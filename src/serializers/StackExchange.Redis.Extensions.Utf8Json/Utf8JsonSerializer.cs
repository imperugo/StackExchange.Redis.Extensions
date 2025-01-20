// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using StackExchange.Redis.Extensions.Core;

using Utf8Json;

namespace StackExchange.Redis.Extensions.Utf8Json;

/// <summary>
/// JSon.Net implementation of <see cref="ISerializer"/>
/// </summary>
public class Utf8JsonSerializer : ISerializer
{
    /// <inheritdoc/>
    public byte[] Serialize<T>(T? item)
    {
        return item == null
            ? []
            : JsonSerializer.Serialize(item);
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(byte[]? serializedObject)
    {
        return JsonSerializer.Deserialize<T?>(serializedObject);
    }
}
