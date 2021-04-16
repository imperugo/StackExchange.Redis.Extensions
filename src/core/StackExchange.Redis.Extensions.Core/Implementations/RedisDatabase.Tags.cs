using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Extensions;
using StackExchange.Redis.Extensions.Core.Helpers;
using StackExchange.Redis.Extensions.Core.Models;

namespace StackExchange.Redis.Extensions.Core.Implementations
{
    public partial class RedisDatabase : IRedisDatabase
    {
        public async Task<IEnumerable<T>> GetByTagAsync<T>(string tag, CommandFlags commandFlags = CommandFlags.None)
        {
            var tagKey = TagHelper.GenerateTagKey(tag);

            var keys = await SetMembersAsync<string>(tagKey, commandFlags).ConfigureAwait(false);

            var result = await GetAllAsync<T>(keys).ConfigureAwait(false);

            return result.Values;
        }

        public async Task<long> RemoveByTagAsync<T>(string tag, CommandFlags commandFlags = CommandFlags.None)
        {
            var tagKey = TagHelper.GenerateTagKey(tag);

            var keys = await SetMembersAsync<string>(tagKey, commandFlags).ConfigureAwait(false);

            var deletedRecordsNumber = await RemoveAllAsync(keys, commandFlags).ConfigureAwait(false);
            return deletedRecordsNumber;
        }

        private Task<bool> ExecuteAddWithTags(
            string key,
            HashSet<string> tags,
            Func<IDatabaseAsync, Task<bool>> action,
            When when = When.Always,
            CommandFlags commandFlags = CommandFlags.None)
        {
            var transaction = Database.CreateTransaction();

            TryAddCondition(transaction, when, key);

            foreach (var tagKey in tags.Select(t => TagHelper.GenerateTagKey(t)))
            {
                transaction.SetAddAsync(tagKey, key.OfValueSize(Serializer, maxValueLength, tagKey), commandFlags);
            }

            action(transaction);

            return transaction.ExecuteAsync(commandFlags);
        }

        private Task<bool> ExecuteSetAddWithTags(
            string key,
            HashSet<string> tags,
            Func<IDatabaseAsync, Task<bool>> action,
            CommandFlags commandFlags = CommandFlags.None)
        {
            var transaction = Database.CreateTransaction();

            foreach (var tagKey in tags.Select(t => TagHelper.GenerateTagSetKey(t)))
            {
                transaction.SetAddAsync(tagKey, key.OfValueSize(Serializer, maxValueLength, tagKey), commandFlags);
            }

            action(transaction);

            return transaction.ExecuteAsync(commandFlags);
        }

        private Task<bool> ExecuteHashAddWithTags(
            string hashKey,
            string key,
            HashSet<string> tags,
            Func<IDatabaseAsync, Task<bool>> action,
            When when = When.Always,
            CommandFlags commandFlags = CommandFlags.None)
        {
            var transaction = Database.CreateTransaction();

            TryAddCondition(transaction, when, hashKey);

            var tagValue = new TagHashValue
            {
                HashKey = hashKey,
                Key = key
            };

            foreach (var tagKey in tags.Select(t => TagHelper.GenerateTagHashKey(t)))
            {
                transaction.SetAddAsync(tagKey, tagValue.OfValueSize(Serializer, maxValueLength, tagKey), commandFlags);
            }

            action(transaction);

            return transaction.ExecuteAsync(commandFlags);
        }

        private static void TryAddCondition(ITransaction transaction, When when, string key)
        {
            var condition = when switch
            {
                When.NotExists => Condition.KeyNotExists(key),
                When.Exists => Condition.KeyExists(key),
                _ => null
            };

            if (condition is null)
                return;

            transaction.AddCondition(condition);
        }
    }
}
