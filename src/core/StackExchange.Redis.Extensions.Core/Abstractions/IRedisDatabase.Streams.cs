// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions.Core.Abstractions;

/// <summary>
/// The Redis Database Streams extensions
/// </summary>
public partial interface IRedisDatabase
{
    /// <summary>
    ///     Appends a serialized entry to a stream using a single named field.
    ///     The value is serialized using the configured <see cref="ISerializer"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value to serialize.</typeparam>
    /// <param name="key">The key of the stream.</param>
    /// <param name="fieldName">The field name within the stream entry.</param>
    /// <param name="value">The value to serialize and store.</param>
    /// <param name="messageId">The id to assign to the message, or null for auto-generation.</param>
    /// <param name="maxLength">The approximate maximum length of the stream, or null for no limit.</param>
    /// <param name="useApproximateMaxLength">If true, allows the stream to exceed maxLength slightly for performance.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The id of the added message.</returns>
    Task<RedisValue> StreamAddAsync<T>(string key, string fieldName, T value, RedisValue? messageId = null, int? maxLength = null, bool useApproximateMaxLength = false, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Appends a multi-field entry to a stream.
    /// </summary>
    /// <param name="key">The key of the stream.</param>
    /// <param name="entries">The field-value pairs to store in the stream entry.</param>
    /// <param name="messageId">The id to assign to the message, or null for auto-generation.</param>
    /// <param name="maxLength">The approximate maximum length of the stream, or null for no limit.</param>
    /// <param name="useApproximateMaxLength">If true, allows the stream to exceed maxLength slightly for performance.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The id of the added message.</returns>
    Task<RedisValue> StreamAddAsync(string key, NameValueEntry[] entries, RedisValue? messageId = null, int? maxLength = null, bool useApproximateMaxLength = false, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns the number of entries in a stream.
    /// </summary>
    /// <param name="key">The key of the stream.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The number of entries in the stream, or 0 if the key does not exist.</returns>
    Task<long> StreamLengthAsync(string key, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Trims a stream to a specified maximum length.
    /// </summary>
    /// <param name="key">The key of the stream.</param>
    /// <param name="maxLength">The maximum length of the stream after trimming.</param>
    /// <param name="useApproximateMaxLength">If true, allows slightly more entries for performance.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The number of entries removed from the stream.</returns>
    Task<long> StreamTrimAsync(string key, int maxLength, bool useApproximateMaxLength = false, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Deletes entries from a stream by their message IDs.
    /// </summary>
    /// <param name="key">The key of the stream.</param>
    /// <param name="messageIds">The IDs of the messages to delete.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The number of entries deleted.</returns>
    Task<long> StreamDeleteAsync(string key, string[] messageIds, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns a range of entries from a stream.
    /// </summary>
    /// <param name="key">The key of the stream.</param>
    /// <param name="minId">The minimum message ID, or null for the beginning.</param>
    /// <param name="maxId">The maximum message ID, or null for the end.</param>
    /// <param name="count">The maximum number of entries to return, or null for all.</param>
    /// <param name="messageOrder">The order to return messages (ascending or descending).</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The stream entries in the requested range.</returns>
    Task<StreamEntry[]> StreamRangeAsync(string key, RedisValue? minId = null, RedisValue? maxId = null, int? count = null, Order messageOrder = Order.Ascending, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Reads entries from a stream starting at the given position.
    /// </summary>
    /// <param name="key">The key of the stream.</param>
    /// <param name="position">The position to read from (e.g. "0-0" for the beginning, or a specific message ID).</param>
    /// <param name="count">The maximum number of entries to return, or null for all.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The stream entries read.</returns>
    Task<StreamEntry[]> StreamReadAsync(string key, RedisValue position, int? count = null, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Creates a consumer group for the specified stream.
    /// </summary>
    /// <param name="key">The key of the stream.</param>
    /// <param name="groupName">The name of the consumer group to create.</param>
    /// <param name="position">The position in the stream to start reading from, or null for the latest.</param>
    /// <param name="createStream">If true, creates the stream if it does not already exist.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>True if the group was created.</returns>
    Task<bool> StreamCreateConsumerGroupAsync(string key, string groupName, RedisValue? position = null, bool createStream = true, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Sets the last delivered ID of a consumer group.
    /// </summary>
    /// <param name="key">The key of the stream.</param>
    /// <param name="groupName">The name of the consumer group.</param>
    /// <param name="position">The new position to set.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>True if the position was set.</returns>
    Task<bool> StreamConsumerGroupSetPositionAsync(string key, string groupName, RedisValue position, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Deletes a consumer group.
    /// </summary>
    /// <param name="key">The key of the stream.</param>
    /// <param name="groupName">The name of the consumer group to delete.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>True if the group was deleted.</returns>
    Task<bool> StreamDeleteConsumerGroupAsync(string key, string groupName, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Removes a consumer from a consumer group.
    /// </summary>
    /// <param name="key">The key of the stream.</param>
    /// <param name="groupName">The name of the consumer group.</param>
    /// <param name="consumerName">The name of the consumer to remove.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The number of pending entries that the consumer had before being removed.</returns>
    Task<long> StreamDeleteConsumerAsync(string key, string groupName, string consumerName, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Reads entries from a stream as a member of a consumer group.
    /// </summary>
    /// <param name="key">The key of the stream.</param>
    /// <param name="groupName">The name of the consumer group.</param>
    /// <param name="consumerName">The name of the consumer.</param>
    /// <param name="position">The position to read from, or null for new messages only (&gt;).</param>
    /// <param name="count">The maximum number of entries to return, or null for all.</param>
    /// <param name="noAck">If true, the messages are not added to the PEL (pending entry list).</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The stream entries read.</returns>
    Task<StreamEntry[]> StreamReadGroupAsync(string key, string groupName, string consumerName, RedisValue? position = null, int? count = null, bool noAck = false, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Acknowledges one message as processed by a consumer group.
    /// </summary>
    /// <param name="key">The key of the stream.</param>
    /// <param name="groupName">The name of the consumer group.</param>
    /// <param name="messageId">The ID of the message to acknowledge.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The number of messages acknowledged (0 or 1).</returns>
    Task<long> StreamAcknowledgeAsync(string key, string groupName, string messageId, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Acknowledges multiple messages as processed by a consumer group.
    /// </summary>
    /// <param name="key">The key of the stream.</param>
    /// <param name="groupName">The name of the consumer group.</param>
    /// <param name="messageIds">The IDs of the messages to acknowledge.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The number of messages acknowledged.</returns>
    Task<long> StreamAcknowledgeAsync(string key, string groupName, string[] messageIds, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns a summary of pending messages for a consumer group.
    /// </summary>
    /// <param name="key">The key of the stream.</param>
    /// <param name="groupName">The name of the consumer group.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>Summary information about pending messages.</returns>
    Task<StreamPendingInfo> StreamPendingAsync(string key, string groupName, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns detailed information about pending messages for a consumer group.
    /// </summary>
    /// <param name="key">The key of the stream.</param>
    /// <param name="groupName">The name of the consumer group.</param>
    /// <param name="count">The maximum number of pending messages to return.</param>
    /// <param name="consumerName">The consumer to filter by.</param>
    /// <param name="minId">The minimum message ID, or null for the beginning.</param>
    /// <param name="maxId">The maximum message ID, or null for the end.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>Detailed information about each pending message.</returns>
    Task<StreamPendingMessageInfo[]> StreamPendingMessagesAsync(string key, string groupName, int count, RedisValue consumerName, RedisValue? minId = null, RedisValue? maxId = null, CommandFlags flag = CommandFlags.None);
}
