using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace StackExchange.Redis.Extensions.Core.Implementations
{
    internal partial class RedisDatabase : IRedisDatabase
    {
        public Task<bool> HashDeleteAsync(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
        {
            return Database.HashDeleteAsync(hashKey, key, commandFlags);
        }

        public Task<long> HashDeleteAsync(string hashKey, IEnumerable<string> keys, CommandFlags commandFlags = CommandFlags.None)
        {
            return Database.HashDeleteAsync(hashKey, keys.Select(x => (RedisValue)x).ToArray(), commandFlags);
        }

        public Task<bool> HashExistsAsync(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
        {
            return Database.HashExistsAsync(hashKey, key, commandFlags);
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
            return (await Database.HashGetAllAsync(hashKey, commandFlags))
                .ToDictionary(
                    x => x.Name.ToString(),
                    x => Serializer.Deserialize<T>(x.Value),
                    StringComparer.Ordinal);
        }

        public Task<long> HashIncerementByAsync(string hashKey, string key, long value, CommandFlags commandFlags = CommandFlags.None)
        {
            return Database.HashIncrementAsync(hashKey, key, value, commandFlags);
        }

        public Task<double> HashIncerementByAsync(string hashKey, string key, double value, CommandFlags commandFlags = CommandFlags.None)
        {
            return Database.HashIncrementAsync(hashKey, key, value, commandFlags);
        }

        public async Task<IEnumerable<string>> HashKeysAsync(string hashKey, CommandFlags commandFlags = CommandFlags.None)
        {
            return (await Database.HashKeysAsync(hashKey, commandFlags)).Select(x => x.ToString());
        }

        public Task<long> HashLengthAsync(string hashKey, CommandFlags commandFlags = CommandFlags.None)
        {
            return Database.HashLengthAsync(hashKey, commandFlags);
        }

        public Task<bool> HashSetAsync<T>(string hashKey, string key, T value, bool nx = false, CommandFlags commandFlags = CommandFlags.None)
        {
            return Database.HashSetAsync(hashKey, key, Serializer.Serialize(value), nx ? When.NotExists : When.Always, commandFlags);
        }

        public Task HashSetAsync<T>(string hashKey, IDictionary<string, T> values, CommandFlags commandFlags = CommandFlags.None)
        {
            var entries = values.Select(kv => new HashEntry(kv.Key, Serializer.Serialize(kv.Value)));

            return Database.HashSetAsync(hashKey, entries.ToArray(), commandFlags);
        }

        public async Task<IEnumerable<T>> HashValuesAsync<T>(string hashKey, CommandFlags commandFlags = CommandFlags.None)
        {
            return (await Database.HashValuesAsync(hashKey, commandFlags)).Select(x => Serializer.Deserialize<T>(x));
        }

        public Dictionary<string, T> HashScan<T>(string hashKey, string pattern, int pageSize = 10, CommandFlags commandFlags = CommandFlags.None)
        {
            return Database.HashScan(hashKey, pattern, pageSize, commandFlags).ToDictionary(x => x.Name.ToString(), x => Serializer.Deserialize<T>(x.Value), StringComparer.Ordinal);
        }
    }
}
