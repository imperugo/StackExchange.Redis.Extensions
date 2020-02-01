using System;
using System.Threading.Tasks;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace StackExchange.Redis.Extensions.Core.Implementations
{
    internal partial class RedisDatabase : IRedisDatabase
    {
        public Task<long> ListAddToLeftAsync<T>(string key, T item, When when = When.Always, CommandFlags flags = CommandFlags.None) where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key cannot be empty.", nameof(key));

            if (item == null)
                throw new ArgumentNullException(nameof(item), "item cannot be null.");

            var serializedItem = Serializer.Serialize(item);

            return Database.ListLeftPushAsync(key, serializedItem, when, flags);
        }

        public async Task<T> ListGetFromRightAsync<T>(string key, CommandFlags flags = CommandFlags.None) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key cannot be empty.", nameof(key));

            var item = await Database.ListRightPopAsync(key, flags);

            if (item == RedisValue.Null) return null;

            return item == RedisValue.Null
                                    ? null
                                    : Serializer.Deserialize<T>(item);
        }
    }
}
