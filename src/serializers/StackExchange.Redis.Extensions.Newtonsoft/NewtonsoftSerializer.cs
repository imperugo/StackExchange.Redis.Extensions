// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Text;

using Newtonsoft.Json;

using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.Newtonsoft;

/// <summary>
/// JSon.Net implementation of <see cref="ISerializer"/>
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="NewtonsoftSerializer"/> class.
/// </remarks>
/// <param name="settings">The settings.</param>
public class NewtonsoftSerializer(JsonSerializerSettings? settings) : ISerializer
{
    /// <summary>
    /// Encoding to use to convert string to byte[] and the other way around.
    /// </summary>
    /// <remarks>
    /// StackExchange.Redis uses Encoding.UTF8 to convert strings to bytes,
    /// hence we do same here.
    /// </remarks>
    private static readonly Encoding encoding = Encoding.UTF8;

    private readonly JsonSerializerSettings settings = settings ?? new();

    /// <summary>
    /// Initializes a new instance of the <see cref="NewtonsoftSerializer"/> class.
    /// </summary>
    public NewtonsoftSerializer()
        : this(null)
    {
    }

    /// <inheritdoc/>
    public byte[] Serialize<T>(T? item)
    {
        if (item == null)
            return [];

        var type = item.GetType();
        var jsonString = JsonConvert.SerializeObject(item, type, settings);
        return encoding.GetBytes(jsonString);
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(byte[]? serializedObject)
    {
        if (serializedObject == null)
            return default;

        var jsonString = encoding.GetString(serializedObject);
        return JsonConvert.DeserializeObject<T>(jsonString, settings);
    }
}
