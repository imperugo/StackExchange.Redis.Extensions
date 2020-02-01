using System;
using System.Collections.Generic;
using System.Linq;
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
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            this.serverEnumerationStrategy = serverEnumerationStrategy ?? new ServerEnumerationStrategy();
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

        public Task<bool> ExistsAsync(string key, CommandFlags flags = CommandFlags.None)
        {
            return Database.KeyExistsAsync(key, flags);
        }

        public Task<bool> RemoveAsync(string key, CommandFlags flags = CommandFlags.None)
        {
            return Database.KeyDeleteAsync(key, flags);
        }

        public Task RemoveAllAsync(IEnumerable<string> keys, CommandFlags flags = CommandFlags.None)
        {
            var redisKeys = keys.Select(x => (RedisKey)x).ToArray();
            return Database.KeyDeleteAsync(redisKeys, flags);
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

        public async Task<bool> AddAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var entryBytes = Serializer.Serialize(value);
            if (maxValueLength > 0 && entryBytes.Length > maxValueLength)
                throw new ArgumentException("value cannot be longer than the MaxValueLength", nameof(value));

            return await Database.StringSetAsync(key, entryBytes, null, when, flag);
        }

        public Task<bool> ReplaceAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return AddAsync(key, value, when, flag);
        }

        public async Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var entryBytes = Serializer.Serialize(value);
            if (maxValueLength > 0 && entryBytes.Length > maxValueLength)
                throw new ArgumentException("value cannot be longer than the MaxValueLength", nameof(value));

            var expiration = expiresAt.UtcDateTime.Subtract(DateTime.UtcNow);

            return await Database.StringSetAsync(key, entryBytes, expiration, when, flag);
        }

        public Task<bool> ReplaceAsync<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return AddAsync(key, value, expiresAt, when, flag);
        }

        public async Task<bool> AddAsync<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var entryBytes = Serializer.Serialize(value);
            if (maxValueLength > 0 && entryBytes.Length > maxValueLength)
                throw new ArgumentException("value cannot be longer than the MaxValueLength", nameof(value));

            return await Database.StringSetAsync(key, entryBytes, expiresIn, when, flag);
        }

        public Task<bool> ReplaceAsync<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return AddAsync(key, value, expiresIn, when, flag);
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

        public async Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var values = GetItemsInMaxLengthLimit(items);

            return await Database.StringSetAsync(values, when, flag);
        }

        public async Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var values = GetItemsInMaxLengthLimit(items);

            var result = await Database.StringSetAsync(values, when, flag);

            Parallel.ForEach(values, async value => await Database.KeyExpireAsync(value.Key, expiresAt.UtcDateTime, flag));

            return result;
        }

        public async Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, TimeSpan expiresOn, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var values = GetItemsInMaxLengthLimit(items);

            var result = await Database.StringSetAsync(values, when, flag);

            Parallel.ForEach(values, async value => await Database.KeyExpireAsync(value.Key, expiresOn, flag));

            return result;
        }

        public async Task<bool> SetAddAsync<T>(string key, T item, CommandFlags flag = CommandFlags.None) where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty.", nameof(key));

            if (item == null) throw new ArgumentNullException(nameof(item), "item cannot be null.");

            var serializedObject = Serializer.Serialize(item);

            return await Database.SetAddAsync(key, serializedObject, flag);
        }

        public async Task<long> SetAddAllAsync<T>(string key, CommandFlags flag = CommandFlags.None, params T[] items) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key cannot be empty.", nameof(key));

            if (items == null) throw new ArgumentNullException(nameof(items), "items cannot be null.");

            if (items.Any(item => item == null)) throw new ArgumentException("items cannot contains any null item.", nameof(items));

            return await Database.SetAddAsync(key, items.Select(item => Serializer.Serialize(item)).Select(x => (RedisValue)x).ToArray(), flag);
        }

        public async Task<bool> SetRemoveAsync<T>(string key, T item, CommandFlags flag = CommandFlags.None) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key cannot be empty.", nameof(key));

            if (item == null) throw new ArgumentNullException(nameof(item), "item cannot be null.");

            var serializedObject = Serializer.Serialize(item);

            return await Database.SetRemoveAsync(key, serializedObject, flag);
        }

        public async Task<long> SetRemoveAllAsync<T>(string key, CommandFlags flag = CommandFlags.None, params T[] items) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key cannot be empty.", nameof(key));

            if (items == null) throw new ArgumentNullException(nameof(items), "items cannot be null.");

            if (items.Any(item => item == null)) throw new ArgumentException("items cannot contains any null item.", nameof(items));

            return await Database.SetRemoveAsync(key, items.Select(item => Serializer.Serialize(item)).Select(x => (RedisValue)x).ToArray(), flag);
        }

        public async Task<string[]> SetMemberAsync(string memberName, CommandFlags flag = CommandFlags.None)
        {
            return (await Database.SetMembersAsync(memberName, flag)).Select(x => x.ToString()).ToArray();
        }

        public async Task<IEnumerable<T>> SetMembersAsync<T>(string key, CommandFlags flag = CommandFlags.None)
        {
            var members = await Database.SetMembersAsync(key, flag);

            return members.Select(m => m == RedisValue.Null ? default : Serializer.Deserialize<T>(m));
        }

        public Task<IEnumerable<string>> SearchKeysAsync(string pattern)
        {
            return Task.Factory.StartNew(() => SearchKeys(pattern));
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

        public async Task SaveAsync(SaveType saveType, CommandFlags flags = CommandFlags.None)
        {
            var endPoints = Database.Multiplexer.GetEndPoints();

            foreach (var endpoint in endPoints)
                await Database.Multiplexer.GetServer(endpoint).SaveAsync(saveType, flags);
        }

        public async Task<Dictionary<string, string>> GetInfoAsync()
        {
            var info = (await Database.ScriptEvaluateAsync("return redis.call('INFO')")).ToString();

            return ParseInfo(info);
        }

        public async Task<List<InfoDetail>> GetInfoCategorizedAsync()
        {
            var info = (await Database.ScriptEvaluateAsync("return redis.call('INFO')")).ToString();

            return ParseCategorizedInfo(info);
        }

        public async Task<long> PublishAsync<T>(RedisChannel channel, T message, CommandFlags flags = CommandFlags.None)
        {
            var sub = connectionMultiplexer.GetSubscriber();
            return await sub.PublishAsync(channel, Serializer.Serialize(message), flags);
        }

        public async Task SubscribeAsync<T>(RedisChannel channel, Func<T, Task> handler, CommandFlags flags = CommandFlags.None)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var sub = connectionMultiplexer.GetSubscriber();
            await
                sub.SubscribeAsync(channel, async (redisChannel, value) => await handler(Serializer.Deserialize<T>(value)), flags);
        }

        public async Task UnsubscribeAsync<T>(RedisChannel channel, Func<T, Task> handler, CommandFlags flags = CommandFlags.None)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var sub = connectionMultiplexer.GetSubscriber();
            await sub.UnsubscribeAsync(channel, (redisChannel, value) => handler(Serializer.Deserialize<T>(value)), flags);
        }

        public async Task UnsubscribeAllAsync(CommandFlags flags = CommandFlags.None)
        {
            var sub = connectionMultiplexer.GetSubscriber();
            await sub.UnsubscribeAllAsync(flags);
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

        public async Task<T> ListGetFromRightAsync<T>(string key, CommandFlags flags = CommandFlags.None) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key cannot be empty.", nameof(key));

            var item = await Database.ListRightPopAsync(key, flags);

            if (item == RedisValue.Null) return null;

            return item == RedisValue.Null ? null : Serializer.Deserialize<T>(item);
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

        public async Task<bool> SortedSetAddAsync<T>(string key, T value, double score, CommandFlags commandFlags = CommandFlags.None)
        {
            var entryBytes = Serializer.Serialize(value);

            return await Database.SortedSetAddAsync(key, entryBytes, score, commandFlags);
        }

        public async Task<bool> SortedSetRemoveAsync<T>(string key, T value, CommandFlags commandFlags = CommandFlags.None)
        {
            var entryBytes = Serializer.Serialize(value);

            return await Database.SortedSetRemoveAsync(key, entryBytes, commandFlags);
        }

        public IEnumerable<T> SortedSetRangeByScore<T>(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0L,
            long take = -1L, CommandFlags commandFlags = CommandFlags.None)
        {
            var result = Database.SortedSetRangeByScore(key, start, stop, exclude, order, skip, take, commandFlags);

            return result.Select(m => m == RedisValue.Null ? default : Serializer.Deserialize<T>(m));
        }

        public async Task<IEnumerable<T>> SortedSetRangeByScoreAsync<T>(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, Exclude exclude = Exclude.None, Order order = Order.Ascending,
            long skip = 0L,
            long take = -1L, CommandFlags commandFlags = CommandFlags.None)
        {
            var result = await Database.SortedSetRangeByScoreAsync(key, start, stop, exclude, order, skip, take, commandFlags);

            return result.Select(m => m == RedisValue.Null ? default : Serializer.Deserialize<T>(m));
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
            var category = string.Empty;
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
        /// <param name="value">The instance of T.</param>
        /// <param name="score">Score of the entry</param>
        /// <param name="commandFlags">Command execution flags</param>
        /// <returns>
        ///      if the object has been added return previous score. Otherwise return 0.0 when first add
        /// </returns>
        public double SortedSetAddIncrement<T>(string key, T value, double score, CommandFlags commandFlags = CommandFlags.None)
        {
            var entryBytes = Serializer.Serialize(value);
            return Database.SortedSetIncrement(key, entryBytes, score, commandFlags);
        }

        /// <summary>
        ///     Add  the entry to a sorted set with  an incremen score 
        /// </summary>
        /// <remarks>
        ///     Time complexity: O(1)
        /// </remarks>
        /// <param name="key">Key of the set</param>
        /// <param name="value">The instance of T.</param>
        /// <param name="score">Score of the entry</param>
        /// <param name="commandFlags">Command execution flags</param>
        /// <returns>
        ///      if the object has been added return previous score. Otherwise return 0.0 when first add
        /// </returns>
        /// 
        public async Task<double> SortedSetAddIncrementAsync<T>(string key, T value, double score, CommandFlags commandFlags = CommandFlags.None)
        {
            var entryBytes = Serializer.Serialize(value);
            return await Database.SortedSetIncrementAsync(key, entryBytes, score, commandFlags);
        }
    }
}
