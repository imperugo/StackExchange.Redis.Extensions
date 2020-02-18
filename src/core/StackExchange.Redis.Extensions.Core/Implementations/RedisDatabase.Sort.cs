using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace StackExchange.Redis.Extensions.Core.Implementations
{
    internal partial class RedisDatabase : IRedisDatabase
    {
        public Task<bool> SortedSetAddAsync<T>(
                                    string key,
                                    T value,
                                    double score,
                                    CommandFlags commandFlags = CommandFlags.None)
        {
            var entryBytes = Serializer.Serialize(value);

            return Database.SortedSetAddAsync(key, entryBytes, score, commandFlags);
        }

        public Task<bool> SortedSetRemoveAsync<T>(
                                    string key,
                                    T value,
                                    CommandFlags commandFlags = CommandFlags.None)
        {
            var entryBytes = Serializer.Serialize(value);

            return Database.SortedSetRemoveAsync(key, entryBytes, commandFlags);
        }

        public IEnumerable<T> SortedSetRangeByScore<T>(
                                    string key,
                                    double start = double.NegativeInfinity,
                                    double stop = double.PositiveInfinity,
                                    Exclude exclude = Exclude.None,
                                    Order order = Order.Ascending,
                                    long skip = 0L,
                                    long take = -1L,
                                    CommandFlags commandFlags = CommandFlags.None)
        {
            var result = Database.SortedSetRangeByScore(key, start, stop, exclude, order, skip, take, commandFlags);

            return result.Select(m => m == RedisValue.Null ? default : Serializer.Deserialize<T>(m));
        }

        public async Task<IEnumerable<T>> SortedSetRangeByScoreAsync<T>(
                                    string key,
                                    double start = double.NegativeInfinity,
                                    double stop = double.PositiveInfinity,
                                    Exclude exclude = Exclude.None,
                                    Order order = Order.Ascending,
                                    long skip = 0L,
                                    long take = -1L,
                                    CommandFlags commandFlags = CommandFlags.None)
        {
            var result = await Database.SortedSetRangeByScoreAsync(key, start, stop, exclude, order, skip, take, commandFlags);

            return result.Select(m => m == RedisValue.Null ? default : Serializer.Deserialize<T>(m));
        }
    }
}
