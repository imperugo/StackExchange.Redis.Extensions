// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisDatabase"/> class.
    /// </summary>
    /// <param name="connectionPoolManager">The connection pool manager.</param>
    /// <param name="serializer">The serializer.</param>
    /// <param name="serverEnumerationStrategy">The server enumeration strategy.</param>
    /// <param name="dbNumber">The database to use.</param>
    /// <param name="maxvalueLength">The max lenght of the cache object.</param>
    /// <param name="keyPrefix">The key prefix.</param>
    /// <param name="logger">The logger.</param>
    public RedisDatabase(
        IRedisConnectionPoolManager connectionPoolManager,
        ISerializer serializer,
        ServerEnumerationStrategy serverEnumerationStrategy,
        int dbNumber,
        uint maxvalueLength,
        string? keyPrefix = null,
        ILogger<RedisDatabase>? logger = null)
    {
        Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        this.connectionPoolManager = connectionPoolManager ?? throw new ArgumentNullException(nameof(connectionPoolManager));
        this.serverEnumerationStrategy = serverEnumerationStrategy;
        this.dbNumber = dbNumber;
        this.keyPrefix = keyPrefix ?? string.Empty;
        maxValueLength = maxvalueLength;
        this.logger = logger ?? (ILogger)NullLogger<RedisDatabase>.Instance;
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
        // Database resolves a pooled connection on every access: read it once for the two commands.
        var db = Database;
        var valueBytes = await db.StringGetAsync(key, flag).ConfigureAwait(false);
        var result = !valueBytes.HasValue ? default : Serializer.Deserialize<T>(valueBytes);

        if (!EqualityComparer<T?>.Default.Equals(result, default))
            await db.KeyExpireAsync(key, expiresAt.UtcDateTime.Subtract(DateTime.UtcNow)).ConfigureAwait(false);

        return result;
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None)
    {
        var db = Database;
        var valueBytes = await db.StringGetAsync(key, flag).ConfigureAwait(false);
        var result = !valueBytes.HasValue ? default : Serializer.Deserialize<T>(valueBytes);

        if (!EqualityComparer<T?>.Default.Equals(result, default))
            await db.KeyExpireAsync(key, expiresIn).ConfigureAwait(false);

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
        var values = items.ToRedisEntries(Serializer, maxValueLength);

        return Database.StringSetAsync(values, when, flag);
    }

    /// <inheritdoc/>
    public async Task<bool> AddAllAsync<T>(Tuple<string, T>[] items, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
    {
        var values = items.ToRedisEntries(Serializer, maxValueLength);

        if (values.Length == 0)
            return false;

        var expiration = expiresAt.UtcDateTime.Subtract(DateTime.UtcNow);

        if (expiration <= TimeSpan.Zero)
            return false;

        var db = Database;
        var batch = db.CreateBatch();

        var tasks = values.ToFastArray(v => batch.StringSetAsync(v.Key, v.Value, expiration, when, flag));

        batch.Execute();

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return Array.TrueForAll(tasks, t => t.Result);
    }

    /// <inheritdoc/>
    public async Task<bool> AddAllAsync<T>(Tuple<string, T>[] items, TimeSpan expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
    {
        var values = items.ToRedisEntries(Serializer, maxValueLength);

        if (values.Length == 0)
            return false;

        var db = Database;
        var batch = db.CreateBatch();

        var tasks = values.ToFastArray(v => batch.StringSetAsync(v.Key, v.Value, expiresAt, when, flag));

        batch.Execute();

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return Array.TrueForAll(tasks, t => t.Result);
    }

    /// <inheritdoc/>
    public Task<bool> SetAddAsync<T>(string key, T item, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(item);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (item == null)
            throw new ArgumentNullException(nameof(item), "item cannot be null.");
#endif

        var serializedObject = Serializer.Serialize(item);

        return Database.SetAddAsync(key, serializedObject, flag);
    }

    /// <inheritdoc/>
    public async Task<T?> SetPopAsync<T>(string key, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));
#endif

        var item = await Database.SetPopAsync(key, flag).ConfigureAwait(false);

        return item == RedisValue.Null
            ? default
            : Serializer.Deserialize<T>(item);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T?>> SetPopAsync<T>(string key, long count, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));
#endif

        var items = await Database.SetPopAsync(key, count, flag).ConfigureAwait(false);

        // Materialized eagerly: a lazy Select would re-deserialize every element on each enumeration.
        return items.ToFastArray(item => item == RedisValue.Null ? default : Serializer.Deserialize<T>(item));
    }

    /// <inheritdoc/>
    public Task<bool> SetContainsAsync<T>(string key, T item, CommandFlags flag = CommandFlags.None)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(item);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (item == null)
            throw new ArgumentNullException(nameof(item), "item cannot be null.");
#endif

        var serializedObject = Serializer.Serialize(item);

        return Database.SetContainsAsync(key, serializedObject, flag);
    }

    /// <inheritdoc/>
    public Task<long> SetAddAllAsync<T>(string key, CommandFlags flag = CommandFlags.None, params T[]? items)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(items);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (items == null)
            throw new ArgumentNullException(nameof(items), "items cannot be null.");
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
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(item);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (item == null)
            throw new ArgumentNullException(nameof(item), "item cannot be null.");
#endif

        var serializedObject = Serializer.Serialize(item);

        return Database.SetRemoveAsync(key, serializedObject, flag);
    }

    /// <inheritdoc/>
    public Task<long> SetRemoveAllAsync<T>(string key, CommandFlags flag = CommandFlags.None, params T[] items)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(items);
#else
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("key cannot be empty.", nameof(key));

        if (items == null)
            throw new ArgumentNullException(nameof(items), "items cannot be null.");
#endif

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
    public async Task<T[]> SetCombineAsync<T>(SetOperation operation, string firstKey, string secondKey, CommandFlags flag = CommandFlags.None)
    {
        var members = await Database.SetCombineAsync(operation, firstKey, secondKey, flag).ConfigureAwait(false);

        if (members.Length == 0)
            return [];

        return members.ToFastArray(m => Serializer.Deserialize<T>(m)!);
    }

    /// <inheritdoc/>
    public async Task<T[]> SetCombineAsync<T>(SetOperation operation, string[] keys, CommandFlags flag = CommandFlags.None)
    {
        var redisKeys = keys.ToFastArray(k => (RedisKey)k);
        var members = await Database.SetCombineAsync(operation, redisKeys, flag).ConfigureAwait(false);

        if (members.Length == 0)
            return [];

        return members.ToFastArray(m => Serializer.Deserialize<T>(m)!);
    }

    /// <inheritdoc/>
    public Task<long> SetCombineAndStoreAsync(SetOperation operation, string destinationKey, string firstKey, string secondKey, CommandFlags flag = CommandFlags.None)
        => Database.SetCombineAndStoreAsync(operation, destinationKey, firstKey, secondKey, flag);

    /// <inheritdoc/>
    public Task<long> SetCombineAndStoreAsync(SetOperation operation, string destinationKey, string[] keys, CommandFlags flag = CommandFlags.None)
        => Database.SetCombineAndStoreAsync(operation, destinationKey, keys.ToFastArray(k => (RedisKey)k), flag);

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> SearchKeysAsync(string pattern)
    {
        pattern = keyPrefix + pattern;
        var keys = new HashSet<string>();
        var hasPrefix = !string.IsNullOrEmpty(keyPrefix);

        var serverSource = new ServerSource(connectionPoolManager.GetConnection());
        foreach (var server in serverSource.GetServers(serverEnumerationStrategy))
        {
            // The prefix is stripped while filling: a lazy Select would re-allocate every substring on each enumeration.
            await foreach (var key in server.KeysAsync(dbNumber, pattern, 1000).ConfigureAwait(false))
                keys.Add(hasPrefix ? ((string)key!)[keyPrefix.Length..] : key!);
        }

        return keys;
    }

    /// <inheritdoc/>
    public Task FlushDbAsync()
    {
        // Database resolves a pooled connection and allocates the key-prefixed wrapper on every access: read it once.
        var db = Database;
        var multiplexer = db.Multiplexer;
        var endPoints = multiplexer.GetEndPoints();

        var tasks = new List<Task>(endPoints.Length);

        ref var searchSpace = ref MemoryMarshal.GetReference(endPoints.AsSpan());

        for (var i = 0; i < endPoints.Length; i++)
        {
            ref var endpoint = ref Unsafe.Add(ref searchSpace, i);

            var server = multiplexer.GetServer(endpoint);

            if (!server.IsReplica)
                tasks.Add(server.FlushDatabaseAsync(db.Database));
        }

        return Task.WhenAll(tasks);
    }

    /// <inheritdoc/>
    public Task SaveAsync(SaveType saveType, CommandFlags flag = CommandFlags.None)
    {
        var multiplexer = Database.Multiplexer;
        var endPoints = multiplexer.GetEndPoints();

        var tasks = endPoints.ToFastArray(endpoint => multiplexer.GetServer(endpoint).SaveAsync(saveType, flag));

        return Task.WhenAll(tasks);
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, string>> GetInfoAsync()
    {
        var info = (await Database.ScriptEvaluateAsync("return redis.call('INFO')").ConfigureAwait(false)).ToString();

        return InfoDetailsParser.ParseAsDictionary(info);
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, string>> GetInfoAsync(string section)
    {
        var script = string.IsNullOrWhiteSpace(section)
            ? "return redis.call('INFO')"
            : $"return redis.call('INFO','{section}')";
        var info = (await Database.ScriptEvaluateAsync(script).ConfigureAwait(false)).ToString();

        return InfoDetailsParser.ParseAsDictionary(info);
    }

    /// <inheritdoc/>
    public async Task<InfoDetail[]> GetInfoCategorizedAsync()
    {
        var info = (await Database.ScriptEvaluateAsync("return redis.call('INFO')").ConfigureAwait(false)).ToString();

        return InfoDetail.ParseFrom(info);
    }

    /// <inheritdoc/>
    public async Task<InfoDetail[]> GetInfoCategorizedAsync(string section)
    {
        var script = string.IsNullOrWhiteSpace(section)
            ? "return redis.call('INFO')"
            : $"return redis.call('INFO','{section}')";
        var info = (await Database.ScriptEvaluateAsync(script).ConfigureAwait(false)).ToString();

        return InfoDetail.ParseFrom(info);
    }

    /// <inheritdoc/>
    public Task<double> SortedSetAddIncrementAsync<T>(string key, T? value, double score, CommandFlags flag = CommandFlags.None)
    {
        var entryBytes = Serializer.Serialize(value);
        return Database.SortedSetIncrementAsync(key, entryBytes, score, flag);
    }

    /// <inheritdoc/>
    public Task<long> StringIncrementAsync(string key, long value = 1, CommandFlags flag = CommandFlags.None)
        => Database.StringIncrementAsync(key, value, flag);

    /// <inheritdoc/>
    public Task<long> StringDecrementAsync(string key, long value = 1, CommandFlags flag = CommandFlags.None)
        => Database.StringDecrementAsync(key, value, flag);

    /// <inheritdoc/>
    public Task<double> StringIncrementAsync(string key, double value, CommandFlags flag = CommandFlags.None)
        => Database.StringIncrementAsync(key, value, flag);

    /// <inheritdoc/>
    public Task<double> StringDecrementAsync(string key, double value, CommandFlags flag = CommandFlags.None)
        => Database.StringDecrementAsync(key, value, flag);

    /// <inheritdoc/>
    public Task<bool> KeyRenameAsync(string key, string newKey, When when = When.Always, CommandFlags flag = CommandFlags.None)
        => Database.KeyRenameAsync(key, newKey, when, flag);

    /// <inheritdoc/>
    public Task<RedisType> KeyTypeAsync(string key, CommandFlags flag = CommandFlags.None)
        => Database.KeyTypeAsync(key, flag);

    /// <inheritdoc/>
    public Task<byte[]?> KeyDumpAsync(string key, CommandFlags flag = CommandFlags.None)
        => Database.KeyDumpAsync(key, flag);

    /// <inheritdoc/>
    public Task KeyRestoreAsync(string key, byte[] value, TimeSpan? expiry = null, CommandFlags flag = CommandFlags.None)
        => Database.KeyRestoreAsync(key, value, expiry, flag);
}
