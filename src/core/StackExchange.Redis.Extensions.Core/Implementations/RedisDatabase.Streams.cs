// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;

using StackExchange.Redis.Extensions.Core.Helpers;

namespace StackExchange.Redis.Extensions.Core.Implementations;

/// <inheritdoc/>
public partial class RedisDatabase
{
    /// <inheritdoc/>
    public Task<RedisValue> StreamAddAsync<T>(string key, string fieldName, T value, RedisValue? messageId = null, int? maxLength = null, bool useApproximateMaxLength = false, CommandFlags flag = CommandFlags.None)
    {
        var serializedValue = Serializer.Serialize(value);

        return Database.StreamAddAsync(key, fieldName, serializedValue, messageId, maxLength, useApproximateMaxLength, flag);
    }

    /// <inheritdoc/>
    public Task<RedisValue> StreamAddAsync(string key, NameValueEntry[] entries, RedisValue? messageId = null, int? maxLength = null, bool useApproximateMaxLength = false, CommandFlags flag = CommandFlags.None) =>
        Database.StreamAddAsync(key, entries, messageId, maxLength, useApproximateMaxLength, flag);

    /// <inheritdoc/>
    public Task<long> StreamLengthAsync(string key, CommandFlags flag = CommandFlags.None) =>
        Database.StreamLengthAsync(key, flag);

    /// <inheritdoc/>
    public Task<long> StreamTrimAsync(string key, int maxLength, bool useApproximateMaxLength = false, CommandFlags flag = CommandFlags.None) =>
        Database.StreamTrimAsync(key, maxLength, useApproximateMaxLength, flag);

    /// <inheritdoc/>
    public Task<long> StreamDeleteAsync(string key, string[] messageIds, CommandFlags flag = CommandFlags.None) =>
        Database.StreamDeleteAsync(key, messageIds.ToFastArray(id => (RedisValue)id), flag);

    /// <inheritdoc/>
    public Task<StreamEntry[]> StreamRangeAsync(string key, RedisValue? minId = null, RedisValue? maxId = null, int? count = null, Order messageOrder = Order.Ascending, CommandFlags flag = CommandFlags.None) =>
        Database.StreamRangeAsync(key, minId, maxId, count, messageOrder, flag);

    /// <inheritdoc/>
    public Task<StreamEntry[]> StreamReadAsync(string key, RedisValue position, int? count = null, CommandFlags flag = CommandFlags.None) =>
        Database.StreamReadAsync(key, position, count, flag);

    /// <inheritdoc/>
    public Task<bool> StreamCreateConsumerGroupAsync(string key, string groupName, RedisValue? position = null, bool createStream = true, CommandFlags flag = CommandFlags.None) =>
        Database.StreamCreateConsumerGroupAsync(key, groupName, position, createStream, flag);

    /// <inheritdoc/>
    public Task<bool> StreamConsumerGroupSetPositionAsync(string key, string groupName, RedisValue position, CommandFlags flag = CommandFlags.None) =>
        Database.StreamConsumerGroupSetPositionAsync(key, groupName, position, flag);

    /// <inheritdoc/>
    public Task<bool> StreamDeleteConsumerGroupAsync(string key, string groupName, CommandFlags flag = CommandFlags.None) =>
        Database.StreamDeleteConsumerGroupAsync(key, groupName, flag);

    /// <inheritdoc/>
    public Task<long> StreamDeleteConsumerAsync(string key, string groupName, string consumerName, CommandFlags flag = CommandFlags.None) =>
        Database.StreamDeleteConsumerAsync(key, groupName, consumerName, flag);

    /// <inheritdoc/>
    public Task<StreamEntry[]> StreamReadGroupAsync(string key, string groupName, string consumerName, RedisValue? position = null, int? count = null, bool noAck = false, CommandFlags flag = CommandFlags.None) =>
        Database.StreamReadGroupAsync(key, groupName, consumerName, position, count, noAck, flag);

    /// <inheritdoc/>
    public Task<long> StreamAcknowledgeAsync(string key, string groupName, string messageId, CommandFlags flag = CommandFlags.None) =>
        Database.StreamAcknowledgeAsync(key, groupName, messageId, flag);

    /// <inheritdoc/>
    public Task<long> StreamAcknowledgeAsync(string key, string groupName, string[] messageIds, CommandFlags flag = CommandFlags.None) =>
        Database.StreamAcknowledgeAsync(key, groupName, messageIds.ToFastArray(id => (RedisValue)id), flag);

    /// <inheritdoc/>
    public Task<StreamPendingInfo> StreamPendingAsync(string key, string groupName, CommandFlags flag = CommandFlags.None) =>
        Database.StreamPendingAsync(key, groupName, flag);

    /// <inheritdoc/>
    public Task<StreamPendingMessageInfo[]> StreamPendingMessagesAsync(string key, string groupName, int count, RedisValue consumerName, RedisValue? minId = null, RedisValue? maxId = null, CommandFlags flag = CommandFlags.None) =>
        Database.StreamPendingMessagesAsync(key, groupName, count, consumerName, minId, maxId, flag);
}
