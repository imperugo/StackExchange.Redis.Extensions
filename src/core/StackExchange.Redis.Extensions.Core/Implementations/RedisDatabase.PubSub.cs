// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using StackExchange.Redis.Extensions.Core.Helpers;

namespace StackExchange.Redis.Extensions.Core.Implementations;

public partial class RedisDatabase
{
    /// <inheritdoc/>
    public Task<long> PublishAsync<T>(RedisChannel channel, T message, CommandFlags flag = CommandFlags.None)
    {
        var sub = connectionPoolManager.GetConnection().GetSubscriber();
        return sub.PublishAsync(channel, Serializer.Serialize(message), flag);
    }

    /// <inheritdoc/>
    public Task SubscribeAsync<T>(RedisChannel channel, Func<T?, Task> handler, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(handler);
#else
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));
#endif

        var sub = connectionPoolManager.GetConnection().GetSubscriber();

        return sub.SubscribeAsync(channel, Handler, flag);

        void Handler(RedisChannel redisChannel, RedisValue value) =>
            _ = handler(Serializer.Deserialize<T>(value)).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task UnsubscribeAsync<T>(RedisChannel channel, Func<T?, Task> handler, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(handler);
#else
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));
#endif

        var sub = connectionPoolManager.GetConnection().GetSubscriber();
        return sub.UnsubscribeAsync(channel, (_, value) => handler(Serializer.Deserialize<T>(value)), flag);
    }

    /// <inheritdoc/>
    public Task UnsubscribeAllAsync(CommandFlags flag = CommandFlags.None)
    {
        var sub = connectionPoolManager.GetConnection().GetSubscriber();
        return sub.UnsubscribeAllAsync(flag);
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateExpiryAsync(string key, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None)
    {
        if (await Database.KeyExistsAsync(key).ConfigureAwait(false))
            return await Database.KeyExpireAsync(key, expiresAt.UtcDateTime.Subtract(DateTime.UtcNow), flag).ConfigureAwait(false);

        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateExpiryAsync(string key, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None)
    {
        if (await Database.KeyExistsAsync(key).ConfigureAwait(false))
            return await Database.KeyExpireAsync(key, expiresIn, flag).ConfigureAwait(false);

        return false;
    }

    /// <inheritdoc/>
    public async Task<IDictionary<string, bool>> UpdateExpiryAllAsync(HashSet<string> keys, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None)
    {
        var tasks = keys.ToFastArray(key => UpdateExpiryAsync(key, expiresAt.UtcDateTime, flag));

        await Task.WhenAll(tasks).ConfigureAwait(false);

        var results = new Dictionary<string, bool>(keys.Count, StringComparer.Ordinal);

        keys.FastIteration((key, i) => results.Add(key, tasks[i].Result));

        return results;
    }

    /// <inheritdoc/>
    public async Task<IDictionary<string, bool>> UpdateExpiryAllAsync(HashSet<string> keys, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None)
    {
        var tasks = keys.ToFastArray(key => UpdateExpiryAsync(key, expiresIn, flag));

        await Task.WhenAll(tasks).ConfigureAwait(false);

        var results = new Dictionary<string, bool>(keys.Count, StringComparer.Ordinal);

        keys.FastIteration((key, i) => results.Add(key, tasks[i].Result));

        return results;
    }
}
