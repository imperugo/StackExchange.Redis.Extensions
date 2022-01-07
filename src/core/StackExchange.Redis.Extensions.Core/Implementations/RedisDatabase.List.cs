// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using StackExchange.Redis.Extensions.Core.Abstractions;

namespace StackExchange.Redis.Extensions.Core.Implementations;

public partial class RedisDatabase : IRedisDatabase
{
    /// <inheritdoc/>
    public Task<long> ListAddToLeftAsync<T>(string key, T item, When when = When.Always, CommandFlags flags = CommandFlags.None)
        where T : class
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (item == null)
            throw new ArgumentNullException(nameof(item), "item cannot be null.");

        var serializedItem = Serializer.Serialize(item);

        return Database.ListLeftPushAsync(key, serializedItem, when, flags);
    }

    /// <inheritdoc/>
    public Task<long> ListAddToLeftAsync<T>(string key, T[] items, CommandFlags flags = CommandFlags.None)
        where T : class
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (items == null)
            throw new ArgumentNullException(nameof(items), "item cannot be null.");

        var serializedItems = items.Select(x => (RedisValue)Serializer.Serialize(x)).ToArray();

        return Database.ListLeftPushAsync(key, serializedItems, flags);
    }

    /// <inheritdoc/>
    public async Task<T?> ListGetFromRightAsync<T>(string key, CommandFlags flags = CommandFlags.None)
        where T : class
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        var item = await Database.ListRightPopAsync(key, flags).ConfigureAwait(false);

        if (item == RedisValue.Null)
            return null;

        return item == RedisValue.Null
            ? null
            : Serializer.Deserialize<T>(item);
    }
}
