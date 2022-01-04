using StackExchange.Redis.Extensions.Core;

using Utf8Json;

namespace StackExchange.Redis.Extensions.Utf8Json;

/// <summary>
/// JSon.Net implementation of <see cref="ISerializer"/>
/// </summary>
public class Utf8JsonSerializer : ISerializer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Utf8JsonSerializer"/> class.
    /// </summary>
    public Utf8JsonSerializer()
    {
    }

    /// <inheritdoc/>
    public byte[] Serialize(object item)
    {
        return JsonSerializer.Serialize(item);
    }

    /// <inheritdoc/>
    public T Deserialize<T>(byte[] serializedObject)
    {
        return JsonSerializer.Deserialize<T>(serializedObject);
    }
}
