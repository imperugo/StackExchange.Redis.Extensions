// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Extensions;
using StackExchange.Redis.Extensions.Core.Helpers;
using StackExchange.Redis.Extensions.Core.Models;
using StackExchange.Redis.Extensions.Core.ServerIteration;
using StackExchange.Redis.KeyspaceIsolation;

namespace StackExchange.Redis.Extensions.Core.Implementations;

/// <inheritdoc/>
public partial class RedisDatabase : IRedisDatabase
{
    private readonly IRedisConnectionPoolManager connectionPoolManager;
    private readonly ServerEnumerationStrategy serverEnumerationStrategy;
    private readonly string keyPrefix;
    private readonly uint maxValueLength;
    private readonly int dbNumber;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisDatabase"/> class.
    /// </summary>
    /// <param name="connectionPoolManager">The connection pool manager.</param>
    /// <param name="serializer">The serializer.</param>
    /// <param name="serverEnumerationStrategy">The server enumeration strategy.</param>
    /// <param name="dbNumber">The database to use.</param>
    /// <param name="maxvalueLength">The max lenght of the cache object.</param>
    /// <param name="keyPrefix">The key prefix.</param>
    public RedisDatabase(
        IRedisConnectionPoolManager connectionPoolManager,
        ISerializer serializer,
        ServerEnumerationStrategy serverEnumerationStrategy,
        int dbNumber,
        uint maxvalueLength,
        string? keyPrefix = null)
    {
        Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        this.connectionPoolManager = connectionPoolManager ?? throw new ArgumentNullException(nameof(connectionPoolManager));
        this.serverEnumerationStrategy = serverEnumerationStrategy;
        this.dbNumber = dbNumber;
        this.keyPrefix = keyPrefix ?? string.Empty;
        maxValueLength = maxvalueLength;
    }

    /// <inheritdoc/>
    public IDatabase Database
    {
        get
        {
            var db = connectionPoolManager.GetConnection().GetDatabase(dbNumber);

            return keyPrefix.Length > 0
                ? db.WithKeyPrefix(keyPrefix)
                : db;
        }
    }

    /// <inheritdoc/>
    public ISerializer Serializer { get; }

    /// <inheritdoc/>
    public Task<bool> ExistsAsync(string key, CommandFlags flag = CommandFlags.None)
    {
        return Database.KeyExistsAsync(key, flag);
    }

    /// <inheritdoc/>
    public Task<bool> RemoveAsync(string key, CommandFlags flag = CommandFlags.None)
    {
        return Database.KeyDeleteAsync(key, flag);
    }

    /// <inheritdoc/>
    public Task<long> RemoveAllAsync(string[] keys, CommandFlags flag = CommandFlags.None)
    {
        var redisKeys = keys.ToFastArray(key => (RedisKey)key);

        return Database.KeyDeleteAsync(redisKeys, flag);
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key, CommandFlags flag = CommandFlags.None)
    {
        var valueBytes = await Database
            .StringGetAsync(key, flag)
            .ConfigureAwait(false);

        return !valueBytes.HasValue
            ? default
            : Serializer.Deserialize<T>(valueBytes);
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None)
    {
        var result = await GetAsync<T>(key, flag).ConfigureAwait(false);

        if (!EqualityComparer<T?>.Default.Equals(result, default))
            await Database.KeyExpireAsync(key, expiresAt.UtcDateTime.Subtract(DateTime.UtcNow)).ConfigureAwait(false);

        return result;
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None)
    {
        var result = await GetAsync<T>(key, flag).ConfigureAwait(false);

        if (!EqualityComparer<T?>.Default.Equals(result, default))
            await Database.KeyExpireAsync(key, expiresIn).ConfigureAwait(false);

        return result;
    }

    /// <inheritdoc/>
    public Task<bool> AddAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None, HashSet<string>? tags = null)
    {
        var entryBytes = value.OfValueSize(Serializer, maxValueLength, key);

        return tags?.Count > 0
            ? ExecuteAddWithTagsAsync(key, tags, db => db.StringSetAsync(key, entryBytes, null, when, flag), when, flag)
            : Database.StringSetAsync(key, entryBytes, null, when, flag);
    }

    /// <inheritdoc/>
    public Task<bool> ReplaceAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None)
    {
        return AddAsync(key, value, when, flag);
    }

    /// <inheritdoc/>
    public Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None, HashSet<string>? tags = null)
    {
        var entryBytes = value.OfValueSize(Serializer, maxValueLength, key);

        var expiration = expiresAt.UtcDateTime.Subtract(DateTime.UtcNow);

        return tags?.Count > 0
            ? ExecuteAddWithTagsAsync(key, tags, db => db.StringSetAsync(key, entryBytes, expiration, when, flag), when, flag)
            : Database.StringSetAsync(key, entryBytes, expiration, when, flag);
    }

    /// <inheritdoc/>
    public Task<bool> ReplaceAsync<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
    {
        return AddAsync(key, value, expiresAt, when, flag);
    }

    /// <inheritdoc/>
    public Task<bool> AddAsync<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None, HashSet<string>? tags = null)
    {
        var entryBytes = value.OfValueSize(Serializer, maxValueLength, key);

        return tags?.Count > 0
            ? ExecuteAddWithTagsAsync(key, tags, db => db.StringSetAsync(key, entryBytes, expiresIn, when, flag), when, flag)
            : Database.StringSetAsync(key, entryBytes, expiresIn, when, flag);
    }

    /// <inheritdoc/>
    public Task<bool> ReplaceAsync<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None)
    {
        return AddAsync(key, value, expiresIn, when, flag);
    }

    /// <inheritdoc/>
    public async Task<IDictionary<string, T?>> GetAllAsync<T>(HashSet<string> keys, CommandFlags flag = CommandFlags.None)
    {
        if (keys.Count == 0)
            return new Dictionary<string, T?>(0, StringComparer.Ordinal);

        var redisKeys = keys.ToFastArray(key => (RedisKey)key);

        var result = await Database.StringGetAsync(redisKeys, flag).ConfigureAwait(false);

        var dict = new Dictionary<string, T?>(redisKeys.Length, StringComparer.Ordinal);

        for (var index = 0; index < redisKeys.Length; index++)
        {
            var value = result[index];
            dict.Add(redisKeys[index]!, value == RedisValue.Null
                ? default
                : Serializer.Deserialize<T>(value));
        }

        return dict;
    }

    /// <inheritdoc/>
    public async Task<IDictionary<string, T?>> GetAllAsync<T>(HashSet<string> keys, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None)
    {
        var tsk1 = GetAllAsync<T>(keys, flag);
        var tsk2 = UpdateExpiryAllAsync(keys, expiresAt);

        await Task.WhenAll(tsk1, tsk2).ConfigureAwait(false);

        return tsk1.Result;
    }

    /// <inheritdoc/>
    public async Task<IDictionary<string, T?>> GetAllAsync<T>(HashSet<string> keys, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None)
    {
        var tsk1 = GetAllAsync<T>(keys, flag);
        var tsk2 = UpdateExpiryAllAsync(keys, expiresIn);

        await Task.WhenAll(tsk1, tsk2).ConfigureAwait(false);

        return tsk1.Result;
    }

    /// <inheritdoc/>
    public Task<bool> AddAllAsync<T>(Tuple<string, T>[] items, When when = When.Always, CommandFlags flag = CommandFlags.None)
    {
        var values = items
            .OfValueInListSize(Serializer, maxValueLength)
            .Select(x => new KeyValuePair<RedisKey, RedisValue>(x.Key, x.Value))
            .ToArray();

        return Database.StringSetAsync(values, when, flag);
    }

    /// <inheritdoc/>
    public async Task<bool> AddAllAsync<T>(Tuple<string, T>[] items, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
    {
        var values = items
            .OfValueInListSize(Serializer, maxValueLength)
            .Select(x => new KeyValuePair<RedisKey, RedisValue>(x.Key, x.Value))
            .ToArray();

        await Database.StringSetAsync(values, when, flag).ConfigureAwait(false);

        var tasks = values.ToFastArray(value => Database.KeyExpireAsync(value.Key, expiresAt.UtcDateTime, flag));

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return tasks[0].Result;
    }

    /// <inheritdoc/>
    public async Task<bool> AddAllAsync<T>(Tuple<string, T>[] items, TimeSpan expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
    {
        var values = items
            .OfValueInListSize(Serializer, maxValueLength)
            .Select(x => new KeyValuePair<RedisKey, RedisValue>(x.Key, x.Value))
            .ToArray();

        await Database.StringSetAsync(values, when, flag).ConfigureAwait(false);

        var tasks = values.ToFastArray(value => Database.KeyExpireAsync(value.Key, expiresAt, flag));

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return tasks[0].Result;
    }

    /// <inheritdoc/>
    public Task<bool> SetAddAsync<T>(string key, T item, CommandFlags flag = CommandFlags.None)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (item == null)
            throw new ArgumentNullException(nameof(item), "item cannot be null.");

        var serializedObject = Serializer.Serialize(item);

        return Database.SetAddAsync(key, serializedObject, flag);
    }

    /// <inheritdoc/>
    public async Task<T?> SetPopAsync<T>(string key, CommandFlags flag = CommandFlags.None)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        var item = await Database.SetPopAsync(key, flag).ConfigureAwait(false);

        return item == RedisValue.Null
            ? default
            : Serializer.Deserialize<T>(item);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T?>> SetPopAsync<T>(string key, long count, CommandFlags flag = CommandFlags.None)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        var items = await Database.SetPopAsync(key, count, flag).ConfigureAwait(false);

        return items.Select(item => item == RedisValue.Null ? default : Serializer.Deserialize<T>(item));
    }

    /// <inheritdoc/>
    public Task<bool> SetContainsAsync<T>(string key, T item, CommandFlags flag = CommandFlags.None)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (item == null)
            throw new ArgumentNullException(nameof(item), "item cannot be null.");

        var serializedObject = Serializer.Serialize(item);

        return Database.SetContainsAsync(key, serializedObject, flag);
    }

    /// <inheritdoc/>
    public Task<long> SetAddAllAsync<T>(string key, CommandFlags flag = CommandFlags.None, params T[]? items)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (items == null)
            throw new ArgumentNullException(nameof(items), "items cannot be null.");

        ExceptionThrowHelper.ThrowIfExistsNullElement(items, nameof(items));

        var values = items.ToFastArray(item => (RedisValue)Serializer.Serialize(item));

        return Database
            .SetAddAsync(
                key,
                values,
                flag);
    }

    /// <inheritdoc/>
    public Task<long> SetAddAllAsync<T>(string key, CommandFlags flag = CommandFlags.None, params ReadOnlySpan<T> items)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));
#endif
        ExceptionThrowHelper.ThrowIfExistsNullElement(items, nameof(items));

        var values = items.ToFastArray(item => (RedisValue)Serializer.Serialize(item));

        return Database
            .SetAddAsync(
                key,
                values,
                flag);
    }

    /// <inheritdoc/>
    public Task<bool> SetRemoveAsync<T>(string key, T item, CommandFlags flag = CommandFlags.None)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (item == null)
            throw new ArgumentNullException(nameof(item), "item cannot be null.");

        var serializedObject = Serializer.Serialize(item);

        return Database.SetRemoveAsync(key, serializedObject, flag);
    }

    /// <inheritdoc/>
    public Task<long> SetRemoveAllAsync<T>(string key, CommandFlags flag = CommandFlags.None, params T[] items)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (items == null)
            throw new ArgumentNullException(nameof(items), "items cannot be null.");

        ExceptionThrowHelper.ThrowIfExistsNullElement(items, nameof(items));

        var values = items.ToFastArray(item => (RedisValue)Serializer.Serialize(item));

        return Database.SetRemoveAsync(key, values, flag);
    }

    /// <inheritdoc/>
    public Task<long> SetRemoveAllAsync<T>(string key, CommandFlags flag = CommandFlags.None, params ReadOnlySpan<T> items)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));
#endif
        ExceptionThrowHelper.ThrowIfExistsNullElement(items, nameof(items));

        var values = items.ToFastArray(item => (RedisValue)Serializer.Serialize(item));

        return Database.SetRemoveAsync(key, values, flag);
    }

    /// <inheritdoc/>
    public async Task<string[]> SetMemberAsync(string memberName, CommandFlags flag = CommandFlags.None)
    {
        var members = await Database.SetMembersAsync(memberName, flag).ConfigureAwait(false);

        return members.ToFastArray(x => x.ToString());
    }

    /// <inheritdoc/>
    public async Task<T[]> SetMembersAsync<T>(string key, CommandFlags flag = CommandFlags.None)
    {
        var members = await Database.SetMembersAsync(key, flag).ConfigureAwait(false);

        if (members.Length == 0)
            return [];

        return members.ToFastArray(m => Serializer.Deserialize<T>(m)!);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> SearchKeysAsync(string pattern)
    {
        pattern = $"{keyPrefix}{pattern}";
        var keys = new HashSet<string>();

        foreach (var unused in ServerIteratorFactory.GetServers(connectionPoolManager.GetConnection(), serverEnumerationStrategy))
        {
            ulong nextCursor = 0;
            do
            {
                var redisResult = await unused.ExecuteAsync("SCAN", nextCursor.ToString(CultureInfo.InvariantCulture), "MATCH", pattern, "COUNT", "1000").ConfigureAwait(false);
                var innerResult = (RedisResult[])redisResult!;

                nextCursor = ulong.Parse((string)innerResult[0]!, CultureInfo.InvariantCulture);

                var resultLines = ((string[])innerResult[1]!).ToArray();
                keys.UnionWith(resultLines);
            }
            while (nextCursor != 0);
        }

        return !string.IsNullOrEmpty(keyPrefix)
            ? keys.Select(k => k[keyPrefix.Length..])
            : keys;
    }

    /// <inheritdoc/>
    public Task FlushDbAsync()
    {
        var endPoints = Database.Multiplexer.GetEndPoints();

        var tasks = new List<Task>(endPoints.Length);

        ref var searchSpace = ref MemoryMarshal.GetReference(endPoints.AsSpan());

        for (var i = 0; i < endPoints.Length; i++)
        {
            ref var endpoint = ref Unsafe.Add(ref searchSpace, i);

            var server = Database.Multiplexer.GetServer(endpoint);

            if (!server.IsReplica)
                tasks.Add(server.FlushDatabaseAsync(Database.Database));
        }

        return Task.WhenAll(tasks);
    }

    /// <inheritdoc/>
    public Task SaveAsync(SaveType saveType, CommandFlags flag = CommandFlags.None)
    {
        var endPoints = Database.Multiplexer.GetEndPoints();

        var tasks = endPoints.ToFastArray(endpoint => Database.Multiplexer.GetServer(endpoint).SaveAsync(saveType, flag));

        return Task.WhenAll(tasks);
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, string>> GetInfoAsync()
    {
        var info = (await Database.ScriptEvaluateAsync("return redis.call('INFO')").ConfigureAwait(false)).ToString();

        return string.IsNullOrEmpty(info)
            ? new Dictionary<string, string>()
            : ParseInfo(info);
    }

    /// <inheritdoc/>
    public async Task<InfoDetail[]> GetInfoCategorizedAsync()
    {
        var info = (await Database.ScriptEvaluateAsync("return redis.call('INFO')").ConfigureAwait(false)).ToString();

        return string.IsNullOrEmpty(info)
            ? []
            : ParseCategorizedInfo(info);
    }

    /// <inheritdoc/>
    public Task<double> SortedSetAddIncrementAsync<T>(string key, T? value, double score, CommandFlags flag = CommandFlags.None)
    {
        var entryBytes = Serializer.Serialize(value);
        return Database.SortedSetIncrementAsync(key, entryBytes, score, flag);
    }

    private static Dictionary<string, string> ParseInfo(string info)
    {
        // Call Parse Categorized Info to cut back on duplicated code.
        var data = ParseCategorizedInfo(info);

        // Return a dictionary of the Info Key and Info value

        var result = new Dictionary<string, string>();

        data.FastIteration((x, _) => result.TryAdd(x.Key, x.InfoValue));

        return result;
    }

    private static InfoDetail[] ParseCategorizedInfo(string info)
    {
        var data = new List<InfoDetail>();
        var category = string.Empty;

        info.AsSpan().EnumerateLines(ref data, ref category);

        return data.ToArray();
    }
}
