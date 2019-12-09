using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Models;

namespace StackExchange.Redis.Extensions.Core.Implementations
{
    public class RedisDefaultCacheClient : IRedisDefaultCacheClient
    {
        private readonly IRedisCacheClient cacheClient;

        public RedisDefaultCacheClient(IRedisCacheClient cacheClient)
        {
            this.cacheClient = cacheClient;
        }

        public IDatabase Database => cacheClient.GetDbFromConfiguration().Database;

        public ISerializer Serializer => cacheClient.GetDbFromConfiguration().Serializer;

        public bool Exists(string key, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().Exists(key, flag);
        }

        public Task<bool> ExistsAsync(string key, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().ExistsAsync(key, flag);
        }

        public bool Remove(string key, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().Remove(key, flag);
        }

        public Task<bool> RemoveAsync(string key, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().RemoveAsync(key, flag);
        }

        public void RemoveAll(IEnumerable<string> keys, CommandFlags flag = CommandFlags.None)
        {
            cacheClient.GetDbFromConfiguration().RemoveAll(keys, flag);
        }

        public Task RemoveAllAsync(IEnumerable<string> keys, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().RemoveAllAsync(keys, flag);
        }

        public T Get<T>(string key, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().Get<T>(key, flag);
        }

        public T Get<T>(string key, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().Get<T>(key, expiresAt, flag);
        }

        public T Get<T>(string key, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().Get<T>(key, expiresIn, flag);
        }

        public Task<T> GetAsync<T>(string key, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().GetAsync<T>(key, flag);
        }

        public Task<T> GetAsync<T>(string key, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().GetAsync<T>(key, expiresAt, flag);
        }

        public Task<T> GetAsync<T>(string key, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().GetAsync<T>(key, expiresIn, flag);
        }

        public bool Add<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().Add(key, value, when, flag);
        }

        public Task<bool> AddAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().AddAsync(key, value, when, flag);
        }

        public bool Replace<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().Replace(key, value, when, flag);
        }

        public Task<bool> ReplaceAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().ReplaceAsync(key, value, when, flag);
        }

        public bool Add<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().Add(key, value, expiresAt, when, flag);
        }

        public Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().AddAsync(key, value, expiresAt, when, flag);
        }

        public bool Replace<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().Replace(key, value, expiresAt, when, flag);
        }

        public Task<bool> ReplaceAsync<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().ReplaceAsync(key, value, expiresAt, when, flag);
        }

        public bool Add<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().Add(key, value, expiresIn, when, flag);
        }

        public Task<bool> AddAsync<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().AddAsync(key, value, expiresIn, when, flag);
        }

        public bool Replace<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().Replace(key, value, expiresIn, when, flag);
        }

        public Task<bool> ReplaceAsync<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().ReplaceAsync(key, value, expiresIn, when, flag);
        }

        public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys)
        {
            return cacheClient.GetDbFromConfiguration().GetAll<T>(keys);
        }

        public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys, DateTimeOffset expiresAt)
        {
            return cacheClient.GetDbFromConfiguration().GetAll<T>(keys, expiresAt);
        }

        public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys, TimeSpan expiresIn)
        {
            return cacheClient.GetDbFromConfiguration().GetAll<T>(keys, expiresIn);
        }

        public Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys)
        {
            return cacheClient.GetDbFromConfiguration().GetAllAsync<T>(keys);
        }

        public Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys, DateTimeOffset expiresAt)
        {
            return cacheClient.GetDbFromConfiguration().GetAllAsync<T>(keys, expiresAt);
        }

        public Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys, TimeSpan expiresIn)
        {
            return cacheClient.GetDbFromConfiguration().GetAllAsync<T>(keys, expiresIn);
        }

        public bool AddAll<T>(IList<Tuple<string, T>> items, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().AddAll(items, when, flag);
        }

        public Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().AddAllAsync(items, when, flag);
        }

        public bool AddAll<T>(IList<Tuple<string, T>> items, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().AddAll(items, expiresAt, when, flag);
        }

        public Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().AddAllAsync(items, when, flag);
        }

        public bool AddAll<T>(IList<Tuple<string, T>> items, TimeSpan expiresOn, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().AddAll(items, expiresOn, when, flag);
        }

        public Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, TimeSpan expiresOn, When when = When.Always, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().AddAllAsync(items, expiresOn, when, flag);
        }

        public bool SetAdd<T>(string key, T item, CommandFlags flag = CommandFlags.None)
            where T : class
        {
            return cacheClient.GetDbFromConfiguration().SetAdd(key, item, flag);
        }

        public Task<bool> SetAddAsync<T>(string key, T item, CommandFlags flag = CommandFlags.None)
            where T : class
        {
            return cacheClient.GetDbFromConfiguration().SetAddAsync(key, item, flag);
        }

        public Task<bool> SetContainsAsync<T>(string key, T item, CommandFlags flag = CommandFlags.None) 
            where T : class
        {
            return cacheClient.GetDbFromConfiguration().SetContainsAsync(key, item, flag);
        }

        public long SetAddAll<T>(string key, CommandFlags flag = CommandFlags.None, params T[] items)
            where T : class
        {
            return cacheClient.GetDbFromConfiguration().SetAddAll(key, flag, items);
        }

        public Task<long> SetAddAllAsync<T>(string key, CommandFlags flag = CommandFlags.None, params T[] items)
            where T : class
        {
            return cacheClient.GetDbFromConfiguration().SetAddAllAsync(key, flag, items);
        }

        public bool SetRemove<T>(string key, T item, CommandFlags flag = CommandFlags.None)
            where T : class
        {
            return cacheClient.GetDbFromConfiguration().SetRemove(key, item, flag);
        }

        public Task<bool> SetRemoveAsync<T>(string key, T item, CommandFlags flag = CommandFlags.None)
            where T : class
        {
            return cacheClient.GetDbFromConfiguration().SetRemoveAsync(key, item, flag);
        }

        public long SetRemoveAll<T>(string key, CommandFlags flag = CommandFlags.None, params T[] items)
            where T : class
        {
            return cacheClient.GetDbFromConfiguration().SetRemoveAll(key, flag, items);
        }

        public Task<long> SetRemoveAllAsync<T>(string key, CommandFlags flag = CommandFlags.None, params T[] items)
            where T : class
        {
            return cacheClient.GetDbFromConfiguration().SetRemoveAllAsync(key, flag, items);
        }

        public string[] SetMember(string memberName, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().SetMember(memberName, flag);
        }

        public Task<string[]> SetMemberAsync(string memberName, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().SetMemberAsync(memberName, flag);
        }

        public IEnumerable<T> SetMembers<T>(string key, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().SetMembers<T>(key, flag);
        }

        public Task<IEnumerable<T>> SetMembersAsync<T>(string key, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().SetMembersAsync<T>(key, flag);
        }

        public IEnumerable<string> SearchKeys(string pattern)
        {
            return cacheClient.GetDbFromConfiguration().SearchKeys(pattern);
        }

        public Task<IEnumerable<string>> SearchKeysAsync(string pattern)
        {
            return cacheClient.GetDbFromConfiguration().SearchKeysAsync(pattern);
        }

        public void FlushDb()
        {
            cacheClient.GetDbFromConfiguration().FlushDb();
        }

        public Task FlushDbAsync()
        {
            return cacheClient.GetDbFromConfiguration().FlushDbAsync();
        }

        public void Save(SaveType saveType, CommandFlags flag = CommandFlags.None)
        {
            cacheClient.GetDbFromConfiguration().Save(saveType, flag);
        }

        public Task SaveAsync(SaveType saveType, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().SaveAsync(saveType, flag);
        }

        public Dictionary<string, string> GetInfo()
        {
            return cacheClient.GetDbFromConfiguration().GetInfo();
        }

        public Task<Dictionary<string, string>> GetInfoAsync()
        {
            return cacheClient.GetDbFromConfiguration().GetInfoAsync();
        }

        public List<InfoDetail> GetInfoCategorized()
        {
            return cacheClient.GetDbFromConfiguration().GetInfoCategorized();
        }

        public Task<List<InfoDetail>> GetInfoCategorizedAsync()
        {
            return cacheClient.GetDbFromConfiguration().GetInfoCategorizedAsync();
        }

        public long Publish<T>(RedisChannel channel, T message, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().Publish(channel, message, flag);
        }

        public Task<long> PublishAsync<T>(RedisChannel channel, T message, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().PublishAsync(channel, message, flag);
        }

        public void Subscribe<T>(RedisChannel channel, Action<T> handler, CommandFlags flag = CommandFlags.None)
        {
            cacheClient.GetDbFromConfiguration().Subscribe<T>(channel, handler, flag);
        }

        public Task SubscribeAsync<T>(RedisChannel channel, Func<T, Task> handler, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().SubscribeAsync(channel, handler, flag);
        }

        public void Unsubscribe<T>(RedisChannel channel, Action<T> handler, CommandFlags flag = CommandFlags.None)
        {
            cacheClient.GetDbFromConfiguration().Unsubscribe(channel, handler, flag);
        }

        public Task UnsubscribeAsync<T>(RedisChannel channel, Func<T, Task> handler, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().UnsubscribeAsync(channel, handler, flag);
        }

        public void UnsubscribeAll(CommandFlags flag = CommandFlags.None)
        {
            cacheClient.GetDbFromConfiguration().UnsubscribeAll(flag);
        }

        public Task UnsubscribeAllAsync(CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().UnsubscribeAllAsync(flag);
        }

        public long ListAddToLeft<T>(string key, T item, When when = When.Always, CommandFlags flag = CommandFlags.None)
            where T : class
        {
            return cacheClient.GetDbFromConfiguration().ListAddToLeft(key, item, when, flag);
        }

        public Task<long> ListAddToLeftAsync<T>(string key, T item, When when = When.Always, CommandFlags flag = CommandFlags.None)
            where T : class
        {
            return cacheClient.GetDbFromConfiguration().ListAddToLeftAsync(key, item, when, flag);
        }

        public T ListGetFromRight<T>(string key, CommandFlags flag = CommandFlags.None)
            where T : class
        {
            return cacheClient.GetDbFromConfiguration().ListGetFromRight<T>(key, flag);
        }

        public Task<T> ListGetFromRightAsync<T>(string key, CommandFlags flag = CommandFlags.None)
            where T : class
        {
            return cacheClient.GetDbFromConfiguration().ListGetFromRightAsync<T>(key, flag);
        }

        public bool HashDelete(string hashKey, string key, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashDelete(hashKey, key, flag);
        }

        public long HashDelete(string hashKey, IEnumerable<string> keys, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashDelete(hashKey, keys, flag);
        }

        public bool HashExists(string hashKey, string key, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashExists(hashKey, key, flag);
        }

        public T HashGet<T>(string hashKey, string key, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashGet<T>(hashKey, key, flag);
        }

        public Dictionary<string, T> HashGet<T>(string hashKey, IEnumerable<string> keys, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashGet<T>(hashKey, keys, flag);
        }

        public Dictionary<string, T> HashGetAll<T>(string hashKey, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashGetAll<T>(hashKey, flag);
        }

        public long HashIncerementBy(string hashKey, string key, long value, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashIncerementBy(hashKey, key, value, flag);
        }

        public double HashIncerementBy(string hashKey, string key, double value, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashIncerementBy(hashKey, key, value, flag);
        }

        public IEnumerable<string> HashKeys(string hashKey, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashKeys(hashKey, flag);
        }

        public long HashLength(string hashKey, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashLength(hashKey, flag);
        }

        public bool HashSet<T>(string hashKey, string key, T value, bool nx = false, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashSet(hashKey, key, value, nx, flag);
        }

        public void HashSet<T>(string hashKey, Dictionary<string, T> values, CommandFlags flag = CommandFlags.None)
        {
            cacheClient.GetDbFromConfiguration().HashSet<T>(hashKey, values, flag);
        }

        public IEnumerable<T> HashValues<T>(string hashKey, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashValues<T>(hashKey, flag);
        }

        public Dictionary<string, T> HashScan<T>(string hashKey, string pattern, int pageSize = 10, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashScan<T>(hashKey, pattern, pageSize, flag);
        }

        public Task<bool> HashDeleteAsync(string hashKey, string key, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashDeleteAsync(hashKey, key, flag);
        }

        public Task<long> HashDeleteAsync(string hashKey, IEnumerable<string> keys, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashDeleteAsync(hashKey, keys, flag);
        }

        public Task<bool> HashExistsAsync(string hashKey, string key, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashExistsAsync(hashKey, key, flag);
        }

        public Task<T> HashGetAsync<T>(string hashKey, string key, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashGetAsync<T>(hashKey, key, flag);
        }

        public Task<Dictionary<string, T>> HashGetAsync<T>(string hashKey, IEnumerable<string> keys, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashGetAsync<T>(hashKey, keys, flag);
        }

        public Task<Dictionary<string, T>> HashGetAllAsync<T>(string hashKey, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashGetAllAsync<T>(hashKey, flag);
        }

        public Task<long> HashIncerementByAsync(string hashKey, string key, long value, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashIncerementByAsync(hashKey, key, value, flag);
        }

        public Task<double> HashIncerementByAsync(string hashKey, string key, double value, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashIncerementByAsync(hashKey, key, value, flag);
        }

        public Task<IEnumerable<string>> HashKeysAsync(string hashKey, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashKeysAsync(hashKey, flag);
        }

        public Task<long> HashLengthAsync(string hashKey, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashLengthAsync(hashKey, flag);
        }

        public Task<bool> HashSetAsync<T>(string hashKey, string key, T value, bool nx = false, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashSetAsync(hashKey, key, value, nx, flag);
        }

        public Task HashSetAsync<T>(string hashKey, IDictionary<string, T> values, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashSetAsync(hashKey, values, flag);
        }

        public Task<IEnumerable<T>> HashValuesAsync<T>(string hashKey, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashValuesAsync<T>(hashKey, flag);
        }

        public Task<Dictionary<string, T>> HashScanAsync<T>(string hashKey, string pattern, int pageSize = 10, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().HashScanAsync<T>(hashKey, pattern, pageSize, flag);
        }

        public bool UpdateExpiry(string key, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().UpdateExpiry(key, expiresAt, flag);
        }

        public bool UpdateExpiry(string key, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().UpdateExpiry(key, expiresIn, flag);
        }

        public Task<bool> UpdateExpiryAsync(string key, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().UpdateExpiryAsync(key, expiresAt, flag);
        }

        public Task<bool> UpdateExpiryAsync(string key, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().UpdateExpiryAsync(key, expiresIn, flag);
        }

        public IDictionary<string, bool> UpdateExpiryAll(string[] keys, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().UpdateExpiryAll(keys, expiresAt, flag);
        }

        public IDictionary<string, bool> UpdateExpiryAll(string[] keys, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().UpdateExpiryAll(keys, expiresIn, flag);
        }

        public Task<IDictionary<string, bool>> UpdateExpiryAllAsync(string[] keys, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().UpdateExpiryAllAsync(keys, expiresAt, flag);
        }

        public Task<IDictionary<string, bool>> UpdateExpiryAllAsync(string[] keys, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().UpdateExpiryAllAsync(keys, expiresIn, flag);
        }

        public bool SortedSetAdd<T>(string key, T value, double score, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().SortedSetAdd(key, value, score, flag);
        }

        public Task<bool> SortedSetAddAsync<T>(string key, T value, double score, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().SortedSetAddAsync(key, value, score, flag);
        }

        public bool SortedSetRemove<T>(string key, T value, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().SortedSetRemove(key, value, flag);
        }

        public Task<bool> SortedSetRemoveAsync<T>(string key, T value, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().SortedSetRemoveAsync(key, value, flag);
        }

        public IEnumerable<T> SortedSetRangeByScore<T>(string key, double start = Double.NegativeInfinity, double stop = Double.PositiveInfinity, Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().SortedSetRangeByScore<T>(key, start, stop, exclude, order, skip, take, flag);
        }

        public Task<IEnumerable<T>> SortedSetRangeByScoreAsync<T>(string key, double start = Double.NegativeInfinity, double stop = Double.PositiveInfinity, Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flag = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().SortedSetRangeByScoreAsync<T>(key, start, stop, exclude, order, skip, take, flag);
        }

        public double SortedSetAddIncrement<T>(string key, T value, double score, CommandFlags commandFlags = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().SortedSetAddIncrement(key, value, score, commandFlags);
        }

        public Task<double> SortedSetAddIncrementAsync<T>(string key, T value, double score, CommandFlags commandFlags = CommandFlags.None)
        {
            return cacheClient.GetDbFromConfiguration().SortedSetAddIncrementAsync(key, value, score, commandFlags);
        }
    }
}