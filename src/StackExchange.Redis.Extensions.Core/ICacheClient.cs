using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions.Core
{
	/// <summary>
	/// Contrac for ICache implementation
	/// </summary>
	public interface ICacheClient : IDisposable
	{
		/// <summary>
		/// Return the instance of <see cref="StackExchange.Redis.IDatabase"/> used be ICacheClient implementation
		/// </summary>
		IDatabase Database { get; }

		/// <summary>
		/// Return the instance of <see cref="ISerializer"/>
		/// </summary>
		ISerializer Serializer { get; }

		/// <summary>
		/// Verify that the specified cache key exists
		/// </summary>
		/// <param name="key">The cache key.</param>
		/// <returns>True if the key is present into Redis. Othwerwise False</returns>
		bool Exists(string key);

		/// <summary>
		/// Verify that the specified cache key exists
		/// </summary>
		/// <param name="key">The cache key.</param>
		/// <returns>True if the key is present into Redis. Othwerwise False</returns>
		Task<bool> ExistsAsync(string key);

		/// <summary>
		/// Removes the specified key from Redis Database
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>True if the key has removed. Othwerwise False</returns>
		bool Remove(string key);

		/// <summary>
		/// Removes the specified key from Redis Database
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>True if the key has removed. Othwerwise False</returns>
		Task<bool> RemoveAsync(string key);

		/// <summary>
		/// Removes all specified keys from Redis Database
		/// </summary>
		/// <param name="keys">The key.</param>
		void RemoveAll(IEnumerable<string> keys);

		/// <summary>
		/// Removes all specified keys from Redis Database
		/// </summary>
		/// <param name="keys">The key.</param>
		Task RemoveAllAsync(IEnumerable<string> keys);

		/// <summary>
		/// Get the object with the specified key from Redis database
		/// </summary>
		/// <typeparam name="T">The type of the expected object</typeparam>
		/// <param name="key">The cache key.</param>
		/// <returns>Null if not present, otherwise the instance of T.</returns>
		T Get<T>(string key);

        /// <summary>
        /// Get the object with the specified key from Redis database and update the expiry time
        /// </summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="expiresAt">Expiration time.</param>
        /// <returns>Null if not present, otherwise the instance of T.</returns>
        T Get<T>(string key, DateTimeOffset expiresAt);

        /// <summary>
        ///     Get the object with the specified key from Redis database and update the expiry time
        /// </summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="expiresIn">Time till the object expires.</param>
        /// <returns>
        ///     Null if not present, otherwise the instance of T.
        /// </returns>
        T Get<T>(string key, TimeSpan expiresIn);

        /// <summary>
        /// Get the object with the specified key from Redis database
        /// </summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="key">The cache key.</param>
        /// <returns>Null if not present, otherwise the instance of T.</returns>
        Task<T> GetAsync<T>(string key);

        /// <summary>Get the object with the specified key from Redis database and update the expiry time</summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="expiresAt">Expiration time.</param>
        /// <returns>Null if not present, otherwise the instance of T.</returns>
        Task<T> GetAsync<T>(string key, DateTimeOffset expiresAt);

        /// <summary>
        ///     Get the object with the specified key from Redis database and update the expiry time
        /// </summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="expiresIn">Time till the object expires.</param>
        /// <returns>
        ///     Null if not present, otherwise the instance of T.
        /// </returns>
        Task<T> GetAsync<T>(string key, TimeSpan expiresIn);

        /// <summary>
        /// Adds the specified instance to the Redis database.
        /// </summary>
        /// <typeparam name="T">The type of the class to add to Redis</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The instance of T.</param>
        /// <returns>True if the object has been added. Otherwise false</returns>
        bool Add<T>(string key, T value);

		/// <summary>
		/// Adds the specified instance to the Redis database.
		/// </summary>
		/// <typeparam name="T">The type of the class to add to Redis</typeparam>
		/// <param name="key">The cache key.</param>
		/// <param name="value">The instance of T.</param>
		/// <returns>True if the object has been added. Otherwise false</returns>
		Task<bool> AddAsync<T>(string key, T value);

		/// <summary>
		/// Replaces the object with specified key into Redis database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <param name="value">The instance of T</param>
		/// <returns>
		/// True if the object has been added. Otherwise false
		/// </returns>
		bool Replace<T>(string key, T value);

		/// <summary>
		/// Replaces the object with specified key into Redis database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <param name="value">The instance of T</param>
		/// <returns>
		/// True if the object has been added. Otherwise false
		/// </returns>
		Task<bool> ReplaceAsync<T>(string key, T value);

		/// <summary>
		/// Adds the specified instance to the Redis database.
		/// </summary>
		/// <typeparam name="T">The type of the class to add to Redis</typeparam>
		/// <param name="key">The cache key.</param>
		/// <param name="value">The instance of T.</param>
		/// <param name="expiresAt">Expiration time.</param>
		/// <returns>
		/// True if the object has been added. Otherwise false
		/// </returns>
		bool Add<T>(string key, T value, DateTimeOffset expiresAt);

		/// <summary>
		/// Adds the specified instance to the Redis database.
		/// </summary>
		/// <typeparam name="T">The type of the class to add to Redis</typeparam>
		/// <param name="key">The cache key.</param>
		/// <param name="value">The instance of T.</param>
		/// <param name="expiresAt">Expiration time.</param>
		/// <returns>
		/// True if the object has been added. Otherwise false
		/// </returns>
		Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt);

		/// <summary>
		/// Replaces the object with specified key into Redis database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <param name="value">The instance of T</param>
		/// <param name="expiresAt">Expiration time.</param>
		/// <returns>
		/// True if the object has been added. Otherwise false
		/// </returns>
		bool Replace<T>(string key, T value, DateTimeOffset expiresAt);

		/// <summary>
		/// Replaces the object with specified key into Redis database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <param name="value">The instance of T</param>
		/// <param name="expiresAt">Expiration time.</param>
		/// <returns>
		/// True if the object has been added. Otherwise false
		/// </returns>
		Task<bool> ReplaceAsync<T>(string key, T value, DateTimeOffset expiresAt);

		/// <summary>
		/// Adds the specified instance to the Redis database.
		/// </summary>
		/// <typeparam name="T">The type of the class to add to Redis</typeparam>
		/// <param name="key">The cache key.</param>
		/// <param name="value">The instance of T.</param>
		/// <param name="expiresIn">The duration of the cache using Timespan.</param>
		/// <returns>
		/// True if the object has been added. Otherwise false
		/// </returns>
		bool Add<T>(string key, T value, TimeSpan expiresIn);

		/// <summary>
		/// Adds the specified instance to the Redis database.
		/// </summary>
		/// <typeparam name="T">The type of the class to add to Redis</typeparam>
		/// <param name="key">The cache key.</param>
		/// <param name="value">The instance of T.</param>
		/// <param name="expiresIn">The duration of the cache using Timespan.</param>
		/// <returns>
		/// True if the object has been added. Otherwise false
		/// </returns>
		Task<bool> AddAsync<T>(string key, T value, TimeSpan expiresIn);

		/// <summary>
		/// Replaces the object with specified key into Redis database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <param name="value">The instance of T</param>
		/// <param name="expiresIn">The duration of the cache using Timespan.</param>
		/// <returns>
		/// True if the object has been added. Otherwise false
		/// </returns>
		bool Replace<T>(string key, T value, TimeSpan expiresIn);

		/// <summary>
		/// Replaces the object with specified key into Redis database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <param name="value">The instance of T</param>
		/// <param name="expiresIn">The duration of the cache using Timespan.</param>
		/// <returns>
		/// True if the object has been added. Otherwise false
		/// </returns>
		Task<bool> ReplaceAsync<T>(string key, T value, TimeSpan expiresIn);

		/// <summary>
		/// Get the objects with the specified keys from Redis database with one roundtrip
		/// </summary>
		/// <typeparam name="T">The type of the expected object</typeparam>
		/// <param name="keys">The keys.</param>
		/// <returns>
		/// Empty list if there are no results, otherwise the instance of T.
		/// If a cache key is not present on Redis the specified object into the returned Dictionary will be null
		/// </returns>
		IDictionary<string, T> GetAll<T>(IEnumerable<string> keys);

        /// <summary>
        ///     Get the objects with the specified keys from Redis database with one roundtrip
        /// </summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="keys">The keys.</param>
        /// <param name="expiresAt">Expiration time.</param>
        /// <returns>
        ///     Empty list if there are no results, otherwise the instance of T.
        ///     If a cache key is not present on Redis the specified object into the returned Dictionary will be null
        /// </returns>
        IDictionary<string, T> GetAll<T>(IEnumerable<string> keys, DateTimeOffset expiresAt);

        /// <summary>
        ///     Get the objects with the specified keys from Redis database with one roundtrip
        /// </summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="keys">The keys.</param>
        /// <param name="expiresIn">Time until expiration.</param>
        /// <returns>
        ///     Empty list if there are no results, otherwise the instance of T.
        ///     If a cache key is not present on Redis the specified object into the returned Dictionary will be null
        /// </returns>
        IDictionary<string, T> GetAll<T>(IEnumerable<string> keys, TimeSpan expiresIn);

        /// <summary>
        /// Get the objects with the specified keys from Redis database with a single roundtrip
        /// </summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="keys">The keys.</param>
        /// <returns>
        /// Empty list if there are no results, otherwise the instance of T.
        /// If a cache key is not present on Redis the specified object into the returned Dictionary will be null
        /// </returns>
        Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys);

        /// <summary>
        ///     Get the objects with the specified keys from Redis database with one roundtrip
        /// </summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="keys">The keys.</param>
        /// <param name="expiresAt">Expiration time.</param>
        /// <returns>
        ///     Empty list if there are no results, otherwise the instance of T.
        ///     If a cache key is not present on Redis the specified object into the returned Dictionary will be null
        /// </returns>
        Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys, DateTimeOffset expiresAt);

        /// <summary>
        ///     Get the objects with the specified keys from Redis database with one roundtrip
        /// </summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="keys">The keys.</param>
        /// <param name="expiresIn">Time until expiration.</param>
        /// <returns>
        ///     Empty list if there are no results, otherwise the instance of T.
        ///     If a cache key is not present on Redis the specified object into the returned Dictionary will be null
        /// </returns>
        Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys, TimeSpan expiresIn);

        /// <summary>
        /// Add the objects with the specified keys to Redis database with a single roundtrip
        /// </summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="items">The items.</param>
        bool AddAll<T>(IList<Tuple<string, T>> items);

        /// <summary>
        /// Add the objects with the specified keys to Redis database with a single roundtrip
        /// </summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="items">The items.</param>
        Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items);

        /// <summary>
        /// Add the objects with the specified keys to Redis database with a single roundtrip
        /// </summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="items">The items.</param>
        bool AddAll<T>(IList<Tuple<string, T>> items, DateTimeOffset expiresAt);

        /// <summary>
        /// Add the objects with the specified keys to Redis database with a single roundtrip
        /// </summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="items">The items.</param>
        Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, DateTimeOffset expiresAt);

        /// <summary>
        /// Add the objects with the specified keys to Redis database with a single roundtrip
        /// </summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="items">The items.</param>
        bool AddAll<T>(IList<Tuple<string, T>> items, TimeSpan expiresIn);

        /// <summary>
        /// Add the objects with the specified keys to Redis database with a single roundtrip
        /// </summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="items">The items.</param>
        Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, TimeSpan expiresIn);

        /// <summary>
        /// Run SADD command http://redis.io/commands/sadd
        /// </summary>
        /// <param name="item">Name of the member.</param>
        /// <param name="key">The key.</param>
        bool SetAdd<T>(string key, T item) where T : class;

		/// <summary>
		/// Run SADD command http://redis.io/commands/sadd
		/// </summary>
		/// <param name="item">Name of the member.</param>
		/// <param name="key">The key.</param>
		Task<bool> SetAddAsync<T>(string key, T item) where T : class;

		/// <summary>
		/// Run SADD command http://redis.io/commands/sadd
		/// </summary>
		/// <param name="items">Name of the member.</param>
		/// <param name="key">The key.</param>
		long SetAddAll<T>(string key, params T[] items) where T : class;

		/// <summary>
		/// Run SADD command http://redis.io/commands/sadd
		/// </summary>
		/// <param name="items">Name of the member.</param>
		/// <param name="key">The key.</param>
		Task<long> SetAddAllAsync<T>(string key, params T[] items) where T : class;

		/// <summary>
		/// Run SREM command http://redis.io/commands/srem
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="item"></param>
		bool SetRemove<T>(string key, T item) where T : class;

		/// <summary>
		/// Run SREM command http://redis.io/commands/srem"
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="item"></param>
		Task<bool> SetRemoveAsync<T>(string key, T item) where T : class;

		/// <summary>
		/// Run SREM command http://redis.io/commands/srem
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="items"></param>
		long SetRemoveAll<T>(string key, params T[] items) where T : class;

		/// <summary>
		/// Run SREM command http://redis.io/commands/srem
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="items"></param>
		Task<long> SetRemoveAllAsync<T>(string key, params T[] items) where T : class;

		/// <summary>
		/// Run SMEMBERS command http://redis.io/commands/SMEMBERS
		/// </summary>
		/// <param name="memberName">Name of the member.</param>
		string[] SetMember(string memberName);

		/// <summary>
		/// Run SMEMBERS command see http://redis.io/commands/SMEMBERS
		/// </summary>
		/// <param name="memberName">Name of the member.</param>
		Task<string[]> SetMemberAsync(string memberName);

		/// <summary>
		///     Run SMEMBERS command see http://redis.io/commands/SMEMBERS
		///     Deserializes the results to T
		/// </summary>
		/// <typeparam name="T">The type of the expected objects in the set</typeparam>
		/// <param name="key">The key</param>
		/// <returns>An array of objects in the set</returns>
		IEnumerable<T> SetMembers<T>(string key);

		/// <summary>
		///     Run SMEMBERS command see http://redis.io/commands/SMEMBERS
		///     Deserializes the results to T
		/// </summary>
		/// <typeparam name="T">The type of the expected objects in the set</typeparam>
		/// <param name="key">The key</param>
		/// <returns>An array of objects in the set</returns>
		Task<IEnumerable<T>> SetMembersAsync<T>(string key);

		/// <summary>
		/// Searches the keys from Redis database
		/// </summary>
		/// <remarks>
		/// Consider this as a command that should only be used in production environments with extreme care. It may ruin performance when it is executed against large databases
		/// </remarks>
		/// <param name="pattern">The pattern.</param>
		/// <example>
		///		if you want to return all keys that start with "myCacheKey" uses "myCacheKey*"
		///		if you want to return all keys that contain with "myCacheKey" uses "*myCacheKey*"
		///		if you want to return all keys that end with "myCacheKey" uses "*myCacheKey"
		/// </example>
		/// <returns>A list of cache keys retrieved from Redis database</returns>
		IEnumerable<string> SearchKeys(string pattern);

		/// <summary>
		/// Searches the keys from Redis database
		/// </summary>
		/// <remarks>
		/// Consider this as a command that should only be used in production environments with extreme care. It may ruin performance when it is executed against large databases
		/// </remarks>
		/// <param name="pattern">The pattern.</param>
		/// <example>
		///		if you want to return all keys that start with "myCacheKey" uses "myCacheKey*"
		///		if you want to return all keys that contain with "myCacheKey" uses "*myCacheKey*"
		///		if you want to return all keys that end with "myCacheKey" uses "*myCacheKey"
		/// </example>
		/// <returns>A list of cache keys retrieved from Redis database</returns>
		Task<IEnumerable<string>> SearchKeysAsync(string pattern);

		/// <summary>
		/// Flushes the database.
		/// </summary>
		void FlushDb();

		/// <summary>
		/// Flushes the database asynchronous.
		/// </summary>
		/// <returns></returns>
		Task FlushDbAsync();

		/// <summary>
		/// Save the DB in background.
		/// </summary>
		void Save(SaveType saveType);

		/// <summary>
		/// Save the DB in background asynchronous.
		/// </summary>
		Task SaveAsync(SaveType saveType);

		/// <summary>
		/// Gets the information about redis.
		/// More info see http://redis.io/commands/INFO
		/// </summary>
		Dictionary<string, string> GetInfo();

		/// <summary>
		/// Gets the information about redis.
		/// More info see http://redis.io/commands/INFO
		/// </summary>
		Task<Dictionary<string, string>> GetInfoAsync();

		/// <summary>
		/// Publishes a message to a channel.
		/// </summary>
		long Publish<T>(RedisChannel channel, T message, CommandFlags flags = CommandFlags.None);

		/// <summary>
		/// Publishes a message to a channel.
		/// </summary>
		Task<long> PublishAsync<T>(RedisChannel channel, T message, CommandFlags flags = CommandFlags.None);

		/// <summary>
		/// Registers a callback handler to process messages published to a channel.
		/// </summary>
		void Subscribe<T>(RedisChannel channel, Action<T> handler, CommandFlags flags = CommandFlags.None);

		/// <summary>
		/// Registers a callback handler to process messages published to a channel.
		/// </summary>
		Task SubscribeAsync<T>(RedisChannel channel, Func<T, Task> handler, CommandFlags flags = CommandFlags.None);

		/// <summary>
		/// Unregisters a callback handler to process messages published to a channel.
		/// </summary>
		void Unsubscribe<T>(RedisChannel channel, Action<T> handler, CommandFlags flags = CommandFlags.None);

		/// <summary>
		/// Unregisters a callback handler to process messages published to a channel.
		/// </summary>
		Task UnsubscribeAsync<T>(RedisChannel channel, Func<T, Task> handler, CommandFlags flags = CommandFlags.None);

		/// <summary>
		/// Unregisters all callback handlers on a channel.
		/// </summary>
		void UnsubscribeAll(CommandFlags flags = CommandFlags.None);

		/// <summary>
		/// Unregisters all callback handlers on a channel.
		/// </summary>
		Task UnsubscribeAllAsync(CommandFlags flags = CommandFlags.None);

		/// <summary>
		/// Insert the specified value at the head of the list stored at key. If key does not exist, it is created as empty list before performing the push operations.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <param name="item">The item.</param>
		/// <returns>
		/// the length of the list after the push operations.
		/// </returns>
		/// <remarks>
		/// http://redis.io/commands/lpush
		/// </remarks>
		long ListAddToLeft<T>(string key, T item) where T : class;

		/// <summary>
		/// Lists the add to left asynchronous.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		Task<long> ListAddToLeftAsync<T>(string key, T item) where T : class;

		/// <summary>
		/// Removes and returns the last element of the list stored at key.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		/// <remarks>
		/// http://redis.io/commands/rpop
		/// </remarks>
		T ListGetFromRight<T>(string key) where T : class;

		/// <summary>
		/// Removes and returns the last element of the list stored at key.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		/// <remarks>
		/// http://redis.io/commands/rpop
		/// </remarks>
		Task<T> ListGetFromRightAsync<T>(string key) where T : class;

		/// <summary>
		///     Removes the specified fields from the hash stored at key. 
		///     Specified fields that do not exist within this hash are ignored. 
		/// </summary>
		/// <remarks>
		///     Time complexity: O(1)
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="key">Key of the entry</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>
		///     If key is deleted returns true.
		///     If key does not exist, it is treated as an empty hash and this command returns false.
		/// </returns>
		bool HashDelete(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Removes the specified fields from the hash stored at key. 
		///     Specified fields that do not exist within this hash are ignored. 
		///     If key does not exist, it is treated as an empty hash and this command returns 0.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the number of fields to be removed.
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="keys"></param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>Tthe number of fields that were removed from the hash, not including specified but non existing fields.</returns>
		long HashDelete(string hashKey, IEnumerable<string> keys, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Returns if field is an existing field in the hash stored at key.
		/// </summary>
		/// 
		/// <remarks>
		///     Time complexity: O(1)
		/// </remarks>
		/// <param name="hashKey">The key of the hash in redis</param>
		/// <param name="key">The key of the field in the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>Returns if field is an existing field in the hash stored at key.</returns>
		bool HashExists(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Returns the value associated with field in the hash stored at key.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(1)
		/// </remarks>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="key">Key of the entry</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>the value associated with field, or nil when field is not present in the hash or key does not exist.</returns>
		T HashGet<T>(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Returns the values associated with the specified fields in the hash stored at key.
		///     For every field that does not exist in the hash, a nil value is returned. 
		///     Because a non-existing keys are treated as empty hashes, running HMGET against a non-existing key will return a list of nil values.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the number of fields being requested.
		/// </remarks>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="keys"></param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>list of values associated with the given fields, in the same order as they are requested.</returns>
		Dictionary<string, T> HashGet<T>(string hashKey, IEnumerable<string> keys, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Returns all fields and values of the hash stored at key. In the returned value, every field name is followed by its value, so the length of the reply is twice the size of the hash.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the size of the hash.
		/// </remarks>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>list of fields and their values stored in the hash, or an empty list when key does not exist.</returns>
		Dictionary<string, T> HashGetAll<T>(string hashKey, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Increments the number stored at field in the hash stored at key by increment. If key does not exist, a new key holding a hash is created. 
		///     If field does not exist the value is set to 0 before the operation is performed.
		///     The range of values supported by HINCRBY is limited to 64 bit signed integers.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(1)
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="key">Key of the entry</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <param name="value">the value at field after the increment operation</param>
		long HashIncerementBy(string hashKey, string key, long value, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Increment the specified field of an hash stored at key, and representing a floating point number, by the specified increment. 
		///     If the field does not exist, it is set to 0 before performing the operation.
		/// </summary>
		/// <remarks>
		///     <para>
		///         An error is returned if one of the following conditions occur:
		///         * The field contains a value of the wrong type (not a string).
		///         * The current field content or the specified increment are not parsable as a double precision floating point number.
		///     </para>
		///     <para>
		///         Time complexity: O(1)
		///     </para>
		///     
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="key">Key of the entry</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <param name="value">the value at field after the increment operation</param>
		double HashIncerementBy(string hashKey, string key, double value, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Returns all field names in the hash stored at key.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the size of the hash.
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>list of fields in the hash, or an empty list when key does not exist.</returns>
		IEnumerable<string> HashKeys(string hashKey, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Returns the number of fields contained in the hash stored at key.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(1)
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>number of fields in the hash, or 0 when key does not exist.</returns>
		long HashLength(string hashKey, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Sets field in the hash stored at key to value. If key does not exist, a new key holding a hash is created. If field already exists in the hash, it is overwritten.
		/// </summary>
		/// 
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">The key of the hash in redis</param>
		/// <param name="key">The key of the field in the hash</param>
		/// <param name="nx">Behave like hsetnx - set only if not exists</param>
		/// <param name="value">The value to be inserted</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>
		///     <c>true</c> if field is a new field in the hash and value was set.
		///     <c>false</c> if field already exists in the hash and no operation was performed.
		/// </returns>
		bool HashSet<T>(string hashKey, string key, T value, bool nx = false, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Sets the specified fields to their respective values in the hash stored at key. This command overwrites any existing fields in the hash. If key does not exist, a new key holding a hash is created.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the number of fields being set.
		/// </remarks>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="values"></param>
		/// <param name="commandFlags">Command execution flags</param>
		void HashSet<T>(string hashKey, Dictionary<string, T> values, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Returns all values in the hash stored at key.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the size of the hash.
		/// </remarks>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>list of values in the hash, or an empty list when key does not exist.</returns>
		IEnumerable<T> HashValues<T>(string hashKey, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     iterates fields of Hash types and their associated values.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(1) for every call. O(N) for a complete iteration, including enough command calls for the cursor to return back to 0. 
		///     N is the number of elements inside the collection.
		/// </remarks>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="pattern">GLOB search pattern</param>
		/// <param name="pageSize">Number of elements to retrieve from the redis server in the cursor</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns></returns>
		Dictionary<string, T> HashScan<T>(string hashKey, string pattern, int pageSize = 10, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Removes the specified fields from the hash stored at key. 
		///     Specified fields that do not exist within this hash are ignored. 
		/// </summary>
		/// <remarks>
		///     Time complexity: O(1)
		/// </remarks>

		/// <param name="hashKey">Key of the hash</param>
		/// <param name="key">Key of the entry</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>
		///     If key is deleted returns true.
		///     If key does not exist, it is treated as an empty hash and this command returns false.
		/// </returns>
		Task<bool> HashDeleteAsync(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Removes the specified fields from the hash stored at key. 
		///     Specified fields that do not exist within this hash are ignored. 
		///     If key does not exist, it is treated as an empty hash and this command returns 0.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the number of fields to be removed.
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="keys">Keys to retrieve from the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>Tthe number of fields that were removed from the hash, not including specified but non existing fields.</returns>
		Task<long> HashDeleteAsync(string hashKey, IEnumerable<string> keys, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Returns if field is an existing field in the hash stored at key.
		/// </summary>
		/// 
		/// <remarks>
		///     Time complexity: O(1)
		/// </remarks>
		/// <param name="hashKey">The key of the hash in redis</param>
		/// <param name="key">The key of the field in the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>Returns if field is an existing field in the hash stored at key.</returns>
		Task<bool> HashExistsAsync(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Returns the value associated with field in the hash stored at key.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(1)
		/// </remarks>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="key">Key of the entry</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>the value associated with field, or nil when field is not present in the hash or key does not exist.</returns>
		Task<T> HashGetAsync<T>(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Returns the values associated with the specified fields in the hash stored at key.
		///     For every field that does not exist in the hash, a nil value is returned. 
		///     Because a non-existing keys are treated as empty hashes, running HMGET against a non-existing key will return a list of nil values.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the number of fields being requested.
		/// </remarks>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="keys">Keys to retrieve from the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>list of values associated with the given fields, in the same order as they are requested.</returns>
		Task<Dictionary<string, T>> HashGetAsync<T>(string hashKey, IEnumerable<string> keys, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Returns all fields and values of the hash stored at key. In the returned value, 
		///     every field name is followed by its value, so the length of the reply is twice the size of the hash.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the size of the hash.
		/// </remarks>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>list of fields and their values stored in the hash, or an empty list when key does not exist.</returns>
		Task<Dictionary<string, T>> HashGetAllAsync<T>(string hashKey, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Increments the number stored at field in the hash stored at key by increment. 
		///     If key does not exist, a new key holding a hash is created. 
		///     If field does not exist the value is set to 0 before the operation is performed.
		///     The range of values supported by HINCRBY is limited to 64 bit signed integers.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(1)
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="key">Key of the entry</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <param name="value">the value at field after the increment operation</param>
		Task<long> HashIncerementByAsync(string hashKey, string key, long value, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Increment the specified field of an hash stored at key, and representing a floating point number, by the specified increment. 
		///     If the field does not exist, it is set to 0 before performing the operation.
		/// </summary>
		/// <remarks>
		///     <para>
		///         An error is returned if one of the following conditions occur:
		///         * The field contains a value of the wrong type (not a string).
		///         * The current field content or the specified increment are not parsable as a double precision floating point number.
		///     </para>
		///     <para>
		///         Time complexity: O(1)
		///     </para>
		///     
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="key">Key of the entry</param>
		/// <param name="value">the value at field after the increment operation</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>the value at field after the increment operation.</returns>
		Task<double> HashIncerementByAsync(string hashKey, string key, double value, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Returns all field names in the hash stored at key.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the size of the hash.
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>list of fields in the hash, or an empty list when key does not exist.</returns>
		Task<IEnumerable<string>> HashKeysAsync(string hashKey, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Returns the number of fields contained in the hash stored at key.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(1)
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>number of fields in the hash, or 0 when key does not exist.</returns>
		Task<long> HashLengthAsync(string hashKey, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Sets field in the hash stored at key to value. If key does not exist, a new key holding a hash is created. If field already exists in the hash, it is overwritten.
		/// </summary>
		/// 
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">The key of the hash in redis</param>
		/// <param name="key">The key of the field in the hash</param>
		/// <param name="nx">Behave like hsetnx - set only if not exists</param>
		/// <param name="value">The value to be inserted</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>
		///     <c>true</c> if field is a new field in the hash and value was set.
		///     <c>false</c> if field already exists in the hash and no operation was performed.
		/// </returns>
		Task<bool> HashSetAsync<T>(string hashKey, string key, T value, bool nx = false, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Sets the specified fields to their respective values in the hash stored at key. 
		///     This command overwrites any existing fields in the hash. 
		///     If key does not exist, a new key holding a hash is created.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the number of fields being set.
		/// </remarks>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="values">The values to be inserted</param>
		/// <param name="commandFlags">Command execution flags</param>
		Task HashSetAsync<T>(string hashKey, IDictionary<string, T> values, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     Returns all values in the hash stored at key.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the size of the hash.
		/// </remarks>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>list of values in the hash, or an empty list when key does not exist.</returns>
		Task<IEnumerable<T>> HashValuesAsync<T>(string hashKey, CommandFlags commandFlags = CommandFlags.None);

		/// <summary>
		///     iterates fields of Hash types and their associated values.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(1) for every call. O(N) for a complete iteration, including enough command calls for the cursor to return back to 0. 
		///     N is the number of elements inside the collection.
		/// </remarks>
		/// <typeparam name="T">Type of the returned value</typeparam>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="pattern">GLOB search pattern</param>
		/// <param name="pageSize">Number of elements to retrieve from the redis server in the cursor</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns></returns> 
		Task<Dictionary<string, T>> HashScanAsync<T>(string hashKey, string pattern, int pageSize = 10, CommandFlags commandFlags = CommandFlags.None);

        /// <summary>
        /// Updates the expiry time of a redis cache object
        /// </summary>
        /// <param name="key">The key of the object</param>
        /// <param name="expiresAt">The new expiry time of the object</param>
        /// <returns>True if the object is updated, false if the object does not exist</returns>
        bool UpdateExpiry(string key, DateTimeOffset expiresAt);

        /// <summary>
        /// Updates the expiry time of a redis cache object
        /// </summary>
        /// <param name="key">The key of the object</param>
        /// <param name="expiresIn">Time until the object will expire</param>
        /// <returns>True if the object is updated, false if the object does not exist</returns>
        bool UpdateExpiry(string key, TimeSpan expiresIn);

        /// <summary>
        /// Updates the expiry time of a redis cache object
        /// </summary>
        /// <param name="key">The key of the object</param>
        /// <param name="expiresAt">The new expiry time of the object</param>
        /// <returns>True if the object is updated, false if the object does not exist</returns>
        Task<bool> UpdateExpiryAsync(string key, DateTimeOffset expiresAt);

        /// <summary>
        /// Updates the expiry time of a redis cache object
        /// </summary>
        /// <param name="key">The key of the object</param>
        /// <param name="expiresIn">Time until the object will expire</param>
        /// <returns>True if the object is updated, false if the object does not exist</returns>
        Task<bool> UpdateExpiryAsync(string key, TimeSpan expiresIn);

        /// <summary>
        /// Updates the expiry time of a redis cache object
        /// </summary>
        /// <param name="keys">An array of keys to be updated</param>
        /// <param name="expiresAt">The new expiry time of the object</param>
        /// <returns>An array of type bool, where true if the object is updated and false if the object does not exist at the same index as the input keys</returns>
        IDictionary<string, bool> UpdateExpiryAll(string[] keys, DateTimeOffset expiresAt);

        /// <summary>
        /// Updates the expiry time of a redis cache object
        /// </summary>
        /// <param name="keys">An array of keys to be updated</param>
        /// <param name="expiresIn">Time until the object will expire</param>
        /// <returns>An IDictionary object that contains the origional key and the result of the operation</returns>
        IDictionary<string, bool> UpdateExpiryAll(string[] keys, TimeSpan expiresIn);

        /// <summary>
        /// Updates the expiry time of a redis cache object
        /// </summary>
        /// <param name="keys">An array of keys to be updated</param>
        /// <param name="expiresAt">The new expiry time of the object</param>
        /// <returns>An array of type bool, where true if the object is updated and false if the object does not exist at the same index as the input keys</returns>
        Task<IDictionary<string, bool>> UpdateExpiryAllAsync(string[] keys, DateTimeOffset expiresAt);

        /// <summary>
        /// Updates the expiry time of a redis cache object
        /// </summary>
        /// <param name="keys">An array of keys to be updated</param>
        /// <param name="expiresIn">Time until the object will expire</param>
        /// <returns>An IDictionary object that contains the origional key and the result of the operation</returns>
        Task<IDictionary<string, bool>> UpdateExpiryAllAsync(string[] keys, TimeSpan expiresIn);
    }
}