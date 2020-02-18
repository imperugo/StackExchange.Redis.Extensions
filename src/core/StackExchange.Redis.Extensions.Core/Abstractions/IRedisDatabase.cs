using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis.Extensions.Core.Models;

namespace StackExchange.Redis.Extensions.Core.Abstractions
{
    /// <summary>
    /// The Redis Database
    /// </summary>
    public partial interface IRedisDatabase
    {
        /// <summary>
        ///     Gets the instance of <see cref="IDatabase" /> used be ICacheClient implementation
        /// </summary>
        IDatabase Database { get; }

        /// <summary>
        ///     Gets the instance of <see cref="ISerializer" />
        /// </summary>
        ISerializer Serializer { get; }

        /// <summary>
        ///     Verify that the specified cache key exists
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <returns>True if the key is present into Redis. Othwerwise False</returns>
        Task<bool> ExistsAsync(string key, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Removes the specified key from Redis Database
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>True if the key has removed. Othwerwise False</returns>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        Task<bool> RemoveAsync(string key, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Removes all specified keys from Redis Database
        /// </summary>
        /// <param name="keys">The cache keys.</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        Task RemoveAllAsync(IEnumerable<string> keys, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Get the object with the specified key from Redis database
        /// </summary>
        /// <typeparam name="T">The type of the expected object.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <returns>Null if not present, otherwise the instance of T.</returns>
        Task<T> GetAsync<T>(string key, CommandFlags flag = CommandFlags.None);

        /// <summary>Get the object with the specified key from Redis database and update the expiry time</summary>
        /// <typeparam name="T">The type of the expected object.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="expiresAt">Expiration time.</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <returns>Null if not present, otherwise the instance of T.</returns>
        Task<T> GetAsync<T>(string key, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None);

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
        Task<T> GetAsync<T>(string key, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Adds the specified instance to the Redis database.
        /// </summary>
        /// <typeparam name="T">The type of the class to add to Redis</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The instance of T.</param>
        /// <param name="when">The condition (Always is the default value).</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <returns>True if the object has been added. Otherwise false</returns>
        Task<bool> AddAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None);

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
        Task<bool> ReplaceAsync<T>(string key, T value, When when = When.Always, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Adds the specified instance to the Redis database.
        /// </summary>
        /// <typeparam name="T">The type of the class to add to Redis</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The instance of T.</param>
        /// <param name="expiresAt">Expiration time.</param>
        /// <param name="when">The condition (Always is the default value).</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <returns>
        ///     True if the object has been added. Otherwise false
        /// </returns>
        Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None);

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
        Task<bool> ReplaceAsync<T>(string key, T value, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Adds the specified instance to the Redis database.
        /// </summary>
        /// <typeparam name="T">The type of the class to add to Redis</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The instance of T.</param>
        /// <param name="expiresIn">The duration of the cache using Timespan.</param>
        /// <param name="when">The condition (Always is the default value).</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <returns>
        ///     True if the object has been added. Otherwise false
        /// </returns>
        Task<bool> AddAsync<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None);

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
        Task<bool> ReplaceAsync<T>(string key, T value, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Get the objects with the specified keys from Redis database with a single roundtrip
        /// </summary>
        /// <typeparam name="T">The type of the expected object.</typeparam>
        /// <param name="keys">The cache keys.</param>
        /// <returns>
        ///     Empty list if there are no results, otherwise the instance of T.
        ///     If a cache key is not present on Redis the specified object into the returned Dictionary will be null
        /// </returns>
        Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys);

        /// <summary>
        ///     Get the objects with the specified keys from Redis database with one roundtrip
        /// </summary>
        /// <typeparam name="T">The type of the expected object.</typeparam>
        /// <param name="keys">The cache keys.</param>
        /// <param name="expiresAt">Expiration time.</param>
        /// <returns>
        ///     Empty list if there are no results, otherwise the instance of T.
        ///     If a cache key is not present on Redis the specified object into the returned Dictionary will be null
        /// </returns>
        Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys, DateTimeOffset expiresAt);

        /// <summary>
        ///     Get the objects with the specified keys from Redis database with one roundtrip
        /// </summary>
        /// <typeparam name="T">The type of the expected object.</typeparam>
        /// <param name="keys">The cache keys.</param>
        /// <param name="expiresIn">Time until expiration.</param>
        /// <returns>
        ///     Empty list if there are no results, otherwise the instance of T.
        ///     If a cache key is not present on Redis the specified object into the returned Dictionary will be null
        /// </returns>
        Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys, TimeSpan expiresIn);

        /// <summary>
        ///     Add the objects with the specified keys to Redis database with a single roundtrip
        /// </summary>
        /// <typeparam name="T">The type of the expected object.</typeparam>
        /// <param name="items">The items.</param>
        /// <param name="expiresAt">Expiration time.</param>
        /// <param name="when">The condition (Always is the default value).</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, DateTimeOffset expiresAt, When when = When.Always, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Add the objects with the specified keys to Redis database with a single roundtrip
        /// </summary>
        /// <typeparam name="T">The type of the expected object.</typeparam>
        /// <param name="items">The items.</param>
        /// <param name="when">The condition (Always is the default value).</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, When when = When.Always, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Add the objects with the specified keys to Redis database with a single roundtrip
        /// </summary>
        /// <typeparam name="T">The type of the expected object.</typeparam>
        /// <param name="items">The items.</param>
        /// <param name="expiresIn">Time until expiration.</param>
        /// <param name="when">The condition (Always is the default value).</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, TimeSpan expiresIn, When when = When.Always, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Run SADD command http://redis.io/commands/sadd
        /// </summary>
        /// <typeparam name="T">The type of the expected object.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="item">Name of the member.</param>
        /// <param name="flag">Behaviour markers associated with a given command.</param>
        Task<bool> SetAddAsync<T>(string key, T item, CommandFlags flag = CommandFlags.None)
            where T : class;

        /// <summary>
        ///     Returns if member is a member of the set stored at key.
        /// </summary>
        /// <typeparam name="T">The type of the expected object.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="item">The item to store into redis.</param>
        /// <param name="flag">Behaviour markers associated with a given command.</param>
        Task<bool> SetContainsAsync<T>(string key, T item, CommandFlags flag = CommandFlags.None)
            where T : class;

        /// <summary>
        ///     Run SADD command http://redis.io/commands/sadd
        /// </summary>
        /// <typeparam name="T">The type of the expected object.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <param name="items">Name of the member.</param>
        Task<long> SetAddAllAsync<T>(string key, CommandFlags flag = CommandFlags.None, params T[] items)
            where T : class;

        /// <summary>
        ///     Run SREM command http://redis.io/commands/srem"
        /// </summary>
        /// <typeparam name="T">The type of the expected object.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="item">The object to store into redis</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        Task<bool> SetRemoveAsync<T>(string key, T item, CommandFlags flag = CommandFlags.None)
            where T : class;

        /// <summary>
        ///     Run SREM command http://redis.io/commands/srem
        /// </summary>
        /// <typeparam name="T">The type of the expected object.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <param name="items">The items to store into Redis.</param>
        Task<long> SetRemoveAllAsync<T>(string key, CommandFlags flag = CommandFlags.None, params T[] items)
            where T : class;

        /// <summary>
        ///     Run SMEMBERS command see http://redis.io/commands/SMEMBERS
        /// </summary>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        Task<string[]> SetMemberAsync(string memberName, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Run SMEMBERS command see http://redis.io/commands/SMEMBERS
        ///     Deserializes the results to T
        /// </summary>
        /// <typeparam name="T">The type of the expected objects in the set</typeparam>
        /// <param name="key">The key</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <returns>An array of objects in the set</returns>
        Task<IEnumerable<T>> SetMembersAsync<T>(string key, CommandFlags flag = CommandFlags.None);

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
        Task<IEnumerable<string>> SearchKeysAsync(string pattern);

        /// <summary>
        ///     Flushes the database asynchronous.
        /// </summary>
        Task FlushDbAsync();

        /// <summary>
        ///     Save the DB in background asynchronous.
        /// </summary>
        Task SaveAsync(SaveType saveType, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Gets the information about redis.
        ///     More info see http://redis.io/commands/INFO
        /// </summary>
        Task<Dictionary<string, string>> GetInfoAsync();

        /// <summary>
        ///     Gets the information about redis with category.
        ///     More info see http://redis.io/commands/INFO
        /// </summary>
        Task<List<InfoDetail>> GetInfoCategorizedAsync();

        /// <summary>
        ///     Updates the expiry time of a redis cache object
        /// </summary>
        /// <param name="key">The key of the object</param>
        /// <param name="expiresAt">The new expiry time of the object</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <returns>True if the object is updated, false if the object does not exist</returns>
        Task<bool> UpdateExpiryAsync(string key, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Updates the expiry time of a redis cache object
        /// </summary>
        /// <param name="key">The key of the object</param>
        /// <param name="expiresIn">Time until the object will expire</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <returns>True if the object is updated, false if the object does not exist</returns>
        Task<bool> UpdateExpiryAsync(string key, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Updates the expiry time of a redis cache object
        /// </summary>
        /// <param name="keys">An array of keys to be updated</param>
        /// <param name="expiresAt">The new expiry time of the object</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <returns>An array of type bool, where true if the object is updated and false if the object does not exist at the same index as the input keys</returns>
        Task<IDictionary<string, bool>> UpdateExpiryAllAsync(string[] keys, DateTimeOffset expiresAt, CommandFlags flag = CommandFlags.None);

        /// <summary>
        ///     Updates the expiry time of a redis cache object
        /// </summary>
        /// <param name="keys">An array of keys to be updated</param>
        /// <param name="expiresIn">Time until the object will expire</param>
        /// <param name="flag">Behaviour markers associated with a given command</param>
        /// <returns>An IDictionary object that contains the origional key and the result of the operation</returns>
        Task<IDictionary<string, bool>> UpdateExpiryAllAsync(string[] keys, TimeSpan expiresIn, CommandFlags flag = CommandFlags.None);
    }
}
