// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions.Core.Implementations;

public partial class RedisDatabase
{
    /// <inheritdoc/>
    public Task<bool> HashDeleteAsync(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
    {
        return Database.HashDeleteAsync(hashKey, key, commandFlags);
    }

    /// <inheritdoc/>
    public Task<long> HashDeleteAsync(string hashKey, string[] keys, CommandFlags commandFlags = CommandFlags.None)
    {
        var redisKeys = new RedisValue[keys.Length];

        for (var i = 0; i < keys.Length; i++)
            redisKeys[i] = (RedisValue)keys[i];

        return Database.HashDeleteAsync(hashKey, redisKeys, commandFlags);
    }

    /// <inheritdoc/>
    public Task<bool> HashExistsAsync(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
    {
        return Database.HashExistsAsync(hashKey, key, commandFlags);
    }

    /// <inheritdoc/>
    public async Task<T?> HashGetAsync<T>(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
    {
        var redisValue = await Database.HashGetAsync(hashKey, key, commandFlags).ConfigureAwait(false);

        return redisValue.HasValue ? Serializer.Deserialize<T>(redisValue!) : default;
    }

    /// <inheritdoc/>
    public async Task<IDictionary<string, T?>> HashGetAsync<T>(string hashKey, string[] keys, CommandFlags commandFlags = CommandFlags.None)
    {
#if NET8_0 || NET7_0 || NET6_0
        var concurrent = new ConcurrentDictionary<string, T?>();

        await Parallel.ForEachAsync(keys, async (key, token) =>
        {
            var result = await HashGetAsync<T>(hashKey, key, commandFlags);
            concurrent.TryAdd(key, result);
        })
            .ConfigureAwait(false);

        return concurrent;
#else
        var tasks = new Task<T?>[keys.Length];

        for (var i = 0; i < keys.Length; i++)
            tasks[i] = HashGetAsync<T>(hashKey, keys[i], commandFlags);

        await Task.WhenAll(tasks).ConfigureAwait(false);

        var result = new Dictionary<string, T?>();

        for (var i = 0; i < tasks.Length; i++)
            result.Add(keys[i], tasks[i].Result);

        return result;
#endif
    }

    /// <inheritdoc/>
    public async Task<IDictionary<string, T?>> HashGetAllAsync<T>(string hashKey, CommandFlags commandFlags = CommandFlags.None)
    {
        return (await Database.HashGetAllAsync(hashKey, commandFlags).ConfigureAwait(false))
            .ToDictionary(
                x => x.Name.ToString(),
                x => Serializer.Deserialize<T>(x.Value!),
                StringComparer.Ordinal);
    }

    /// <inheritdoc/>
    public Task<long> HashIncerementByAsync(string hashKey, string key, long value, CommandFlags commandFlags = CommandFlags.None)
    {
        return Database.HashIncrementAsync(hashKey, key, value, commandFlags);
    }

    /// <inheritdoc/>
    public Task<double> HashIncerementByAsync(string hashKey, string key, double value, CommandFlags commandFlags = CommandFlags.None)
    {
        return Database.HashIncrementAsync(hashKey, key, value, commandFlags);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> HashKeysAsync(string hashKey, CommandFlags commandFlags = CommandFlags.None)
    {
        return (await Database.HashKeysAsync(hashKey, commandFlags).ConfigureAwait(false)).Select(x => x.ToString());
    }

    /// <inheritdoc/>
    public Task<long> HashLengthAsync(string hashKey, CommandFlags commandFlags = CommandFlags.None)
    {
        return Database.HashLengthAsync(hashKey, commandFlags);
    }

    /// <inheritdoc/>
    public Task<bool> HashSetAsync<T>(string hashKey, string key, T value, bool nx = false, CommandFlags commandFlags = CommandFlags.None)
    {
        return Database.HashSetAsync(hashKey, key, Serializer.Serialize(value), nx ? When.NotExists : When.Always, commandFlags);
    }

    /// <inheritdoc/>
    public Task HashSetAsync<T>(string hashKey, IDictionary<string, T> values, CommandFlags commandFlags = CommandFlags.None)
    {
        var entries = values.Select(kv => new HashEntry(kv.Key, Serializer.Serialize(kv.Value)));

        return Database.HashSetAsync(hashKey, entries.ToArray(), commandFlags);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T?>> HashValuesAsync<T>(string hashKey, CommandFlags commandFlags = CommandFlags.None)
    {
        return (await Database.HashValuesAsync(hashKey, commandFlags).ConfigureAwait(false)).Select(x => Serializer.Deserialize<T>(x!));
    }

    /// <inheritdoc/>
    public Dictionary<string, T?> HashScan<T>(string hashKey, string pattern, int pageSize = 10, CommandFlags commandFlags = CommandFlags.None)
    {
        return Database.HashScan(hashKey, pattern, pageSize, commandFlags).ToDictionary(x => x.Name.ToString(), x => Serializer.Deserialize<T>(x.Value!), StringComparer.Ordinal);
    }
}
