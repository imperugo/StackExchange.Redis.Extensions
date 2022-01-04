using System;

using StackExchange.Redis.Extensions.Core;

using Utf8Json;

namespace StackExchange.Redis.Extensions.Utf8Json;

/// <summary>
/// JSon.Net implementation of <see cref="ISerializer"/>
/// </summary>
public class Utf8JsonSerializer : ISerializer
{
    /// <inheritdoc/>
    public byte[] Serialize(object? item)
    {
        return item == null
            ? Array.Empty<byte>()
            : JsonSerializer.Serialize(item);
    }

    /// <inheritdoc/>
    public T Deserialize<T>(byte[] serializedObject) where T : class
    {
        return JsonSerializer.Deserialize<T>(serializedObject);
    }
}
