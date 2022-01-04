using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.System.Text.Json;

/// <summary>
/// System.Text.Json implementation of <see cref="ISerializer"/>
/// </summary>
public class SystemTextJsonSerializer : ISerializer
{
    private readonly Dictionary<Type, JsonSerializerContext> serializationContexts = new();
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

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemTextJsonSerializer"/> class.
    /// </summary>
    public SystemTextJsonSerializer(IEnumerable<ICacheSerializationContext> cacheSerializationContexts)
    {
        foreach (var contexts in cacheSerializationContexts)
        {
            foreach (var context in contexts.GetContexts())
                serializationContexts.Add(context.Key, context.Value);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemTextJsonSerializer"/> class.
    /// </summary>
    public SystemTextJsonSerializer(JsonSerializerOptions defaultSerializer, IEnumerable<ICacheSerializationContext> cacheSerializationContexts)
    {
        this.defaultSerializer = defaultSerializer;

        foreach (var contexts in cacheSerializationContexts)
        {
            foreach (var context in contexts.GetContexts())
                serializationContexts.Add(context.Key, context.Value);
        }
    }

    /// <inheritdoc/>
    public T Deserialize<T>(byte[] serializedObject) where T : class
    {
        return JsonSerializer.Deserialize<T>(serializedObject, Options(typeof(T)))!;
    }

    /// <inheritdoc/>
    public byte[] Serialize(object? item)
    {
        if (item == null)
            return Array.Empty<byte>();

        var type = item.GetType();

        return JsonSerializer.SerializeToUtf8Bytes(item, type, Options(type));
    }

    private JsonSerializerOptions Options(Type type)
    {
        return serializationContexts.TryGetValue(type, out var context)
            ? context.Options
            : defaultSerializer;
    }
}
