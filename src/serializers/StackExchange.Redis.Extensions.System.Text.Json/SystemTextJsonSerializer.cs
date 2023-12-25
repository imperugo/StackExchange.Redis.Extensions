// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Text.Json;

using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.System.Text.Json;

/// <summary>
/// System.Text.Json implementation of <see cref="ISerializer"/>
/// </summary>
public class SystemTextJsonSerializer : ISerializer
{
    private readonly JsonSerializerOptions defaultSerializer = SerializationOptions.Flexible;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemTextJsonSerializer"/> class.
    /// </summary>
    public SystemTextJsonSerializer()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemTextJsonSerializer"/> class.
    /// </summary>
    public SystemTextJsonSerializer(JsonSerializerOptions defaultSerializer)
    {
        this.defaultSerializer = defaultSerializer;
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(byte[] serializedObject)
    {
        return JsonSerializer.Deserialize<T>(serializedObject, defaultSerializer);
    }

    /// <inheritdoc/>
    public byte[] Serialize<T>(T? item)
    {
        return item == null
            ? Array.Empty<byte>()
            : JsonSerializer.SerializeToUtf8Bytes<T>(item, defaultSerializer);
    }
}
