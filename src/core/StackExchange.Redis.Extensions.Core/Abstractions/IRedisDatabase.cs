// Copyright (c) Ugo Lattanzi.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using StackExchange.Redis.Extensions.Core.Models;

namespace StackExchange.Redis.Extensions.Core.Abstractions;

/// <summary>
/// The Redis Database
/// </summary>
public partial interface IRedisDatabase
{
    /// <summary>
    ///     Gets the instance of <see cref="IDatabase" /> used by ICacheClient implementation
    /// </summary>
    public IDatabase Database { get; }

    /// <summary>
    ///     Gets the instance of <see cref="ISerializer" />
    /// </summary>
    public ISerializer Serializer { get; }

    /// <summary>
    ///     Verify that the specified cache key exists
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>True if the key is present into Redis. Otherwise False</returns>
    public Task<bool> ExistsAsync(string key, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Removes the specified key from Redis Database
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>True if the key has removed. Otherwise False</returns>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    public Task<bool> RemoveAsync(string key, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Removes all specified keys from Redis Database
    /// </summary>
    /// <param name="keys">The cache keys.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The number of items removed.</returns>
    public Task<long> RemoveAllAsync(string[] keys, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Get the object with the specified key from Redis database
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>Null if not present, otherwise the instance of T.</returns>
    public Task<T?> GetAsync<T>(string key, CommandFlags flag = CommandFlags.None);

    /// <summary>Get the object with the specified key from Redis database and update the expiry time</summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="expiresAt">Expiration time.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>Null if not present, otherwise the instance of T.</returns>
    public Task<T?> GetAsync<T>(string key, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Get the object with the specified key from Redis database and update the expiry time
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="expiresIn">Time till the object expires.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>
    ///     Null if not present, otherwise the instance of T.
    /// </returns>
    public Task<T?> GetAsync<T>(string key, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Adds the specified instance to the Redis database.
    /// </summary>
    /// <typeparam name="T">The type of the class to add to Redis</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The instance of T.</param>
    /// <param name="when">The condition (Always is the default value).</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <param name="tags">Tags</param>
    /// <returns>True if the object has been added. Otherwise false</returns>
    public Task<bool> AddAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None, HashSet<string>? tags = null);

    /// <summary>
    ///     Replaces the object with specified key into Redis database.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The instance of T</param>
    /// <param name="when">The condition (Always is the default value).</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>
    ///     True if the object has been added. Otherwise false
    /// </returns>
    public Task<bool> ReplaceAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Adds the specified instance to the Redis database.
    /// </summary>
    /// <typeparam name="T">The type of the class to add to Redis</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The instance of T.</param>
    /// <param name="expiresAt">Expiration time.</param>
    /// <param name="when">The condition (Always is the default value).</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <param name="tags">Tags</param>
    /// <returns>
    ///     True if the object has been added. Otherwise false
    /// </returns>
    public Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None, HashSet<string>? tags = null);

    /// <summary>
    ///     Replaces the object with specified key into Redis database.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The instance of T</param>
    /// <param name="expiresAt">Expiration time.</param>
    /// <param name="when">The condition (Always is the default value).</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>
    ///     True if the object has been added. Otherwise false
    /// </returns>
    public Task<bool> ReplaceAsync<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Adds the specified instance to the Redis database.
    /// </summary>
    /// <typeparam name="T">The type of the class to add to Redis</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The instance of T.</param>
    /// <param name="expiresIn">The duration of the cache using Timespan.</param>
    /// <param name="when">The condition (Always is the default value).</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <param name="tags">Tags</param>
    /// <returns>
    ///     True if the object has been added. Otherwise false
    /// </returns>
    public Task<bool> AddAsync<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None, HashSet<string>? tags = null);

    /// <summary>
    ///     Replaces the object with specified key into Redis database.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The instance of T</param>
    /// <param name="expiresIn">The duration of the cache using Timespan.</param>
    /// <param name="when">The condition (Always is the default value).</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>
    ///     True if the object has been added. Otherwise false
    /// </returns>
    public Task<bool> ReplaceAsync<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Get the objects with the specified keys from Redis database with a single roundtrip
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="keys">The cache keys.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>
    ///     Empty list if there are no results, otherwise the instance of T.
    ///     If a cache key is not present on Redis the specified object into the returned Dictionary will be null
    /// </returns>
    public Task<IDictionary<string, T?>> GetAllAsync<T>(HashSet<string> keys, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Get the objects with the specified keys from Redis database with one roundtrip
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="keys">The cache keys.</param>
    /// <param name="expiresAt">Expiration time.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>
    ///     Empty list if there are no results, otherwise the instance of T.
    ///     If a cache key is not present on Redis the specified object into the returned Dictionary will be null
    /// </returns>
    public Task<IDictionary<string, T?>> GetAllAsync<T>(HashSet<string> keys, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Get the objects with the specified keys from Redis database with one roundtrip
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="keys">The cache keys.</param>
    /// <param name="expiresIn">Time until expiration.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>
    ///     Empty list if there are no results, otherwise the instance of T.
    ///     If a cache key is not present on Redis the specified object into the returned Dictionary will be null
    /// </returns>
    public Task<IDictionary<string, T?>> GetAllAsync<T>(HashSet<string> keys, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Add the objects with the specified keys to Redis database with a single roundtrip
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="items">The items.</param>
    /// <param name="expiresAt">Expiration time.</param>
    /// <param name="when">The condition (Always is the default value).</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>True if all the objects have been added. Otherwise false</returns>
    public Task<bool> AddAllAsync<T>(Tuple<string, T>[] items, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Add the objects with the specified keys to Redis database with a single roundtrip
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="items">The items.</param>
    /// <param name="when">The condition (Always is the default value).</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>True if all the objects have been added. Otherwise false</returns>
    public Task<bool> AddAllAsync<T>(Tuple<string, T>[] items, When when = When.Always, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Add the objects with the specified keys to Redis database with a single roundtrip
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="items">The items.</param>
    /// <param name="expiresAt">Time until expiration.</param>
    /// <param name="when">The condition (Always is the default value).</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>True if all the objects have been added. Otherwise false</returns>
    public Task<bool> AddAllAsync<T>(Tuple<string, T>[] items, TimeSpan expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Run SADD command http://redis.io/commands/sadd
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="item">Name of the member.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    public Task<bool> SetAddAsync<T>(string key, T item, CommandFlags flag = CommandFlags.None)
       ;

    /// <summary>
    ///     Run SPOP command https://redis.io/commands/spop
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The key of the set</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    public Task<T?> SetPopAsync<T>(string key, CommandFlags flag = CommandFlags.None)
       ;

    /// <summary>
    ///     Run SPOP command https://redis.io/commands/spop
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The key of the set</param>
    /// <param name="count">The number of elements to return</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    public Task<IEnumerable<T?>> SetPopAsync<T>(string key, long count, CommandFlags flag = CommandFlags.None)
       ;

    /// <summary>
    ///     Returns if member is a member of the set stored at key.
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="item">The item to store into redis.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    public Task<bool> SetContainsAsync<T>(string key, T item, CommandFlags flag = CommandFlags.None)
       ;

    /// <summary>
    ///     Run SADD command http://redis.io/commands/sadd
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <param name="items">Name of the member.</param>
    public Task<long> SetAddAllAsync<T>(string key, CommandFlags flag = CommandFlags.None, params T[] items);

    /// <summary>
    ///     Run SADD command http://redis.io/commands/sadd
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <param name="items">Name of the member.</param>
    public Task<long> SetAddAllAsync<T>(string key, CommandFlags flag = CommandFlags.None, params ReadOnlySpan<T> items);

    /// <summary>
    ///     Run SREM command http://redis.io/commands/srem"
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="item">The object to store into redis</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    public Task<bool> SetRemoveAsync<T>(string key, T item, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Run SREM command http://redis.io/commands/srem
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <param name="items">The items to store into Redis.</param>
    public Task<long> SetRemoveAllAsync<T>(string key, CommandFlags flag = CommandFlags.None, params T[] items);

    /// <summary>
    ///     Run SREM command http://redis.io/commands/srem
    /// </summary>
    /// <typeparam name="T">The type of the expected object.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <param name="items">The items to store into Redis.</param>
    public Task<long> SetRemoveAllAsync<T>(string key, CommandFlags flag = CommandFlags.None, params ReadOnlySpan<T> items);

    /// <summary>
    ///     Run SMEMBERS command see http://redis.io/commands/SMEMBERS
    /// </summary>
    /// <param name="memberName">Name of the member.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    public Task<string[]> SetMemberAsync(string memberName, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Run SMEMBERS command see http://redis.io/commands/SMEMBERS
    ///     Deserializes the results to T
    /// </summary>
    /// <typeparam name="T">The type of the expected objects in the set</typeparam>
    /// <param name="key">The key</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>An array of objects in the set</returns>
    public Task<T[]> SetMembersAsync<T>(string key, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Run SUNION/SINTER/SDIFF command. See https://redis.io/commands/sunion, https://redis.io/commands/sinter, https://redis.io/commands/sdiff
    ///     Returns the result of the specified set operation between two sets, deserializing each member to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the expected objects in the set</typeparam>
    /// <param name="operation">The set operation to perform (Union, Intersect, Difference)</param>
    /// <param name="firstKey">The key of the first set</param>
    /// <param name="secondKey">The key of the second set</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>An array of deserialized objects resulting from the set operation</returns>
    public Task<T[]> SetCombineAsync<T>(SetOperation operation, string firstKey, string secondKey, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Run SUNION/SINTER/SDIFF command. See https://redis.io/commands/sunion, https://redis.io/commands/sinter, https://redis.io/commands/sdiff
    ///     Returns the result of the specified set operation between multiple sets, deserializing each member to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the expected objects in the set</typeparam>
    /// <param name="operation">The set operation to perform (Union, Intersect, Difference)</param>
    /// <param name="keys">The keys of the sets to combine</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>An array of deserialized objects resulting from the set operation</returns>
    public Task<T[]> SetCombineAsync<T>(SetOperation operation, string[] keys, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Run SUNIONSTORE/SINTERSTORE/SDIFFSTORE command. See https://redis.io/commands/sunionstore, https://redis.io/commands/sinterstore, https://redis.io/commands/sdiffstore
    ///     Performs the specified set operation between two sets and stores the result in the destination key.
    /// </summary>
    /// <param name="operation">The set operation to perform (Union, Intersect, Difference)</param>
    /// <param name="destinationKey">The key where the result will be stored</param>
    /// <param name="firstKey">The key of the first set</param>
    /// <param name="secondKey">The key of the second set</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The number of elements in the resulting set</returns>
    public Task<long> SetCombineAndStoreAsync(SetOperation operation, string destinationKey, string firstKey, string secondKey, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Run SUNIONSTORE/SINTERSTORE/SDIFFSTORE command. See https://redis.io/commands/sunionstore, https://redis.io/commands/sinterstore, https://redis.io/commands/sdiffstore
    ///     Performs the specified set operation between multiple sets and stores the result in the destination key.
    /// </summary>
    /// <param name="operation">The set operation to perform (Union, Intersect, Difference)</param>
    /// <param name="destinationKey">The key where the result will be stored</param>
    /// <param name="keys">The keys of the sets to combine</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The number of elements in the resulting set</returns>
    public Task<long> SetCombineAndStoreAsync(SetOperation operation, string destinationKey, string[] keys, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Searches the keys from Redis database
    /// </summary>
    /// <remarks>
    ///     Consider this as a command that should only be used in production environments with extreme care. It may ruin performance when it is executed against large databases
    /// </remarks>
    /// <param name="pattern">The pattern.</param>
    /// <example>
    ///     if you want to return all keys that start with "myCacheKey" uses "myCacheKey*"
    ///     if you want to return all keys that contain with "myCacheKey" uses "*myCacheKey*"
    ///     if you want to return all keys that end with "myCacheKey" uses "*myCacheKey"
    /// </example>
    /// <returns>A list of cache keys retrieved from Redis database</returns>
    public Task<IEnumerable<string>> SearchKeysAsync(string pattern);

    /// <summary>
    ///     Flushes the database asynchronous.
    /// </summary>
    public Task FlushDbAsync();

    /// <summary>
    ///     Save the DB in background asynchronous.
    /// </summary>
    public Task SaveAsync(SaveType saveType, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Gets the information about redis.
    ///     More info see http://redis.io/commands/INFO
    /// </summary>
    public Task<Dictionary<string, string>> GetInfoAsync();

    /// <summary>
    ///     Gets the information about redis with category.
    ///     More info see http://redis.io/commands/INFO
    /// </summary>
    public Task<InfoDetail[]> GetInfoCategorizedAsync();

    /// <summary>
    ///     Updates the expiry time of a redis cache object
    /// </summary>
    /// <param name="key">The key of the object</param>
    /// <param name="expiresAt">The new expiry time of the object</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>True if the object is updated, false if the object does not exist</returns>
    public Task<bool> UpdateExpiryAsync(string key, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Updates the expiry time of a redis cache object
    /// </summary>
    /// <param name="key">The key of the object</param>
    /// <param name="expiresIn">Time until the object will expire</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>True if the object is updated, false if the object does not exist</returns>
    public Task<bool> UpdateExpiryAsync(string key, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Updates the expiry time of a redis cache object
    /// </summary>
    /// <param name="keys">An array of keys to be updated</param>
    /// <param name="expiresAt">The new expiry time of the object</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>An array of type bool, where true if the object is updated and false if the object does not exist at the same index as the input keys</returns>
    public Task<IDictionary<string, bool>> UpdateExpiryAllAsync(HashSet<string> keys, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Updates the expiry time of a redis cache object
    /// </summary>
    /// <param name="keys">An array of keys to be updated</param>
    /// <param name="expiresIn">Time until the object will expire</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>An IDictionary object that contains the original key and the result of the operation</returns>
    public Task<IDictionary<string, bool>> UpdateExpiryAllAsync(HashSet<string> keys, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Increments the number stored at key by <paramref name="value"/>.
    ///     If the key does not exist, it is set to 0 before performing the operation.
    ///     Run INCRBY command https://redis.io/commands/incrby
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The amount to increment by (default 1).</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The value of key after the increment.</returns>
    public Task<long> StringIncrementAsync(string key, long value = 1, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Decrements the number stored at key by <paramref name="value"/>.
    ///     If the key does not exist, it is set to 0 before performing the operation.
    ///     Run DECRBY command https://redis.io/commands/decrby
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The amount to decrement by (default 1).</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The value of key after the decrement.</returns>
    public Task<long> StringDecrementAsync(string key, long value = 1, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Increments the floating point number stored at key by <paramref name="value"/>.
    ///     If the key does not exist, it is set to 0 before performing the operation.
    ///     Run INCRBYFLOAT command https://redis.io/commands/incrbyfloat
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The amount to increment by.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The value of key after the increment.</returns>
    public Task<double> StringIncrementAsync(string key, double value, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Decrements the floating point number stored at key by <paramref name="value"/>.
    ///     If the key does not exist, it is set to 0 before performing the operation.
    ///     There is no DECRBYFLOAT command; this is implemented via INCRBYFLOAT with a negated value.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The amount to decrement by.</param>
    /// <param name="flag">Behaviour markers associated with a given command.</param>
    /// <returns>The value of key after the decrement.</returns>
    public Task<double> StringDecrementAsync(string key, double value, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Renames the specified key.
    ///     Run RENAME command https://redis.io/commands/rename
    /// </summary>
    /// <param name="key">The key to rename.</param>
    /// <param name="newKey">The new name for the key.</param>
    /// <param name="when">The condition under which the rename should occur (Always is the default value).</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>True if the key was renamed. Otherwise false</returns>
    public Task<bool> KeyRenameAsync(string key, string newKey, When when = When.Always, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Returns the type of the value stored at the specified key.
    ///     Run TYPE command https://redis.io/commands/type
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The <see cref="RedisType"/> of the key, or <see cref="RedisType.None"/> when the key does not exist.</returns>
    public Task<RedisType> KeyTypeAsync(string key, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Serializes the value stored at the specified key in a Redis-specific format and returns it.
    ///     Run DUMP command https://redis.io/commands/dump
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    /// <returns>The serialized value as a byte array, or null if the key does not exist.</returns>
    public Task<byte[]?> KeyDumpAsync(string key, CommandFlags flag = CommandFlags.None);

    /// <summary>
    ///     Creates a key associated with a value that is obtained by deserializing the provided serialized value (obtained via DUMP).
    ///     Run RESTORE command https://redis.io/commands/restore
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The serialized value previously obtained using <see cref="KeyDumpAsync"/>.</param>
    /// <param name="expiry">The optional expiry to set on the key.</param>
    /// <param name="flag">Behaviour markers associated with a given command</param>
    public Task KeyRestoreAsync(string key, byte[] value, TimeSpan? expiry = null, CommandFlags flag = CommandFlags.None);
}
