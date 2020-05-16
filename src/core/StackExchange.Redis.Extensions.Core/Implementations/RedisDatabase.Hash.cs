using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace StackExchange.Redis.Extensions.Core.Implementations
{
    public partial class RedisDatabase : IRedisDatabase
    {
        /// <inheritdoc/>
        public Task<bool> HashDeleteAsync(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
        {
            return Database.HashDeleteAsync(hashKey, key, commandFlags);
        }

        /// <inheritdoc/>
        public Task<long> HashDeleteAsync(string hashKey, IEnumerable<string> keys, CommandFlags commandFlags = CommandFlags.None)
        {
            return Database.HashDeleteAsync(hashKey, keys.Select(x => (RedisValue)x).ToArray(), commandFlags);
        }

        /// <inheritdoc/>
        public Task<bool> HashExistsAsync(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
        {
            return Database.HashExistsAsync(hashKey, key, commandFlags);
        }

        /// <inheritdoc/>
        public async Task<T> HashGetAsync<T>(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
        {
            var redisValue = await Database.HashGetAsync(hashKey, key, commandFlags).ConfigureAwait(false);

            return redisValue.HasValue ? Serializer.Deserialize<T>(redisValue) : default;
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, T>> HashGetAsync<T>(string hashKey, IList<string> keys, CommandFlags commandFlags = CommandFlags.None)
        {
            var tasks = new Task<T>[keys.Count];

            for (var i = 0; i < keys.Count; i++)
                tasks[i] = HashGetAsync<T>(hashKey, keys[i], commandFlags);

            await Task.WhenAll(tasks).ConfigureAwait(false);

            var result = new Dictionary<string, T>();

            for (var i = 0; i < tasks.Length; i++)
                result.Add(keys[i], tasks[i].Result);

            return result;
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, T>> HashGetAllAsync<T>(string hashKey, CommandFlags commandFlags = CommandFlags.None)
        {
            return (await Database.HashGetAllAsync(hashKey, commandFlags).ConfigureAwait(false))
                .ToDictionary(
                    x => x.Name.ToString(),
                    x => Serializer.Deserialize<T>(x.Value),
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
        public Task<bool> HashSetAsync<T>(string hashKey, string key, T value, bool nx = false, CommandFlags commandFlags = CommandFlags.None, HashSet<string> tags = null)
        {
            var when = nx ? When.NotExists : When.Always;
            if (tags != null && tags.Count > 0)
            {
                return ExecuteHashAddWithTags(
                    hashKey,
                    key,
                    tags,
                    db => db.HashSetAsync(hashKey, key, Serializer.Serialize(value), when, commandFlags),
                    when,
                    commandFlags);
            }

            return Database.HashSetAsync(hashKey, key, Serializer.Serialize(value), when, commandFlags);
        }

        /// <inheritdoc/>
        public Task HashSetAsync<T>(string hashKey, IDictionary<string, T> values, CommandFlags commandFlags = CommandFlags.None)
        {
            var entries = values.Select(kv => new HashEntry(kv.Key, Serializer.Serialize(kv.Value)));

            return Database.HashSetAsync(hashKey, entries.ToArray(), commandFlags);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> HashValuesAsync<T>(string hashKey, CommandFlags commandFlags = CommandFlags.None)
        {
            return (await Database.HashValuesAsync(hashKey, commandFlags).ConfigureAwait(false)).Select(x => Serializer.Deserialize<T>(x));
        }

        /// <inheritdoc/>
        public Dictionary<string, T> HashScan<T>(string hashKey, string pattern, int pageSize = 10, CommandFlags commandFlags = CommandFlags.None)
        {
            return Database.HashScan(hashKey, pattern, pageSize, commandFlags).ToDictionary(x => x.Name.ToString(), x => Serializer.Deserialize<T>(x.Value), StringComparer.Ordinal);
        }
    }
}
