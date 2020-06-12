using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Extensions;
using StackExchange.Redis.Extensions.Core.Models;
using StackExchange.Redis.Extensions.Core.ServerIteration;
using StackExchange.Redis.KeyspaceIsolation;

namespace StackExchange.Redis.Extensions.Core.Implementations
{
    /// <inheritdoc/>
    public partial class RedisDatabase : IRedisDatabase
    {
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly ServerEnumerationStrategy serverEnumerationStrategy = new ServerEnumerationStrategy();
        private readonly string keyprefix;
        private readonly uint maxValueLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisDatabase"/> class.
        /// </summary>
        /// <param name="connectionMultiplexer">The connection.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="serverEnumerationStrategy">The server enumeration strategy.</param>
        /// <param name="database">The StackExchange.Redis.Database.</param>
        /// <param name="maxvalueLength">The max lenght of the cache object.</param>
        /// <param name="keyPrefix">The key prefix.</param>
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

        /// <inheritdoc/>
        public IDatabase Database { get; }

        /// <inheritdoc/>
        public ISerializer Serializer { get; }

        /// <inheritdoc/>
        public Task<bool> ExistsAsync(string key, CommandFlags flags = CommandFlags.None)
        {
            return Database.KeyExistsAsync(key, flags);
        }

        /// <inheritdoc/>
        public Task<bool> RemoveAsync(string key, CommandFlags flags = CommandFlags.None)
        {
            return Database.KeyDeleteAsync(key, flags);
        }

        /// <inheritdoc/>
        public Task<long> RemoveAllAsync(IEnumerable<string> keys, CommandFlags flags = CommandFlags.None)
        {
            var redisKeys = keys.Select(x => (RedisKey)x).ToArray();
            return Database.KeyDeleteAsync(redisKeys, flags);
        }

        /// <inheritdoc/>
        public async Task<T> GetAsync<T>(string key, CommandFlags flag = CommandFlags.None)
        {
            var valueBytes = await Database.StringGetAsync(key, flag).ConfigureAwait(false);

            if (!valueBytes.HasValue)
                return default;

            return Serializer.Deserialize<T>(valueBytes);
        }

        /// <inheritdoc/>
        public async Task<T> GetAsync<T>(string key, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None)
        {
            var result = await GetAsync<T>(key, flag).ConfigureAwait(false);

            if (!Equals(result, default(T)))
                await Database.KeyExpireAsync(key, expiresAt.UtcDateTime.Subtract(DateTime.UtcNow)).ConfigureAwait(false);

            return result;
        }

        /// <inheritdoc/>
        public async Task<T> GetAsync<T>(string key, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None)
        {
            var result = await GetAsync<T>(key, flag).ConfigureAwait(false);

            if (!Equals(result, default(T)))
                await Database.KeyExpireAsync(key, expiresIn).ConfigureAwait(false);

            return result;
        }

        /// <inheritdoc/>
        public Task<bool> AddAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None, HashSet<string> tags = null)
        {
            var entryBytes = value.OfValueSize(Serializer, maxValueLength, key);

            if (tags?.Any() == true)
            {
                return ExecuteAddWithTags(key, value, tags, db => db.StringSetAsync(key, entryBytes, null, when, flag), null, when, flag);
            }

            return Database.StringSetAsync(key, entryBytes, null, when, flag);
        }

        /// <inheritdoc/>
        public Task<bool> ReplaceAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return AddAsync(key, value, when, flag);
        }

        /// <inheritdoc/>
        public Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None, HashSet<string> tags = null)
        {
            var entryBytes = value.OfValueSize(Serializer, maxValueLength, key);

            var expiration = expiresAt.UtcDateTime.Subtract(DateTime.UtcNow);

            if (tags?.Any() == true)
            {
                return ExecuteAddWithTags(key, value, tags, db => db.StringSetAsync(key, entryBytes, expiration, when, flag), expiration, when, flag);
            }

            return Database.StringSetAsync(key, entryBytes, expiration, when, flag);
        }

        /// <inheritdoc/>
        public Task<bool> ReplaceAsync<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return AddAsync(key, value, expiresAt, when, flag);
        }

        /// <inheritdoc/>
        public Task<bool> AddAsync<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None, HashSet<string> tags = null)
        {
            var entryBytes = value.OfValueSize(Serializer, maxValueLength, key);

            return Database.StringSetAsync(key, entryBytes, expiresIn, when, flag);
        }

        /// <inheritdoc/>
        public Task<bool> ReplaceAsync<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return AddAsync(key, value, expiresIn, when, flag);
        }

        /// <inheritdoc/>
        public async Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys)
        {
            var redisKeys = keys.Select(x => (RedisKey)x).ToArray();
            var result = await Database.StringGetAsync(redisKeys).ConfigureAwait(false);
            var dict = new Dictionary<string, T>(redisKeys.Length, StringComparer.Ordinal);

            for (var index = 0; index < redisKeys.Length; index++)
            {
                var value = result[index];
                dict.Add(redisKeys[index], value == RedisValue.Null ? default : Serializer.Deserialize<T>(value));
            }

            return dict;
        }

        /// <inheritdoc/>
        public async Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys, DateTimeOffset expiresAt)
        {
            var tsk1 = GetAllAsync<T>(keys);
            var tsk2 = UpdateExpiryAllAsync(keys.ToArray(), expiresAt);

            await Task.WhenAll(tsk1, tsk2).ConfigureAwait(false);

            return tsk1.Result;
        }

        /// <inheritdoc/>
        public async Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys, TimeSpan expiresIn)
        {
            var tsk1 = GetAllAsync<T>(keys);
            var tsk2 = UpdateExpiryAllAsync(keys.ToArray(), expiresIn);

            await Task.WhenAll(tsk1, tsk2).ConfigureAwait(false);

            return tsk1.Result;
        }

        /// <inheritdoc/>
        public Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var values = items
                        .OfValueInListSize(Serializer, maxValueLength)
                        .Select(x => new KeyValuePair<RedisKey, RedisValue>(x.Key, x.Value))
                        .ToArray();

            return Database.StringSetAsync(values, when, flag);
        }

        /// <inheritdoc/>
        public async Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var values = items
                        .OfValueInListSize(Serializer, maxValueLength)
                        .Select(x => new KeyValuePair<RedisKey, RedisValue>(x.Key, x.Value))
                        .ToArray();

            var tasks = new Task[values.Length + 1];
            tasks[0] = Database.StringSetAsync(values, when, flag);

            for (var i = 1; i < values.Length + 1; i++)
                tasks[i] = Database.KeyExpireAsync(values[i - 1].Key, expiresAt.UtcDateTime, flag);

            await Task.WhenAll(tasks).ConfigureAwait(false);

            return ((Task<bool>)tasks[0]).Result;
        }

        /// <inheritdoc/>
        public async Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, TimeSpan expiresOn, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            var values = items
                        .OfValueInListSize(Serializer, maxValueLength)
                        .Select(x => new KeyValuePair<RedisKey, RedisValue>(x.Key, x.Value))
                        .ToArray();

            var tasks = new Task[values.Length + 1];
            tasks[0] = Database.StringSetAsync(values, when, flag);

            for (var i = 1; i < values.Length + 1; i++)
                tasks[i] = Database.KeyExpireAsync(values[i - 1].Key, expiresOn, flag);

            await Task.WhenAll(tasks).ConfigureAwait(false);

            return ((Task<bool>)tasks[0]).Result;
        }

        /// <inheritdoc/>
        public Task<bool> SetAddAsync<T>(string key, T item, CommandFlags flag = CommandFlags.None)
            where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty.", nameof(key));

            if (item == null)
                throw new ArgumentNullException(nameof(item), "item cannot be null.");

            var serializedObject = Serializer.Serialize(item);

            return Database.SetAddAsync(key, serializedObject, flag);
        }

        /// <inheritdoc/>
        public async Task<T> SetPopAsync<T>(string key, CommandFlags flag = CommandFlags.None)
            where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty.", nameof(key));

            var item = await Database.SetPopAsync(key, flag).ConfigureAwait(false);

            if (item == RedisValue.Null)
                return default;

            return Serializer.Deserialize<T>(item);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> SetPopAsync<T>(string key, long count, CommandFlags flag = CommandFlags.None)
            where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty.", nameof(key));

            var items = await Database.SetPopAsync(key, count, flag).ConfigureAwait(false);

            return items.Select(item => item == RedisValue.Null ? default : Serializer.Deserialize<T>(item));
        }

        /// <inheritdoc/>
        public Task<bool> SetContainsAsync<T>(string key, T item, CommandFlags flag = CommandFlags.None)
            where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty.", nameof(key));

            if (item == null)
                throw new ArgumentNullException(nameof(item), "item cannot be null.");

            var serializedObject = Serializer.Serialize(item);

            return Database.SetContainsAsync(key, serializedObject, flag);
        }

        /// <inheritdoc/>
        public Task<long> SetAddAllAsync<T>(string key, CommandFlags flag = CommandFlags.None, params T[] items)
            where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty.", nameof(key));

            if (items == null)
                throw new ArgumentNullException(nameof(items), "items cannot be null.");

            if (items.Any(item => item == null))
                throw new ArgumentException("items cannot contains any null item.", nameof(items));

            return Database
                .SetAddAsync(
                    key,
                    items.Select(item => Serializer.Serialize(item)).Select(x => (RedisValue)x).ToArray(),
                    flag);
        }

        /// <inheritdoc/>
        public Task<bool> SetRemoveAsync<T>(string key, T item, CommandFlags flag = CommandFlags.None)
            where T : class
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
            where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty.", nameof(key));

            if (items == null)
                throw new ArgumentNullException(nameof(items), "items cannot be null.");

            if (items.Any(item => item == null))
                throw new ArgumentException("items cannot contains any null item.", nameof(items));

            return Database.SetRemoveAsync(key, items.Select(item => Serializer.Serialize(item)).Select(x => (RedisValue)x).ToArray(), flag);
        }

        /// <inheritdoc/>
        public async Task<string[]> SetMemberAsync(string memberName, CommandFlags flag = CommandFlags.None)
        {
            var members = await Database.SetMembersAsync(memberName, flag).ConfigureAwait(false);
            return members.Select(x => x.ToString()).ToArray();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> SetMembersAsync<T>(string key, CommandFlags flag = CommandFlags.None)
        {
            var members = await Database.SetMembersAsync(key, flag).ConfigureAwait(false);

            return members.Select(m => m == RedisValue.Null ? default : Serializer.Deserialize<T>(m));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> SearchKeysAsync(string pattern)
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
                    var redisResult = await Database.ExecuteAsync("SCAN", nextCursor.ToString(), "MATCH", pattern, "COUNT", "1000").ConfigureAwait(false);
                    var innerResult = (RedisResult[])redisResult;

                    nextCursor = int.Parse((string)innerResult[0]);

                    var resultLines = ((string[])innerResult[1]).ToArray();
                    keys.UnionWith(resultLines);
                }
                while (nextCursor != 0);
            }

            return !string.IsNullOrEmpty(keyprefix) ? keys.Select(k => k.Substring(keyprefix.Length)) : keys;
        }

        /// <inheritdoc/>
        public Task FlushDbAsync()
        {
            var endPoints = Database.Multiplexer.GetEndPoints();

            var tasks = new List<Task>(endPoints.Length);

            for (var i = 0; i < endPoints.Length; i++)
            {
                var server = Database.Multiplexer.GetServer(endPoints[i]);

                if (!server.IsSlave)
                    tasks.Add(server.FlushDatabaseAsync(Database.Database));
            }

            return Task.WhenAll(tasks);
        }

        /// <inheritdoc/>
        public Task SaveAsync(SaveType saveType, CommandFlags flags = CommandFlags.None)
        {
            var endPoints = Database.Multiplexer.GetEndPoints();

            var tasks = new Task[endPoints.Length];

            for (var i = 0; i < endPoints.Length; i++)
                tasks[i] = Database.Multiplexer.GetServer(endPoints[i]).SaveAsync(saveType, flags);

            return Task.WhenAll(tasks);
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, string>> GetInfoAsync()
        {
            var info = (await Database.ScriptEvaluateAsync("return redis.call('INFO')").ConfigureAwait(false)).ToString();

            return ParseInfo(info);
        }

        /// <inheritdoc/>
        public async Task<List<InfoDetail>> GetInfoCategorizedAsync()
        {
            var info = (await Database.ScriptEvaluateAsync("return redis.call('INFO')").ConfigureAwait(false)).ToString();

            return ParseCategorizedInfo(info);
        }

        /// <inheritdoc/>
        public Task<double> SortedSetAddIncrementAsync<T>(string key, T value, double score, CommandFlags commandFlags = CommandFlags.None)
        {
            var entryBytes = Serializer.Serialize(value);
            return Database.SortedSetIncrementAsync(key, entryBytes, score, commandFlags);
        }

        /// <summary>
        /// Utility method for creation of tags
        /// </summary>
        /// <param name="tag">The requested tag to set</param>
        /// <returns>Tag string</returns>
        internal static string GenerateTagKey(string tag) => $"tag:{tag}";

        private Dictionary<string, string> ParseInfo(string info)
        {
            // Call Parse Categorized Info to cut back on duplicated code.
            var data = ParseCategorizedInfo(info);

            // Return a dictionary of the Info Key and Info value
            return data.ToDictionary(x => x.Key, x => x.InfoValue);
        }

        private List<InfoDetail> ParseCategorizedInfo(string info)
        {
            var data = new List<InfoDetail>();
            var category = string.Empty;
            if (!string.IsNullOrWhiteSpace(info))
            {
                var lines = info.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    if (line[0] == '#')
                    {
                        category = line.Replace("#", string.Empty).Trim();
                        continue;
                    }

                    var idx = line.IndexOf(':');
                    if (idx > 0)
                    {
                        var key = line.Substring(0, idx);
                        var infoValue = line.Substring(idx + 1).Trim();

                        data.Add(new InfoDetail { Category = category, Key = key, InfoValue = infoValue });
                    }
                }
            }

            return data;
        }

        private Task<bool> ExecuteAddWithTags<TValue>(
            string key,
            TValue value,
            HashSet<string> tags,
            Func<IDatabaseAsync, Task<bool>> action,
            TimeSpan? expiration = null,
            When when = When.Always,
            CommandFlags commandFlags = CommandFlags.None)
        {
            var trx = Database.CreateTransaction();

            switch (when)
            {
                case When.NotExists:
                    trx.AddCondition(Condition.KeyNotExists(key));
                    break;
                case When.Exists:
                    trx.AddCondition(Condition.KeyExists(key));
                    break;
            }

            foreach (var tag in tags)
            {
                var tagKey = GenerateTagKey(tag);
                trx.SetAddAsync(tagKey, key.OfValueSize(Serializer, maxValueLength, tagKey), commandFlags);
            }

            action(trx);

            return trx.ExecuteAsync(commandFlags);
        }
    }
}
