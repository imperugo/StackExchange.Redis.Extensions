// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

using StackExchange.Redis.Extensions.Core.Helpers;

namespace StackExchange.Redis.Extensions.Core.Implementations;

public partial class RedisDatabase
{
    /// <inheritdoc/>
    public Task<long> ListAddToLeftAsync<T>(string key, T item, When when = When.Always, CommandFlags flag = CommandFlags.None)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (item == null)
            throw new ArgumentNullException(nameof(item), "item cannot be null.");

        var serializedItem = Serializer.Serialize(item);

        return Database.ListLeftPushAsync(key, serializedItem, when, flag);
    }

    /// <inheritdoc/>
    public Task<long> ListAddToLeftAsync<T>(string key, T[] items, CommandFlags flag = CommandFlags.None)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (items == null)
            throw new ArgumentNullException(nameof(items), "item cannot be null.");

        var serializedItems = items.ToFastArray(item => (RedisValue)Serializer.Serialize(item));

        return Database.ListLeftPushAsync(key, serializedItems, flag);
    }

    /// <inheritdoc/>
    public async Task<T?> ListGetFromRightAsync<T>(string key, CommandFlags flag = CommandFlags.None)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        var item = await Database.ListRightPopAsync(key, flag).ConfigureAwait(false);

        if (item == RedisValue.Null)
            return default;

        return item == RedisValue.Null
            ? default
            : Serializer.Deserialize<T>(item);
    }
}
