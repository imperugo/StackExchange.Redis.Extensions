// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using ServiceStack.Text;

using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.ServiceStack;

/// <summary>
/// ServiceStack.Text implementation of <see cref="ISerializer"/>
/// </summary>
public class ServiceStackJsonSerializer : ISerializer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceStackJsonSerializer"/> class.
    /// </summary>
    public ServiceStackJsonSerializer()
    {
        JsConfig.Init(new Config
        {
            DateHandler = DateHandler.ISO8601,
            AppendUtcOffset = false, // Append "Z" on UTC and "+00:00" on Local times
            TimeSpanHandler = TimeSpanHandler.DurationFormat,
            AssumeUtc = true,
            SkipDateTimeConversion = true,
            IncludeNullValues = false,
            AlwaysUseUtc = true
        });
    }

    /// <inheritdoc/>
    public T Deserialize<T>(byte[] serializedObject)
    {
        var json = JsConfig.UTF8Encoding.GetString(serializedObject);
        return JsonSerializer.DeserializeFromString<T>(json);
    }

    /// <inheritdoc/>
    public byte[] Serialize<T>(T? item)
    {
        if (item == null)
            return [];

        var json = JsonSerializer.SerializeToString(item);
        return JsConfig.UTF8Encoding.GetBytes(json);
    }
}
