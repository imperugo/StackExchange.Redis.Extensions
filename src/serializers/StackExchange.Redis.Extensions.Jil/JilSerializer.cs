// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Text;

using Jil;

using StackExchange.Redis.Extensions.Core;

namespace StackExchange.Redis.Extensions.Jil;

/// <summary>
/// Jil implementation of <see cref="ISerializer"/>
/// </summary>
public class JilSerializer : ISerializer
{
    private static readonly Encoding encoding = Encoding.UTF8;

    /// <summary>
    /// Initializes a new instance of the <see cref="JilSerializer"/> class.
    /// </summary>
    public JilSerializer()
        : this(new(
            true,
            false,
            false,
            DateTimeFormat.ISO8601,
            true,
            UnspecifiedDateTimeKindBehavior.IsLocal))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JilSerializer"/> class.
    /// </summary>
    public JilSerializer(Options options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        JSON.SetDefaultOptions(options);
    }

    /// <inheritdoc/>
    public byte[] Serialize<T>(T? item)
        where T : class
    {
        var jsonString = JSON.Serialize(item);
        return encoding.GetBytes(jsonString);
    }

    /// <inheritdoc/>
    public T Deserialize<T>(byte[] serializedObject) where T : class
    {
        var jsonString = encoding.GetString(serializedObject);
        return JSON.Deserialize<T>(jsonString);
    }
}
