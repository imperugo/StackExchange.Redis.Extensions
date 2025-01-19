// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Text.Json;

using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.System.Text.Json;

/// <summary>
/// System.Text.Json implementation of <see cref="ISerializer"/>
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SystemTextJsonSerializer"/> class.
/// </remarks>
public class SystemTextJsonSerializer(JsonSerializerOptions defaultSerializer) : ISerializer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SystemTextJsonSerializer"/> class.
    /// </summary>
    public SystemTextJsonSerializer()
        : this(SerializationOptions.Flexible)
    {
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
            ? []
            : JsonSerializer.SerializeToUtf8Bytes(item, defaultSerializer);
    }
}
