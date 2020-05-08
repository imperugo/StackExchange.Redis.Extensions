﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Models;
using StackExchange.Redis.Extensions.Core.ServerIteration;
using StackExchange.Redis.KeyspaceIsolation;

namespace StackExchange.Redis.Extensions.Core.Implementations
{
    internal class RedisDatabase : IRedisDatabase
    {
        private static readonly Encoding encoding = Encoding.UTF8;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly ServerEnumerationStrategy serverEnumerationStrategy = new ServerEnumerationStrategy();
        private readonly string keyprefix;
        private readonly uint maxValueLength;

        public RedisDatabase(
                IConnectionMultiplexer connectionMultiplexer,
                ISerializer serializer,
                ServerEnumerationStrategy serverEnumerationStrategy,
                IDatabase database,
                uint maxvalueLength,
                string keyPrefix = null)
        {
            this.serverEnumerationStrategy = serverEnumerationStrategy ?? new ServerEnumerationStrategy();
            this.Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            this.connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));

            Database = database;

            if (!string.IsNullOrWhiteSpace(keyPrefix))
            {
                Database = Database.WithKeyPrefix(keyPrefix);
            }

            keyprefix = keyPrefix;
            maxValueLength = maxvalueLength;
        }

        public IDatabase Database { get; }

        public ISerializer Serializer { get; }

        public bool Exists(string key, CommandFlags flags = CommandFlags.None)
        {
            return Database.KeyExists(key, flags);
        }

        public Task<bool> ExistsAsync(string key, CommandFlags flags = CommandFlags.None)
        {
            return Database.KeyExistsAsync(key, flags);
        }

        public bool Remove(string key, CommandFlags flags = CommandFlags.None)
        {
            return Database.KeyDelete(key, flags);
        }

        public Task<bool> RemoveAsync(string key, CommandFlags flags = CommandFlags.None)
        {
            return Database.KeyDeleteAsync(key, flags);
        }

        public void RemoveAll(IEnumerable<string> keys, CommandFlags flags = CommandFlags.None)
        {
            var redisKeys = keys.Select(x => (RedisKey)x).ToArray();
            Database.KeyDelete(redisKeys, flags);
        }

        public Task RemoveAllAsync(IEnumerable<string> keys, CommandFlags flags = CommandFlags.None)
        {
            var redisKeys = keys.Select(x => (RedisKey)x).ToArray();
            return Database.KeyDeleteAsync(redisKeys, flags);
        }

        public T Get<T>(string key, CommandFlags flag = CommandFlags.None)
        {
            var valueBytes = Database.StringGet(key, flag);

            if (!valueBytes.HasValue)
                return default;

            return Serializer.Deserialize<T>(valueBytes);
        }

        public T Get<T>(string key, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None)
        {
            var result = Get<T>(key, flag);

            if (!Equals(result, default(T)))
                Database.KeyExpire(key, expiresAt.UtcDateTime.Subtract(DateTime.UtcNow));

            return result;
        }

        public T Get<T>(string key, TimeSpan expiresIn, CommandFlags flags = CommandFlags.None)
        {
            var result = Get<T>(key, flags);

            if (!Equals(result, default(T)))
                Database.KeyExpire(key, expiresIn);

            return result;
        }

        public async Task<T> GetAsync<T>(string key, CommandFlags flag = CommandFlags.None)
        {
            var valueBytes = await Database.StringGetAsync(key, flag);

            if (!valueBytes.HasValue)
                return default;

            return Serializer.Deserialize<T>(valueBytes);
        }

        public async Task<T> GetAsync<T>(string key, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None)
        {
            var result = await GetAsync<T>(key, flag);

            if (!Equals(result, default(T)))
                await Database.KeyExpireAsync(key, expiresAt.UtcDateTime.Subtract(DateTime.UtcNow));

            return result;
        }

        public async Task<T> GetAsync<T>(string key, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None)
        {
            var result = await GetAsync<T>(key, flag);

            if (!Equals(result, default(T)))
                await Database.KeyExpireAsync(key, expiresIn);

            return result;
        }

        public bool Add<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var entryBytes = Serializer.Serialize(value);
            if (maxValueLength > 0 && entryBytes.Length > maxValueLength)
                throw new ArgumentException("value cannot be longer than the MaxValueLength", nameof(value));

            return Database.StringSet(key, entryBytes, null, when, flag);
        }

        public async Task<bool> AddAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var entryBytes = Serializer.Serialize(value);
            if (maxValueLength > 0 && entryBytes.Length > maxValueLength)
                throw new ArgumentException("value cannot be longer than the MaxValueLength", nameof(value));

            return await Database.StringSetAsync(key, entryBytes, null, when, flag);
        }

        public bool Replace<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return Add(key, value, when, flag);
        }

        public Task<bool> ReplaceAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return AddAsync(key, value, when, flag);
        }

        public bool Add<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var entryBytes = Serializer.Serialize(value);
            if (maxValueLength > 0 && entryBytes.Length > maxValueLength)
                throw new ArgumentException("value cannot be longer than the MaxValueLength", nameof(value));

            var expiration = expiresAt.UtcDateTime.Subtract(DateTime.UtcNow);

            return Database.StringSet(key, entryBytes, expiration, when, flag);
        }

        public async Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var entryBytes = Serializer.Serialize(value);
            if (maxValueLength > 0 && entryBytes.Length > maxValueLength)
                throw new ArgumentException("value cannot be longer than the MaxValueLength", nameof(value));

            var expiration = expiresAt.UtcDateTime.Subtract(DateTime.UtcNow);

            return await Database.StringSetAsync(key, entryBytes, expiration, when, flag);
        }

        public bool Replace<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return Add(key, value, expiresAt, when, flag);
        }

        public Task<bool> ReplaceAsync<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return AddAsync(key, value, expiresAt, when, flag);
        }

        public bool Add<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var entryBytes = Serializer.Serialize(value);
            if (maxValueLength > 0 && entryBytes.Length > maxValueLength)
                throw new ArgumentException("value cannot be longer than the MaxValueLength", nameof(value));

            return Database.StringSet(key, entryBytes, expiresIn, when, flag);
        }

        public async Task<bool> AddAsync<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var entryBytes = Serializer.Serialize(value);
            if (maxValueLength > 0 && entryBytes.Length > maxValueLength)
                throw new ArgumentException("value cannot be longer than the MaxValueLength", nameof(value));

            return await Database.StringSetAsync(key, entryBytes, expiresIn, when, flag);
        }

        public bool Replace<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return Add(key, value, expiresIn, when, flag);
        }

        public Task<bool> ReplaceAsync<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return AddAsync(key, value, expiresIn, when, flag);
        }

        public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys)
        {
            var redisKeys = keys.Select(x => (RedisKey)x).ToArray();
            var result = Database.StringGet(redisKeys);

            var dict = new Dictionary<string, T>(StringComparer.Ordinal);
            for (var index = 0; index < redisKeys.Length; index++)
            {
                var value = result[index];
                dict.Add(redisKeys[index], value == RedisValue.Null ? default : Serializer.Deserialize<T>(value));
            }

            return dict;
        }

        public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys, DateTimeOffset expiresAt)
        {
            var result = GetAll<T>(keys);
            UpdateExpiryAll(keys.ToArray(), expiresAt);
            return result;
        }

        public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys, TimeSpan expiresIn)
        {
            var result = GetAll<T>(keys);
            UpdateExpiryAll(keys.ToArray(), expiresIn);
            return result;
        }

        public async Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys)
        {
            var redisKeys = keys.Select(x => (RedisKey)x).ToArray();
            var result = await Database.StringGetAsync(redisKeys);
            var dict = new Dictionary<string, T>(StringComparer.Ordinal);
            for (var index = 0; index < redisKeys.Length; index++)
            {
                var value = result[index];
                dict.Add(redisKeys[index], value == RedisValue.Null ? default : Serializer.Deserialize<T>(value));
            }

            return dict;
        }

        public async Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys, DateTimeOffset expiresAt)
        {
            var result = await GetAllAsync<T>(keys);
            await UpdateExpiryAllAsync(keys.ToArray(), expiresAt);
            return result;
        }

        public async Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys, TimeSpan expiresIn)
        {
            var result = await GetAllAsync<T>(keys);
            await UpdateExpiryAllAsync(keys.ToArray(), expiresIn);
            return result;
        }

        public bool AddAll<T>(IList<Tuple<string, T>> items, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var values = GetItemsInMaxLengthLimit(items);

            return Database.StringSet(values, when, flag);
        }

        public async Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var values = GetItemsInMaxLengthLimit(items);

            return await Database.StringSetAsync(values, when, flag);
        }

        public bool AddAll<T>(IList<Tuple<string, T>> items, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var values = GetItemsInMaxLengthLimit(items);

            var result = Database.StringSet(values, when, flag);

            foreach (var value in values)
                Database.KeyExpire(value.Key, expiresAt.UtcDateTime, flag);

            return result;
        }

        public async Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var values = GetItemsInMaxLengthLimit(items);

            var result = await Database.StringSetAsync(values, when, flag);

            Parallel.ForEach(values, async value => await Database.KeyExpireAsync(value.Key, expiresAt.UtcDateTime, flag));

            return result;
        }

        public bool AddAll<T>(IList<Tuple<string, T>> items, TimeSpan expiresOn, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var values = GetItemsInMaxLengthLimit(items);

            var result = Database.StringSet(values, when, flag);

            foreach (var value in values)
                Database.KeyExpire(value.Key, expiresOn, flag);

            return result;
        }

        public async Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, TimeSpan expiresOn, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var values = GetItemsInMaxLengthLimit(items);

            var result = await Database.StringSetAsync(values, when, flag);

            Parallel.ForEach(values, async value => await Database.KeyExpireAsync(value.Key, expiresOn, flag));

            return result;
        }

        public bool SetAdd<T>(string key, T item, CommandFlags flag = CommandFlags.None) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key cannot be empty.", nameof(key));

            if (item == null)
                throw new ArgumentNullException(nameof(item), "item cannot be null.");

            var serializedObject = Serializer.Serialize(item);

            return Database.SetAdd(key, serializedObject, flag);
        }

        public async Task<bool> SetAddAsync<T>(string key, T item, CommandFlags flag = CommandFlags.None) where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty.", nameof(key));

            if (item == null) throw new ArgumentNullException(nameof(item), "item cannot be null.");

            var serializedObject = Serializer.Serialize(item);

            return await Database.SetAddAsync(key, serializedObject, flag);
        }

        public long SetAddAll<T>(string key, CommandFlags flag = CommandFlags.None, params T[] items) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key cannot be empty.", nameof(key));

            if (items == null)
                throw new ArgumentNullException(nameof(items), "items cannot be null.");

            if (items.Any(item => item == null))
                throw new ArgumentException("items cannot contains any null item.", nameof(items));

            return Database.SetAdd(key, items.Select(item => Serializer.Serialize(item)).Select(x => (RedisValue)x).ToArray(), flag);
        }


        public async Task<long> SetAddAllAsync<T>(string key, CommandFlags flag = CommandFlags.None, params T[] items) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key cannot be empty.", nameof(key));

            if (items == null) throw new ArgumentNullException(nameof(items), "items cannot be null.");

            if (items.Any(item => item == null)) throw new ArgumentException("items cannot contains any null item.", nameof(items));

            return await Database.SetAddAsync(key, items.Select(item => Serializer.Serialize(item)).Select(x => (RedisValue)x).ToArray(), flag);
        }

        public bool SetRemove<T>(string key, T item, CommandFlags flag = CommandFlags.None) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key cannot be empty.", nameof(key));

            if (item == null) throw new ArgumentNullException(nameof(item), "item cannot be null.");

            var serializedObject = Serializer.Serialize(item);

            return Database.SetRemove(key, serializedObject, flag);
        }

        public async Task<bool> SetRemoveAsync<T>(string key, T item, CommandFlags flag = CommandFlags.None) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key cannot be empty.", nameof(key));

            if (item == null) throw new ArgumentNullException(nameof(item), "item cannot be null.");

            var serializedObject = Serializer.Serialize(item);

            return await Database.SetRemoveAsync(key, serializedObject, flag);
        }

        public long SetRemoveAll<T>(string key, CommandFlags flag = CommandFlags.None, params T[] items) where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty.", nameof(key));

            if (items == null)
                throw new ArgumentNullException(nameof(items), "items cannot be null.");

            if (items.Any(item => item == null))
                throw new ArgumentException("items cannot contains any null item.", nameof(items));

            return Database.SetRemove(key, items.Select(item => Serializer.Serialize(item)).Select(x => (RedisValue)x).ToArray(), flag);
        }

        public async Task<long> SetRemoveAllAsync<T>(string key, CommandFlags flag = CommandFlags.None, params T[] items) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key cannot be empty.", nameof(key));

            if (items == null) throw new ArgumentNullException(nameof(items), "items cannot be null.");

            if (items.Any(item => item == null)) throw new ArgumentException("items cannot contains any null item.", nameof(items));

            return await Database.SetRemoveAsync(key, items.Select(item => Serializer.Serialize(item)).Select(x => (RedisValue)x).ToArray(), flag);
        }

        public string[] SetMember(string memberName, CommandFlags flag = CommandFlags.None)
        {
            return Database.SetMembers(memberName, flag).Select(x => x.ToString()).ToArray();
        }

        public async Task<string[]> SetMemberAsync(string memberName, CommandFlags flag = CommandFlags.None)
        {
            return (await Database.SetMembersAsync(memberName, flag)).Select(x => x.ToString()).ToArray();
        }

        public IEnumerable<T> SetMembers<T>(string key, CommandFlags flag = CommandFlags.None)
        {
            var members = Database.SetMembers(key, flag);
            return members.Select(m => m == RedisValue.Null ? default : Serializer.Deserialize<T>(m));
        }

        public async Task<IEnumerable<T>> SetMembersAsync<T>(string key, CommandFlags flag = CommandFlags.None)
        {
            var members = await Database.SetMembersAsync(key, flag);

            return members.Select(m => m == RedisValue.Null ? default : Serializer.Deserialize<T>(m));
        }

        public IEnumerable<string> SearchKeys(string pattern)
        {
            pattern = $"{keyprefix}{pattern}";
            var keys = new HashSet<string>();

            var multiplexer = Database.Multiplexer;
            var servers = ServerIteratorFactory.GetServers(connectionMultiplexer, serverEnumerationStrategy).ToArray();

            if (!servers.Any())
                throw new Exception("No server found to serve the KEYS command.");

            foreach (var server in servers)
            {
                var nextCursor = 0;
                do
                {
                    var redisResult = Database.Execute("SCAN", nextCursor.ToString(), "MATCH", pattern, "COUNT", "1000");
                    var innerResult = (RedisResult[])redisResult;

                    nextCursor = int.Parse((string)innerResult[0]);

                    var resultLines = ((string[])innerResult[1]).ToList();

                    keys.UnionWith(resultLines);
                } while (nextCursor != 0);
            }

            return !string.IsNullOrEmpty(keyprefix) ? keys.Select(k => k.Substring(keyprefix.Length)) : keys;
        }

        public Task<IEnumerable<string>> SearchKeysAsync(string pattern)
        {
            return Task.Factory.StartNew(() => SearchKeys(pattern));
        }

        public void FlushDb()
        {
            var endPoints = Database.Multiplexer.GetEndPoints();
            foreach (var endpoint in endPoints)
            {
                var server = Database.Multiplexer.GetServer(endpoint);

                if (!server.IsSlave)
                    server.FlushDatabase(Database.Database);
            }
        }

        public async Task FlushDbAsync()
        {
            var endPoints = Database.Multiplexer.GetEndPoints();

            foreach (var endpoint in endPoints)
            {
                var server = Database.Multiplexer.GetServer(endpoint);

                if (!server.IsSlave)
                    await server.FlushDatabaseAsync(Database.Database);
            }
        }

        public void Save(SaveType saveType, CommandFlags flags = CommandFlags.None)
        {
            var endPoints = Database.Multiplexer.GetEndPoints();

            foreach (var endpoint in endPoints)
                Database.Multiplexer.GetServer(endpoint).Save(saveType, flags);
        }

        public async Task SaveAsync(SaveType saveType, CommandFlags flags = CommandFlags.None)
        {
            var endPoints = Database.Multiplexer.GetEndPoints();

            foreach (var endpoint in endPoints)
                await Database.Multiplexer.GetServer(endpoint).SaveAsync(saveType, flags);
        }

        public Dictionary<string, string> GetInfo()
        {
            var info = Database.ScriptEvaluate("return redis.call('INFO')").ToString();

            return ParseInfo(info);
        }

        public async Task<Dictionary<string, string>> GetInfoAsync()
        {
            var info = (await Database.ScriptEvaluateAsync("return redis.call('INFO')")).ToString();

            return ParseInfo(info);
        }

        public List<InfoDetail> GetInfoCategorized()
        {
            var info = Database.ScriptEvaluate("return redis.call('INFO')").ToString();

            return ParseCategorizedInfo(info);
        }

        public async Task<List<InfoDetail>> GetInfoCategorizedAsync()
        {
            var info = (await Database.ScriptEvaluateAsync("return redis.call('INFO')")).ToString();

            return ParseCategorizedInfo(info);
        }

        public long Publish<T>(RedisChannel channel, T message, CommandFlags flags = CommandFlags.None)
        {
            var sub = connectionMultiplexer.GetSubscriber();
            return sub.Publish(channel, Serializer.Serialize(message), flags);
        }

        public async Task<long> PublishAsync<T>(RedisChannel channel, T message, CommandFlags flags = CommandFlags.None)
        {
            var sub = connectionMultiplexer.GetSubscriber();
            return await sub.PublishAsync(channel, Serializer.Serialize(message), flags);
        }

        public void Subscribe<T>(RedisChannel channel, Action<T> handler, CommandFlags flags = CommandFlags.None)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var sub = connectionMultiplexer.GetSubscriber();
            sub.Subscribe(channel, (redisChannel, value) => handler(Serializer.Deserialize<T>(value)), flags);
        }

        public async Task SubscribeAsync<T>(RedisChannel channel, Func<T, Task> handler, CommandFlags flags = CommandFlags.None)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var sub = connectionMultiplexer.GetSubscriber();
            await
                sub.SubscribeAsync(channel, async (redisChannel, value) => await handler(Serializer.Deserialize<T>(value)), flags);
        }

        public void Unsubscribe<T>(RedisChannel channel, Action<T> handler, CommandFlags flags = CommandFlags.None)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var sub = connectionMultiplexer.GetSubscriber();
            sub.Unsubscribe(channel, (redisChannel, value) => handler(Serializer.Deserialize<T>(value)), flags);
        }

        public async Task UnsubscribeAsync<T>(RedisChannel channel, Func<T, Task> handler, CommandFlags flags = CommandFlags.None)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var sub = connectionMultiplexer.GetSubscriber();
            await sub.UnsubscribeAsync(channel, (redisChannel, value) => handler(Serializer.Deserialize<T>(value)), flags);
        }

        public void UnsubscribeAll(CommandFlags flags = CommandFlags.None)
        {
            var sub = connectionMultiplexer.GetSubscriber();
            sub.UnsubscribeAll(flags);
        }

        public async Task UnsubscribeAllAsync(CommandFlags flags = CommandFlags.None)
        {
            var sub = connectionMultiplexer.GetSubscriber();
            await sub.UnsubscribeAllAsync(flags);
        }

        public long ListAddToLeft<T>(string key, T item, When when = When.Always, CommandFlags flags = CommandFlags.None) where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty.", nameof(key));

            if (item == null)
                throw new ArgumentNullException(nameof(item), "item cannot be null.");

            var serializedItem = Serializer.Serialize(item);

            return Database.ListLeftPush(key, serializedItem);
        }

        public async Task<long> ListAddToLeftAsync<T>(string key, T item, When when = When.Always, CommandFlags flags = CommandFlags.None) where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty.", nameof(key));

            if (item == null)
                throw new ArgumentNullException(nameof(item), "item cannot be null.");

            var serializedItem = Serializer.Serialize(item);

            return await Database.ListLeftPushAsync(key, serializedItem, when, flags);
        }

        public T ListGetFromRight<T>(string key, CommandFlags flags = CommandFlags.None) where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty.", nameof(key));

            var item = Database.ListRightPop(key, flags);

            return item == RedisValue.Null ? null : Serializer.Deserialize<T>(item);
        }

        public async Task<T> ListGetFromRightAsync<T>(string key, CommandFlags flags = CommandFlags.None) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key cannot be empty.", nameof(key));

            var item = await Database.ListRightPopAsync(key, flags);

            if (item == RedisValue.Null) return null;

            return item == RedisValue.Null ? null : Serializer.Deserialize<T>(item);
        }

        public bool HashDelete(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
        {
            return Database.HashDelete(hashKey, key, commandFlags);
        }

        public long HashDelete(string hashKey, IEnumerable<string> keys, CommandFlags commandFlags = CommandFlags.None)
        {
            return Database.HashDelete(hashKey, keys.Select(x => (RedisValue)x).ToArray(), commandFlags);
        }

        public bool HashExists(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
        {
            return Database.HashExists(hashKey, key, commandFlags);
        }

        public T HashGet<T>(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
        {
            var redisValue = Database.HashGet(hashKey, key, commandFlags);
            return redisValue.HasValue ? Serializer.Deserialize<T>(redisValue) : default;
        }

        public Dictionary<string, T> HashGet<T>(string hashKey, IEnumerable<string> keys, CommandFlags commandFlags = CommandFlags.None)
        {
            return keys.Select(x => new { key = x, value = HashGet<T>(hashKey, x, commandFlags) })
                .ToDictionary(kv => kv.key, kv => kv.value, StringComparer.Ordinal);
        }

        public Dictionary<string, T> HashGetAll<T>(string hashKey, CommandFlags commandFlags = CommandFlags.None)
        {
            return Database
                .HashGetAll(hashKey, commandFlags)
                .ToDictionary(
                    x => x.Name.ToString(),
                    x => Serializer.Deserialize<T>(x.Value),
                    StringComparer.Ordinal);
        }

        public long HashIncerementBy(string hashKey, string key, long value, CommandFlags commandFlags = CommandFlags.None)
        {
            return Database.HashIncrement(hashKey, key, value, commandFlags);
        }

        public double HashIncerementBy(string hashKey, string key, double value, CommandFlags commandFlags = CommandFlags.None)
        {
            return Database.HashIncrement(hashKey, key, value, commandFlags);
        }

        public IEnumerable<string> HashKeys(string hashKey, CommandFlags commandFlags = CommandFlags.None)
        {
            return Database.HashKeys(hashKey, commandFlags).Select(x => x.ToString());
        }

        public long HashLength(string hashKey, CommandFlags commandFlags = CommandFlags.None)
        {
            return Database.HashLength(hashKey, commandFlags);
        }

        public bool HashSet<T>(string hashKey, string key, T value, bool nx = false, CommandFlags commandFlags = CommandFlags.None)
        {
            return Database.HashSet(hashKey, key, Serializer.Serialize(value), nx ? When.NotExists : When.Always, commandFlags);
        }

        public void HashSet<T>(string hashKey, Dictionary<string, T> values, CommandFlags commandFlags = CommandFlags.None)
        {
            var entries = values.Select(kv => new HashEntry(kv.Key, Serializer.Serialize(kv.Value)));
            Database.HashSet(hashKey, entries.ToArray(), commandFlags);
        }

        public IEnumerable<T> HashValues<T>(string hashKey, CommandFlags commandFlags = CommandFlags.None)
        {
            return Database.HashValues(hashKey, commandFlags).Select(x => Serializer.Deserialize<T>(x));
        }

        public Dictionary<string, T> HashScan<T>(string hashKey, string pattern, int pageSize = 10, CommandFlags commandFlags = CommandFlags.None)
        {
            return Database
                .HashScan(hashKey, pattern, pageSize, commandFlags)
                .ToDictionary(x => x.Name.ToString(),
                    x => Serializer.Deserialize<T>(x.Value),
                    StringComparer.Ordinal);
        }

        public async Task<bool> HashDeleteAsync(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
        {
            return await Database.HashDeleteAsync(hashKey, key, commandFlags);
        }

        public async Task<long> HashDeleteAsync(string hashKey, IEnumerable<string> keys, CommandFlags commandFlags = CommandFlags.None)
        {
            return await Database.HashDeleteAsync(hashKey, keys.Select(x => (RedisValue)x).ToArray(), commandFlags);
        }

        public async Task<bool> HashExistsAsync(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
        {
            return await Database.HashExistsAsync(hashKey, key, commandFlags);
        }

        public async Task<T> HashGetAsync<T>(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
        {
            var redisValue = await Database.HashGetAsync(hashKey, key, commandFlags);
            return redisValue.HasValue ? Serializer.Deserialize<T>(redisValue) : default;
        }

        public async Task<Dictionary<string, T>> HashGetAsync<T>(string hashKey, IEnumerable<string> keys, CommandFlags commandFlags = CommandFlags.None)
        {
            var result = new Dictionary<string, T>();
            foreach (var key in keys)
            {
                var value = await HashGetAsync<T>(hashKey, key, commandFlags);

                result.Add(key, value);
            }

            return result;
        }

        public async Task<Dictionary<string, T>> HashGetAllAsync<T>(string hashKey, CommandFlags commandFlags = CommandFlags.None)
        {
            return (await Database
                    .HashGetAllAsync(hashKey, commandFlags))
                .ToDictionary(
                    x => x.Name.ToString(),
                    x => Serializer.Deserialize<T>(x.Value),
                    StringComparer.Ordinal);
        }

        public async Task<long> HashIncerementByAsync(string hashKey, string key, long value, CommandFlags commandFlags = CommandFlags.None)
        {
            return await Database.HashIncrementAsync(hashKey, key, value, commandFlags);
        }

        public async Task<double> HashIncerementByAsync(string hashKey, string key, double value, CommandFlags commandFlags = CommandFlags.None)
        {
            return await Database.HashIncrementAsync(hashKey, key, value, commandFlags);
        }

        public async Task<IEnumerable<string>> HashKeysAsync(string hashKey, CommandFlags commandFlags = CommandFlags.None)
        {
            return (await Database.HashKeysAsync(hashKey, commandFlags)).Select(x => x.ToString());
        }

        public async Task<long> HashLengthAsync(string hashKey, CommandFlags commandFlags = CommandFlags.None)
        {
            return await Database.HashLengthAsync(hashKey, commandFlags);
        }

        public async Task<bool> HashSetAsync<T>(string hashKey, string key, T value, bool nx = false, CommandFlags commandFlags = CommandFlags.None)
        {
            return await Database.HashSetAsync(hashKey, key, Serializer.Serialize(value), nx ? When.NotExists : When.Always, commandFlags);
        }

        public async Task HashSetAsync<T>(string hashKey, IDictionary<string, T> values, CommandFlags commandFlags = CommandFlags.None)
        {
            var entries = values.Select(kv => new HashEntry(kv.Key, Serializer.Serialize(kv.Value)));
            await Database.HashSetAsync(hashKey, entries.ToArray(), commandFlags);
        }

        public async Task<IEnumerable<T>> HashValuesAsync<T>(string hashKey, CommandFlags commandFlags = CommandFlags.None)
        {
            return (await Database.HashValuesAsync(hashKey, commandFlags)).Select(x => Serializer.Deserialize<T>(x));
        }

        public async Task<Dictionary<string, T>> HashScanAsync<T>(string hashKey, string pattern, int pageSize = 10, CommandFlags commandFlags = CommandFlags.None)
        {
            return (await Task.Run(() => Database.HashScan(hashKey, pattern, pageSize, commandFlags)))
                .ToDictionary(x => x.Name.ToString(), x => Serializer.Deserialize<T>(x.Value), StringComparer.Ordinal);
        }

        //public async Task HashSetFromModelAsync<T>(string hashKey, T entity, CommandFlags commandFlags = CommandFlags.None)
        //{
        //    PropertyInfo[] properties = entity.GetType().GetProperties();
        //    var entries = properties.Where(c => c.CanRead && c.CanWrite && c.PropertyType.IsPublic)
        //        .Select(property => new HashEntry(property.Name, Serializer.Serialize(property.GetValue(entity))));
        //    await Database.HashSetAsync(hashKey, entries.ToArray(), commandFlags);
        //}

        public async Task HashSetFromModelAsync<T>(string hashKey, T entity, CommandFlags commandFlags = CommandFlags.None)
        {
            PropertyInfo[] properties = entity.GetType().GetProperties();
            var entries = properties.Where(c => c.CanRead && c.CanWrite && c.PropertyType.IsPublic)
                .Select(prop => new HashEntry(prop.Name, Serializer.Serialize(prop.GetValue(entity))));
            await Database.HashSetAsync(hashKey, entries.ToArray(), commandFlags);
        }

        public async Task<T> HashGetToModelAsync<T>(string hashKey, CommandFlags commandFlags = CommandFlags.None)
        {
            var items = (await Database.HashGetAllAsync(hashKey, commandFlags))
                .ToDictionary(x => x.Name.ToString(), x => x.Value, StringComparer.Ordinal);

            PropertyInfo[] properties = typeof(T).GetProperties();
            var obj = Activator.CreateInstance(typeof(T));
            foreach (var prop in properties)
            {
                if (prop.CanWrite == false || prop.PropertyType.IsPublic == false) continue;

                if (items.TryGetValue(prop.Name, out RedisValue value))
                {
                    if (value.IsNull == false)
                    {
                        prop.SetValue(obj, Convert.ChangeType(Serializer.Deserialize(value, prop.PropertyType), prop.PropertyType));
                    }
                }
            }
            return (T)obj;
        }

        public async Task<T> HashGetToModelAsync<T>(string hashKey, string[] fields, CommandFlags commandFlags = CommandFlags.None)
        {
            RedisValue[] hashFields = new RedisValue[fields.Length];
            for (int i = 0, l = fields.Length; i < l; i++) hashFields[i] = fields[i];

            var items = await Database.HashGetAsync(hashKey, hashFields, commandFlags);

            PropertyInfo[] props = typeof(T).GetProperties();
            var obj = Activator.CreateInstance(typeof(T));
            for (int i = 0, l = fields.Length; i < l; i++)
            {
                var prop = props.FirstOrDefault(c => c.Name == fields[i]);
                if (prop == null || prop.CanWrite == false || prop.PropertyType.IsPublic == false) continue;
                if (items[i].IsNull == false) {
                    prop.SetValue(obj, Convert.ChangeType(Serializer.Deserialize(items[i], prop.PropertyType), prop.PropertyType));
                }
            }

            return (T)obj;
        }

        public bool UpdateExpiry(string key, DateTimeOffset expiresAt, CommandFlags flags = CommandFlags.None)
        {
            if (Database.KeyExists(key))
                return Database.KeyExpire(key, expiresAt.UtcDateTime.Subtract(DateTime.UtcNow), flags);

            return false;
        }

        public bool UpdateExpiry(string key, TimeSpan expiresIn, CommandFlags flags = CommandFlags.None)
        {
            if (Database.KeyExists(key))
                return Database.KeyExpire(key, expiresIn, flags);

            return false;
        }

        public async Task<bool> UpdateExpiryAsync(string key, DateTimeOffset expiresAt, CommandFlags flags = CommandFlags.None)
        {
            if (await Database.KeyExistsAsync(key))
                return await Database.KeyExpireAsync(key, expiresAt.UtcDateTime.Subtract(DateTime.UtcNow), flags);

            return false;
        }

        public async Task<bool> UpdateExpiryAsync(string key, TimeSpan expiresIn, CommandFlags flags = CommandFlags.None)
        {
            if (await Database.KeyExistsAsync(key))
                return await Database.KeyExpireAsync(key, expiresIn, flags);

            return false;
        }

        public IDictionary<string, bool> UpdateExpiryAll(string[] keys, DateTimeOffset expiresAt, CommandFlags flags = CommandFlags.None)
        {
            var results = new Dictionary<string, bool>(StringComparer.Ordinal);

            for (var i = 0; i < keys.Length; i++)
                results.Add(keys[i], UpdateExpiry(keys[i], expiresAt.UtcDateTime, flags));

            return results;
        }

        public IDictionary<string, bool> UpdateExpiryAll(string[] keys, TimeSpan expiresIn, CommandFlags flags = CommandFlags.None)
        {
            var results = new Dictionary<string, bool>(StringComparer.Ordinal);

            for (var i = 0; i < keys.Length; i++)
                results.Add(keys[i], UpdateExpiry(keys[i], expiresIn, flags));

            return results;
        }

        public async Task<IDictionary<string, bool>> UpdateExpiryAllAsync(string[] keys, DateTimeOffset expiresAt, CommandFlags flags = CommandFlags.None)
        {
            var results = new Dictionary<string, bool>(StringComparer.Ordinal);

            for (var i = 0; i < keys.Length; i++)
                results.Add(keys[i], await UpdateExpiryAsync(keys[i], expiresAt.UtcDateTime, flags));

            return results;
        }

        public async Task<IDictionary<string, bool>> UpdateExpiryAllAsync(string[] keys, TimeSpan expiresIn, CommandFlags flags = CommandFlags.None)
        {
            var results = new Dictionary<string, bool>(StringComparer.Ordinal);

            for (var i = 0; i < keys.Length; i++)
                results.Add(keys[i], await UpdateExpiryAsync(keys[i], expiresIn, flags));

            return results;
        }

        public bool SortedSetAdd(string key, string member, double score, CommandFlags commandFlags = CommandFlags.None)
        {
            var entryBytes = encoding.GetBytes(member);

            return Database.SortedSetAdd(key, entryBytes, score, commandFlags);
        }

        public async Task<bool> SortedSetAddAsync(string key, string member, double score, CommandFlags commandFlags = CommandFlags.None)
        {
            var entryBytes = encoding.GetBytes(member);

            return await Database.SortedSetAddAsync(key, entryBytes, score, commandFlags);
        }

        public bool SortedSetRemove(string key, string member, CommandFlags commandFlags = CommandFlags.None)
        {
            var entryBytes = encoding.GetBytes(member);

            return Database.SortedSetRemove(key, entryBytes, commandFlags);
        }

        public async Task<bool> SortedSetRemoveAsync(string key, string member, CommandFlags commandFlags = CommandFlags.None)
        {
            var entryBytes = encoding.GetBytes(member);

            return await Database.SortedSetRemoveAsync(key, entryBytes, commandFlags);
        }

        public IEnumerable<string> SortedSetRangeByScore(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0L,
            long take = -1L, CommandFlags commandFlags = CommandFlags.None)
        {
            var result = Database.SortedSetRangeByScore(key, start, stop, exclude, order, skip, take, commandFlags);

            return result.Select(m => m == RedisValue.Null ? string.Empty : (string)m);
        }

        public async Task<IEnumerable<string>> SortedSetRangeByScoreAsync(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, Exclude exclude = Exclude.None, Order order = Order.Ascending,
            long skip = 0L,
            long take = -1L, CommandFlags commandFlags = CommandFlags.None)
        {
            var result = await Database.SortedSetRangeByScoreAsync(key, start, stop, exclude, order, skip, take, commandFlags);

            return result.Select(m => m == RedisValue.Null ? string.Empty : (string)m);
        }

        public async Task<IEnumerable<string>> SortedSetRangeByRankAsync(string key, long start, long stop, Order order = Order.Ascending, CommandFlags commandFlags = CommandFlags.None)
        {
            var result = await Database.SortedSetRangeByRankAsync(key, start, stop, order, commandFlags);
            
            return result.Select(m => m == RedisValue.Null ? string.Empty : (string)m);
        }

        public void Dispose()
        {
            connectionMultiplexer?.Dispose();
        }

        private Dictionary<string, string> ParseInfo(string info)
        {
            // Call Parse Categorized Info to cut back on duplicated code.
            var data = ParseCategorizedInfo(info);
            // Return a dictionary of the Info Key and Info value 
            return data.ToDictionary(x => x.Key, x => x.InfoValue);
        }

        private KeyValuePair<RedisKey, RedisValue>[] GetItemsInMaxLengthLimit<T>(IList<Tuple<string, T>> values)
        {
            if (maxValueLength == default)
                return values
                    .Select(item => new KeyValuePair<RedisKey, RedisValue>(item.Item1, Serializer.Serialize(item.Item2)))
                    .ToArray();

            return GetValuesInLengthLimitIterator(values).ToArray();
        }

        private IEnumerable<KeyValuePair<RedisKey, RedisValue>> GetValuesInLengthLimitIterator<T>(IList<Tuple<string, T>> values)
        {
            foreach (var item in values)
            {
                var itemSerialized = Serializer.Serialize(item.Item2);
                if (itemSerialized.Length <= maxValueLength)
                    yield return new KeyValuePair<RedisKey, RedisValue>(item.Item1, itemSerialized);
                else
                    throw new ArgumentException("value cannot be longer than the MaxValueLength", nameof(item.Item2));
            }
        }

        private List<InfoDetail> ParseCategorizedInfo(string info)
        {
            var data = new List<InfoDetail>();
            string category = string.Empty;
            if (!string.IsNullOrWhiteSpace(info))
            {
                var lines = info.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    if (line[0] == '#')
                    {
                        category = line.Replace("#", string.Empty).Trim();
                        continue;
                    }

                    var idx = line.IndexOf(':');
                    if (idx > 0) // double check this line looks about right
                    {
                        var key = line.Substring(0, idx);
                        var infoValue = line.Substring(idx + 1).Trim();

                        data.Add(new InfoDetail { Category = category, Key = key, InfoValue = infoValue });
                    }
                }
            }
            return data;
        }
        /// <summary>
        ///     Add  the entry to a sorted set with  an incremen score 
        /// </summary>
        /// <remarks>
        ///     Time complexity: O(1)
        /// </remarks>
        /// <param name="key">Key of the set</param>
        /// <param name="member">The instance of T.</param>
        /// <param name="score">Score of the entry</param>
        /// <param name="commandFlags">Command execution flags</param>
        /// <returns>
        ///      if the object has been added return previous score. Otherwise return 0.0 when first add
        /// </returns>
        public double SortedSetAddIncrement(string key, string member, double score, CommandFlags commandFlags = CommandFlags.None)
        {
            var entryBytes = encoding.GetBytes(member);
            return Database.SortedSetIncrement(key, entryBytes, score, commandFlags);
        }

        /// <summary>
        ///     Add  the entry to a sorted set with  an incremen score 
        /// </summary>
        /// <remarks>
        ///     Time complexity: O(1)
        /// </remarks>
        /// <param name="key">Key of the set</param>
        /// <param name="member">The instance of T.</param>
        /// <param name="score">Score of the entry</param>
        /// <param name="commandFlags">Command execution flags</param>
        /// <returns>
        ///      if the object has been added return previous score. Otherwise return 0.0 when first add
        /// </returns>
        /// 
        public async Task<double> SortedSetAddIncrementAsync(string key, string member, double score, CommandFlags commandFlags = CommandFlags.None)
        {
            var entryBytes = encoding.GetBytes(member);
            return await Database.SortedSetIncrementAsync(key, entryBytes, score, commandFlags);
        }

        //public async Task RemoveKey(string key)
        //{
        //    if (await Database.KeyExistsAsync(key))
        //    {
        //        await Database.KeyExpireAsync(key, new TimeSpan(0, 0, 0), CommandFlags.FireAndForget);
        //    }
        //}

        public async Task<long> StringIncrementAsync(string key, long value, CommandFlags commandFlags = CommandFlags.None)
        {
            return await Database.StringIncrementAsync(key, value, commandFlags);
        }

        public async Task<double> StringIncrementAsync(string key, double value, CommandFlags commandFlags = CommandFlags.None)
        {
            return await Database.StringIncrementAsync(key, value, commandFlags);
        }

        #region Script
        public async Task<LoadedLuaScript> ScriptLoadAsync(string script, System.Net.EndPoint endPoint = null)
        {
            if (endPoint == null) endPoint = Database.Multiplexer.GetEndPoints().FirstOrDefault();
            var server = Database.Multiplexer.GetServer(endPoint);
            LuaScript luaScript = LuaScript.Prepare(script);
            return await luaScript.LoadAsync(server);
        }
        #endregion
    }
}