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
    public Task<bool> HashDeleteAsync(string hashKey, string key, CommandFlags flag = CommandFlags.None)
    {
        return Database.HashDeleteAsync(hashKey, key, flag);
    }

    /// <inheritdoc/>
    public Task<long> HashDeleteAsync(string hashKey, string[] keys, CommandFlags flag = CommandFlags.None)
    {
        var redisKeys = new RedisValue[keys.Length];

        for (var i = 0; i < keys.Length; i++)
            redisKeys[i] = (RedisValue)keys[i];

        return Database.HashDeleteAsync(hashKey, redisKeys, flag);
    }

    /// <inheritdoc/>
    public Task<bool> HashExistsAsync(string hashKey, string key, CommandFlags flag = CommandFlags.None)
    {
        return Database.HashExistsAsync(hashKey, key, flag);
    }

    /// <inheritdoc/>
    public async Task<T?> HashGetAsync<T>(string hashKey, string key, CommandFlags flag = CommandFlags.None)
    {
        var redisValue = await Database.HashGetAsync(hashKey, key, flag).ConfigureAwait(false);

        return redisValue.HasValue ? Serializer.Deserialize<T>(redisValue!) : default;
    }

    /// <inheritdoc/>
    public async Task<IDictionary<string, T?>> HashGetAsync<T>(string hashKey, string[] keys, CommandFlags flag = CommandFlags.None)
    {
#if NET6_0_OR_GREATER
        var concurrent = new ConcurrentDictionary<string, T?>();

        await Parallel.ForEachAsync(keys, async (key, _) =>
        {
            var result = await HashGetAsync<T>(hashKey, key, flag);
            concurrent.TryAdd(key, result);
        })
            .ConfigureAwait(false);

        return concurrent;
#else
        var tasks = new Task<T?>[keys.Length];

        for (var i = 0; i < keys.Length; i++)
            tasks[i] = HashGetAsync<T>(hashKey, keys[i], flag);

        await Task.WhenAll(tasks).ConfigureAwait(false);

        var result = new Dictionary<string, T?>();

        for (var i = 0; i < tasks.Length; i++)
            result.Add(keys[i], tasks[i].Result);

        return result;
#endif
    }

    /// <inheritdoc/>
    public async Task<IDictionary<string, T?>> HashGetAllAsyncAtOneTimeAsync<T>(string hashKey, string[] keys, CommandFlags flag = CommandFlags.None)
    {
        var luascript = "local results = {};local insert = table.insert;local rcall = redis.call;for i=1,table.getn(KEYS) do  local value = rcall('HGET','" + hashKey + "', KEYS[i])  if value then insert(results, KEYS[i]) insert(results, value) end end; return results;";

        var list = new List<RedisKey>();

        foreach (var key in keys)
            list.Add(new RedisKey(key));

        var redisKeys = list.ToArray();

        var data = await Database.ScriptEvaluateAsync(luascript, redisKeys, flags: flag).ConfigureAwait(false);

        var dictionary = new Dictionary<string, T?>();

        if (data is null)
            return dictionary;

        var redisValues = ((RedisValue[]?)data);

        if (redisValues == null || redisValues.Length <= 0)
            return dictionary;

        for (var i = 0; i < redisValues.Length; i += 2)
        {
            var key = redisValues[i];

            if (key.HasValue == false)
                continue;

            var redisValue = redisValues[i + 1];

            if (redisValue.HasValue == false)
                continue;

#pragma warning disable CS8604 // Possible null reference argument.
            var value = Serializer.Deserialize<T?>(redisValue);
            dictionary.Add(key, value);
#pragma warning restore CS8604 // Possible null reference argument.
        }

        return dictionary;
    }

    /// <inheritdoc/>
    public async Task<IDictionary<string, T?>> HashGetAllAsync<T>(string hashKey, CommandFlags flag = CommandFlags.None)
    {
        return (await Database.HashGetAllAsync(hashKey, flag).ConfigureAwait(false))
            .ToDictionary(
                x => x.Name.ToString(),
                x => Serializer.Deserialize<T>(x.Value!),
                StringComparer.Ordinal);
    }

    /// <inheritdoc/>
    public Task<long> HashIncrementByAsync(string hashKey, string key, long value, CommandFlags flag = CommandFlags.None)
    {
        return Database.HashIncrementAsync(hashKey, key, value, flag);
    }

    /// <inheritdoc/>
    public Task<double> HashIncrementByAsync(string hashKey, string key, double value, CommandFlags flag = CommandFlags.None)
    {
        return Database.HashIncrementAsync(hashKey, key, value, flag);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> HashKeysAsync(string hashKey, CommandFlags flag = CommandFlags.None)
    {
        return (await Database.HashKeysAsync(hashKey, flag).ConfigureAwait(false)).Select(x => x.ToString());
    }

    /// <inheritdoc/>
    public Task<long> HashLengthAsync(string hashKey, CommandFlags flag = CommandFlags.None)
    {
        return Database.HashLengthAsync(hashKey, flag);
    }

    /// <inheritdoc/>
    public Task<bool> HashSetAsync<T>(string hashKey, string key, T value, bool nx = false, CommandFlags flag = CommandFlags.None)
    {
        return Database.HashSetAsync(hashKey, key, Serializer.Serialize(value), nx ? When.NotExists : When.Always, flag);
    }

    /// <inheritdoc/>
    public Task HashSetAsync<T>(string hashKey, IDictionary<string, T> values, CommandFlags flag = CommandFlags.None)
    {
        var entries = values.Select(kv => new HashEntry(kv.Key, Serializer.Serialize(kv.Value)));

        return Database.HashSetAsync(hashKey, entries.ToArray(), flag);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T?>> HashValuesAsync<T>(string hashKey, CommandFlags flag = CommandFlags.None)
    {
        return (await Database.HashValuesAsync(hashKey, flag).ConfigureAwait(false)).Select(x => Serializer.Deserialize<T>(x!));
    }

    /// <inheritdoc/>
    public Dictionary<string, T?> HashScan<T>(string hashKey, string pattern, int pageSize = 10, CommandFlags flag = CommandFlags.None)
    {
        return Database.HashScan(hashKey, pattern, pageSize, flag).ToDictionary(x => x.Name.ToString(), x => Serializer.Deserialize<T>(x.Value!), StringComparer.Ordinal);
    }
}
