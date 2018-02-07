using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis.Extensions.Core.ServerIteration;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Extensions;
using StackExchange.Redis.KeyspaceIsolation;

namespace StackExchange.Redis.Extensions.Core
{
	/// <summary>
	///     The implementation of <see cref="ICacheClient" />
	/// </summary>
	public class StackExchangeRedisCacheClient : ICacheClient
	{
		private string keyprefix = null;
		private readonly IConnectionMultiplexer connectionMultiplexer;
		private readonly ServerEnumerationStrategy serverEnumerationStrategy = new ServerEnumerationStrategy();

		/// <summary>
		///     Initializes a new instance of the <see cref="StackExchangeRedisCacheClient" /> class.
		/// </summary>
		/// <param name="serializer">The serializer.</param>
		/// <param name="configuration">The configuration.</param>
		public StackExchangeRedisCacheClient(ISerializer serializer, RedisConfiguration configuration)
		{
			if (serializer == null)
			{
				throw new ArgumentNullException(nameof(serializer), "The serializer could not be null.");
			}

			if (configuration == null)
			{
				throw new ArgumentNullException(nameof(configuration), "The configuration could not be null");
			}

			var options = new ConfigurationOptions
			{
				Ssl = configuration.Ssl,
				AllowAdmin = configuration.AllowAdmin,
				Password = configuration.Password,
				AbortOnConnectFail = configuration.AbortOnConnectFail,
				ConnectTimeout = configuration.ConnectTimeout,
			};
			serverEnumerationStrategy = configuration.ServerEnumerationStrategy;

			foreach (RedisHost redisHost in configuration.Hosts)
			{
				options.EndPoints.Add(redisHost.Host, redisHost.Port);
			}

			connectionMultiplexer = ConnectionMultiplexer.Connect(options);
			Database = connectionMultiplexer.GetDatabase(configuration.Database);

			if (!string.IsNullOrWhiteSpace(configuration.KeyPrefix))
				Database = Database.WithKeyPrefix(configuration.KeyPrefix);

			Serializer = serializer;
			keyprefix = configuration.KeyPrefix;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="StackExchangeRedisCacheClient" /> class.
		/// </summary>
		/// <param name="serializer">The serializer.</param>
		/// <param name="connectionString">The connection string.</param>
		/// <param name="keyPrefix">Specifies the key separation prefix to be used for all keys</param>
		/// <exception cref="System.ArgumentNullException">serializer</exception>
		public StackExchangeRedisCacheClient(ISerializer serializer, string connectionString, string keyPrefix)
			: this(serializer, connectionString, 0, keyPrefix)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="StackExchangeRedisCacheClient" /> class.
		/// </summary>
		/// <param name="serializer">The serializer.</param>
		/// <param name="connectionString">The connection string.</param>
		/// <param name="database">The database.</param>
		/// <param name="keyPrefix">Specifies the key separation prefix to be used for all keys</param>
		/// <exception cref="System.ArgumentNullException">serializer</exception>
		public StackExchangeRedisCacheClient(ISerializer serializer, string connectionString, int database = 0,
			string keyPrefix = null)
		{
			if (serializer == null)
			{
				throw new ArgumentNullException(nameof(serializer));
			}

			Serializer = serializer;
			connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
			Database = connectionMultiplexer.GetDatabase(database);

			if (!string.IsNullOrWhiteSpace(keyPrefix))
				Database = Database.WithKeyPrefix(keyPrefix);

			keyprefix = keyPrefix;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="StackExchangeRedisCacheClient" /> class.
		/// </summary>
		/// <param name="connectionMultiplexer">The connection multiplexer.</param>
		/// <param name="serializer">The serializer.</param>
		/// <param name="keyPrefix">Specifies the key separation prefix to be used for all keys</param>
		/// <exception cref="System.ArgumentNullException">
		///     connectionMultiplexer
		///     or
		///     serializer
		/// </exception>
		public StackExchangeRedisCacheClient(IConnectionMultiplexer connectionMultiplexer, ISerializer serializer, string keyPrefix)
			: this(connectionMultiplexer, serializer, 0, keyPrefix)
		{
		}

        /// <summary>
		///     Initializes a new instance of the <see cref="StackExchangeRedisCacheClient" /> class.
		/// </summary>
		/// <param name="connectionMultiplexer">The connection multiplexer.</param>
		/// <param name="serializer">The serializer.</param>
		/// <param name="serverEnumerationStrategy">The strategy to use when executing server wide commands.</param>
		/// <param name="keyPrefix">Specifies the key separation prefix to be used for all keys</param>
		/// <exception cref="System.ArgumentNullException">
		///     connectionMultiplexer
		///     or
		///     serializer
		/// </exception>
        public StackExchangeRedisCacheClient(IConnectionMultiplexer connectionMultiplexer, ISerializer serializer, ServerEnumerationStrategy serverEnumerationStrategy, string keyPrefix)
            : this(connectionMultiplexer, serializer, 0, keyPrefix)
        {
            this.serverEnumerationStrategy = serverEnumerationStrategy;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="StackExchangeRedisCacheClient" /> class.
        /// </summary>
        /// <param name="connectionMultiplexer">The connection multiplexer.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="database">The database.</param>
        /// <param name="keyPrefix">Specifies the key separation prefix to be used for all keys</param>
        /// <exception cref="System.ArgumentNullException">
        ///     connectionMultiplexer
        ///     or
        ///     serializer
        /// </exception>
        public StackExchangeRedisCacheClient(IConnectionMultiplexer connectionMultiplexer, ISerializer serializer,
			int database = 0, string keyPrefix = null)
		{
			if (connectionMultiplexer == null)
			{
				throw new ArgumentNullException(nameof(connectionMultiplexer));
			}

			if (serializer == null)
			{
				throw new ArgumentNullException(nameof(serializer));
			}

			Serializer = serializer;
			this.connectionMultiplexer = connectionMultiplexer;

			Database = connectionMultiplexer.GetDatabase(database);

			if (!string.IsNullOrWhiteSpace(keyPrefix))
				Database = Database.WithKeyPrefix(keyPrefix);

			keyprefix = keyPrefix;
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			connectionMultiplexer.Dispose();
		}

		/// <summary>
		///     Return the instance of <see cref="StackExchange.Redis.IDatabase" /> used be ICacheClient implementation
		/// </summary>
		public IDatabase Database { get; }

		/// <summary>
		///     Gets the serializer.
		/// </summary>
		/// <value>
		///     The serializer.
		/// </value>
		public ISerializer Serializer { get; }

		/// <summary>
		///     Verify that the specified cache key exists
		/// </summary>
		/// <param name="key">The cache key.</param>
		/// <returns>
		///     True if the key is present into Redis. Othwerwise False
		/// </returns>
		public bool Exists(string key)
		{
			return Database.KeyExists(key);
		}

		/// <summary>
		///     Verify that the specified cache key exists
		/// </summary>
		/// <param name="key">The cache key.</param>
		/// <returns>
		///     True if the key is present into Redis. Othwerwise False
		/// </returns>
		public Task<bool> ExistsAsync(string key)
		{
			return Database.KeyExistsAsync(key);
		}

		/// <summary>
		///     Removes the specified key from Redis Database
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     True if the key has removed. Othwerwise False
		/// </returns>
		public bool Remove(string key)
		{
			return Database.KeyDelete(key);
		}

		/// <summary>
		///     Removes the specified key from Redis Database
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		///     True if the key has removed. Othwerwise False
		/// </returns>
		public Task<bool> RemoveAsync(string key)
		{
			return Database.KeyDeleteAsync(key);
		}

		/// <summary>
		///     Removes all specified keys from Redis Database
		/// </summary>
		/// <param name="keys">The key.</param>
		public void RemoveAll(IEnumerable<string> keys)
		{
			var redisKeys = keys.Select(x => (RedisKey)x).ToArray();
			Database.KeyDelete(redisKeys);
		}

		/// <summary>
		///     Removes all specified keys from Redis Database
		/// </summary>
		/// <param name="keys">The key.</param>
		/// <returns></returns>
		public Task RemoveAllAsync(IEnumerable<string> keys)
		{
			var redisKeys = keys.Select(x => (RedisKey)x).ToArray();
			return Database.KeyDeleteAsync(redisKeys);
		}

		/// <summary>
		///     Get the object with the specified key from Redis database
		/// </summary>
		/// <typeparam name="T">The type of the expected object</typeparam>
		/// <param name="key">The cache key.</param>
		/// <returns>
		///     Null if not present, otherwise the instance of T.
		/// </returns>
		public T Get<T>(string key)
		{
			var valueBytes = Database.StringGet(key);

			if (!valueBytes.HasValue)
			{
				return default(T);
			}

			return Serializer.Deserialize<T>(valueBytes);
		}

        /// <summary>
        ///     Get the object with the specified key from Redis database and update the expiry time
        /// </summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="expiresAt">Expiration time.</param>
        /// <returns>
        ///     Null if not present, otherwise the instance of T.
        /// </returns>
        public T Get<T>(string key, DateTimeOffset expiresAt)
        {
            var result = Get<T>(key);
            
            if (!Equals(result, default(T)))
            {
                Database.KeyExpire(key, expiresAt.Subtract(DateTime.Now));
            }

            return result;
        }

        /// <summary>
        ///     Get the object with the specified key from Redis database and update the expiry time
        /// </summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="expiresIn">Time till the object expires.</param>
        /// <returns>
        ///     Null if not present, otherwise the instance of T.
        /// </returns>
        public T Get<T>(string key, TimeSpan expiresIn)
        {
            var result = Get<T>(key);

            if (!Equals(result, default(T)))
            {
                Database.KeyExpire(key, expiresIn);
            }

            return result;
        }

        /// <summary>
        ///     Get the object with the specified key from Redis database
        /// </summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="key">The cache key.</param>
        /// <returns>
        ///     Null if not present, otherwise the instance of T.
        /// </returns>
        public async Task<T> GetAsync<T>(string key)
		{
			var valueBytes = await Database.StringGetAsync(key);

			if (!valueBytes.HasValue)
			{
				return default(T);
			}

			return await Serializer.DeserializeAsync<T>(valueBytes);
		}

        /// <summary>
        ///     Get the object with the specified key from Redis database and update the expiry time
        /// </summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="expiresAt">Expiration time.</param>
        /// <returns>
        ///     Null if not present, otherwise the instance of T.
        /// </returns>
        public async Task<T> GetAsync<T>(string key, DateTimeOffset expiresAt)
        {
            var result = await GetAsync<T>(key);

            if (!Equals(result, default(T)))
            {
                await Database.KeyExpireAsync(key, expiresAt.Subtract(DateTime.Now));
            }

            return default(T);
        }

        /// <summary>
        ///     Get the object with the specified key from Redis database and update the expiry time
        /// </summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="expiresIn">Time till the object expires.</param>
        /// <returns>
        ///     Null if not present, otherwise the instance of T.
        /// </returns>
        public async Task<T> GetAsync<T>(string key, TimeSpan expiresIn)
        {
            var result = await GetAsync<T>(key);

            if (!Equals(result, default(T)))
            {
                await Database.KeyExpireAsync(key, expiresIn);
            }

            return default(T);
        }

        /// <summary>
        ///     Adds the specified instance to the Redis database.
        /// </summary>
        /// <typeparam name="T">The type of the class to add to Redis</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The instance of T.</param>
        /// <returns>
        ///     True if the object has been added. Otherwise false
        /// </returns>
        public bool Add<T>(string key, T value)
		{
			var entryBytes = Serializer.Serialize(value);

			return Database.StringSet(key, entryBytes);
		}

		/// <summary>
		///     Adds the specified instance to the Redis database.
		/// </summary>
		/// <typeparam name="T">The type of the class to add to Redis</typeparam>
		/// <param name="key">The cache key.</param>
		/// <param name="value">The instance of T.</param>
		/// <returns>
		///     True if the object has been added. Otherwise false
		/// </returns>
		public async Task<bool> AddAsync<T>(string key, T value)
		{
			var entryBytes = await Serializer.SerializeAsync(value);

			return await Database.StringSetAsync(key, entryBytes);
		}

		/// <summary>
		///     Replaces the object with specified key into Redis database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <param name="value">The instance of T</param>
		/// <returns>
		///     True if the object has been added. Otherwise false
		/// </returns>
		public bool Replace<T>(string key, T value)
		{
			return Add(key, value);
		}

		/// <summary>
		///     Replaces the object with specified key into Redis database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <param name="value">The instance of T</param>
		/// <returns>
		///     True if the object has been added. Otherwise false
		/// </returns>
		public Task<bool> ReplaceAsync<T>(string key, T value)
		{
			return AddAsync(key, value);
		}

		/// <summary>
		///     Adds the specified instance to the Redis database.
		/// </summary>
		/// <typeparam name="T">The type of the class to add to Redis</typeparam>
		/// <param name="key">The cache key.</param>
		/// <param name="value">The instance of T.</param>
		/// <param name="expiresAt">Expiration time.</param>
		/// <returns>
		///     True if the object has been added. Otherwise false
		/// </returns>
		public bool Add<T>(string key, T value, DateTimeOffset expiresAt)
		{
			var entryBytes = Serializer.Serialize(value);
			var expiration = expiresAt.Subtract(DateTimeOffset.Now);

			return Database.StringSet(key, entryBytes, expiration);
		}

		/// <summary>
		///     Adds the specified instance to the Redis database.
		/// </summary>
		/// <typeparam name="T">The type of the class to add to Redis</typeparam>
		/// <param name="key">The cache key.</param>
		/// <param name="value">The instance of T.</param>
		/// <param name="expiresAt">Expiration time.</param>
		/// <returns>
		///     True if the object has been added. Otherwise false
		/// </returns>
		public async Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt)
		{
			var entryBytes = await Serializer.SerializeAsync(value);
			var expiration = expiresAt.Subtract(DateTimeOffset.Now);

			return await Database.StringSetAsync(key, entryBytes, expiration);
		}

		/// <summary>
		///     Replaces the object with specified key into Redis database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <param name="value">The instance of T</param>
		/// <param name="expiresAt">Expiration time.</param>
		/// <returns>
		///     True if the object has been added. Otherwise false
		/// </returns>
		public bool Replace<T>(string key, T value, DateTimeOffset expiresAt)
		{
			return Add(key, value, expiresAt);
		}

		/// <summary>
		///     Replaces the object with specified key into Redis database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <param name="value">The instance of T</param>
		/// <param name="expiresAt">Expiration time.</param>
		/// <returns>
		///     True if the object has been added. Otherwise false
		/// </returns>
		public Task<bool> ReplaceAsync<T>(string key, T value, DateTimeOffset expiresAt)
		{
			return AddAsync(key, value, expiresAt);
		}

		/// <summary>
		///     Adds the specified instance to the Redis database.
		/// </summary>
		/// <typeparam name="T">The type of the class to add to Redis</typeparam>
		/// <param name="key">The cache key.</param>
		/// <param name="value">The instance of T.</param>
		/// <param name="expiresIn">The duration of the cache using Timespan.</param>
		/// <returns>
		///     True if the object has been added. Otherwise false
		/// </returns>
		public bool Add<T>(string key, T value, TimeSpan expiresIn)
		{
			var entryBytes = Serializer.Serialize(value);

			return Database.StringSet(key, entryBytes, expiresIn);
		}

		/// <summary>
		///     Adds the specified instance to the Redis database.
		/// </summary>
		/// <typeparam name="T">The type of the class to add to Redis</typeparam>
		/// <param name="key">The cache key.</param>
		/// <param name="value">The instance of T.</param>
		/// <param name="expiresIn">The duration of the cache using Timespan.</param>
		/// <returns>
		///     True if the object has been added. Otherwise false
		/// </returns>
		public async Task<bool> AddAsync<T>(string key, T value, TimeSpan expiresIn)
		{
			var entryBytes = await Serializer.SerializeAsync(value);

			return await Database.StringSetAsync(key, entryBytes, expiresIn);
		}

		/// <summary>
		///     Replaces the object with specified key into Redis database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <param name="value">The instance of T</param>
		/// <param name="expiresIn">The duration of the cache using Timespan.</param>
		/// <returns>
		///     True if the object has been added. Otherwise false
		/// </returns>
		public bool Replace<T>(string key, T value, TimeSpan expiresIn)
		{
			return Add(key, value, expiresIn);
		}

		/// <summary>
		///     Replaces the object with specified key into Redis database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <param name="value">The instance of T</param>
		/// <param name="expiresIn">The duration of the cache using Timespan.</param>
		/// <returns>
		///     True if the object has been added. Otherwise false
		/// </returns>
		public Task<bool> ReplaceAsync<T>(string key, T value, TimeSpan expiresIn)
		{
			return AddAsync(key, value, expiresIn);
		}

		/// <summary>
		///     Get the objects with the specified keys from Redis database with one roundtrip
		/// </summary>
		/// <typeparam name="T">The type of the expected object</typeparam>
		/// <param name="keys">The keys.</param>
		/// <returns>
		///     Empty list if there are no results, otherwise the instance of T.
		///     If a cache key is not present on Redis the specified object into the returned Dictionary will be null
		/// </returns>
		public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys)
		{
			var redisKeys = keys.Select(x => (RedisKey)x).ToArray();
			var result = Database.StringGet(redisKeys);

			var dict = new Dictionary<string, T>(StringComparer.Ordinal);
			for (var index = 0; index < redisKeys.Length; index++)
			{
				var value = result[index];
				dict.Add(redisKeys[index], value == RedisValue.Null ? default(T) : Serializer.Deserialize<T>(value));
			}

			return dict;
		}

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
        public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys, DateTimeOffset expiresAt)
        {
            var result = GetAll<T>(keys);
            UpdateExpiryAll(keys.ToArray(), expiresAt);
            return result;
        }

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
        public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys, TimeSpan expiresIn)
        {
            var result = GetAll<T>(keys);
            UpdateExpiryAll(keys.ToArray(), expiresIn);
            return result;
        }

        /// <summary>
        ///     Get the objects with the specified keys from Redis database with one roundtrip
        /// </summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="keys">The keys.</param>
        /// <returns>
        ///     Empty list if there are no results, otherwise the instance of T.
        ///     If a cache key is not present on Redis the specified object into the returned Dictionary will be null
        /// </returns>
        public async Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys)
		{
			var redisKeys = keys.Select(x => (RedisKey)x).ToArray();
			var result = await Database.StringGetAsync(redisKeys);
			var dict = new Dictionary<string, T>(StringComparer.Ordinal);
			for (var index = 0; index < redisKeys.Length; index++)
			{
				var value = result[index];
				dict.Add(redisKeys[index], value == RedisValue.Null ? default(T) : Serializer.Deserialize<T>(value));
			}
			return dict;
        }

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
        public async Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys, DateTimeOffset expiresAt)
        {
            var result = await GetAllAsync<T>(keys);
            await UpdateExpiryAllAsync(keys.ToArray(), expiresAt);
            return result;
        }

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
        public async Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys, TimeSpan expiresIn)
        {
            var result = await GetAllAsync<T>(keys);
            await UpdateExpiryAllAsync(keys.ToArray(), expiresIn);
            return result;
        }

        /// <summary>
        ///     Adds all.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        public bool AddAll<T>(IList<Tuple<string, T>> items)
        {
            var values = items
                .Select(item => new KeyValuePair<RedisKey, RedisValue>(item.Item1, Serializer.Serialize(item.Item2)))
                .ToArray();

            return Database.StringSet(values);
        }

        /// <summary>
        ///     Adds all asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public async Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items)
        {
            var values = items
                .Select(item => new KeyValuePair<RedisKey, RedisValue>(item.Item1, Serializer.Serialize(item.Item2)))
                .ToArray();

            return await Database.StringSetAsync(values);
        }

        /// <summary>
        ///     Adds all.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        public bool AddAll<T>(IList<Tuple<string, T>> items, DateTimeOffset expiresAt)
        {
            var values = items
                .Select(item => new KeyValuePair<RedisKey, RedisValue>(item.Item1, Serializer.Serialize(item.Item2)))
                .ToArray();

            var result = Database.StringSet(values);

            foreach(var value in values)
            {
                Database.KeyExpire(value.Key, expiresAt.DateTime);
            }

            return result;
        }

        /// <summary>
        ///     Adds all asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public async Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, DateTimeOffset expiresAt)
        {
            var values = items
                .Select(item => new KeyValuePair<RedisKey, RedisValue>(item.Item1, Serializer.Serialize(item.Item2)))
                .ToArray();

            var result = await Database.StringSetAsync(values);

            Parallel.ForEach(values, async value => await Database.KeyExpireAsync(value.Key, expiresAt.DateTime));

            return result;
        }

        /// <summary>
        ///     Adds all.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        public bool AddAll<T>(IList<Tuple<string, T>> items, TimeSpan expiresOn)
        {
            var values = items
                .Select(item => new KeyValuePair<RedisKey, RedisValue>(item.Item1, Serializer.Serialize(item.Item2)))
                .ToArray();

            var result = Database.StringSet(values);

            foreach (var value in values)
            {
                Database.KeyExpire(value.Key, expiresOn);
            }

            return result;
        }

        /// <summary>
        ///     Adds all asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public async Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items, TimeSpan expiresOn)
        {
            var values = items
                .Select(item => new KeyValuePair<RedisKey, RedisValue>(item.Item1, Serializer.Serialize(item.Item2)))
                .ToArray();

            var result = await Database.StringSetAsync(values);

            Parallel.ForEach(values, async value => await Database.KeyExpireAsync(value.Key, expiresOn));

            return result;
        }

        /// <summary>
        ///     Run SADD command http://redis.io/commands/sadd
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool SetAdd<T>(string key, T item) where T : class
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException("key cannot be empty.", nameof(key));
			}

			if (item == null)
			{
				throw new ArgumentNullException(nameof(item), "item cannot be null.");
			}

			var serializedObject = Serializer.Serialize(item);

			return Database.SetAdd(key, serializedObject);
		}

		/// <summary>
		///     Run SADD command http://redis.io/commands/sadd"
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public async Task<bool> SetAddAsync<T>(string key, T item) where T : class
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException("key cannot be empty.", nameof(key));
			}

			if (item == null)
			{
				throw new ArgumentNullException(nameof(item), "item cannot be null.");
			}

			var serializedObject = await Serializer.SerializeAsync(item);

			return await Database.SetAddAsync(key, serializedObject);
		}

		/// <summary>
		/// Run SADD command http://redis.io/commands/sadd
		/// </summary>
		/// <param name="items">Name of the member.</param>
		/// <param name="key">The key.</param>
		public long SetAddAll<T>(string key, params T[] items) where T : class
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException("key cannot be empty.", nameof(key));
			}

			if (items == null)
			{
				throw new ArgumentNullException(nameof(items), "items cannot be null.");
			}

			if (items.Any((item) => item == null))
			{
				throw new ArgumentException("items cannot contains any null item.", nameof(items));
			}

			return Database.SetAdd(key, items.Select(item => Serializer.Serialize(item)).Select((x) => (RedisValue)x).ToArray());
		}

		/// <summary>
		/// Run SADD command http://redis.io/commands/sadd
		/// </summary>
		/// <param name="items">Name of the member.</param>
		/// <param name="key">The key.</param>
		public async Task<long> SetAddAllAsync<T>(string key, params T[] items) where T : class
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException("key cannot be empty.", nameof(key));
			}

			if (items == null)
			{
				throw new ArgumentNullException(nameof(items), "items cannot be null.");
			}

			if (items.Any((item) => item == null))
			{
				throw new ArgumentException("items cannot contains any null item.", nameof(items));
			}

			return await Database.SetAddAsync(key, items.Select(item => Serializer.Serialize(item)).Select((x) => (RedisValue)x).ToArray());
		}

		/// <summary>
		///     Run SREM command http://redis.io/commands/srem
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool SetRemove<T>(string key, T item) where T : class
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException("key cannot be empty.", nameof(key));
			}

			if (item == null)
			{
				throw new ArgumentNullException(nameof(item), "item cannot be null.");
			}

			var serializedObject = Serializer.Serialize(item);

			return Database.SetRemove(key, serializedObject);
		}

		/// <summary>
		///     Run SREM command http://redis.io/commands/srem"
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public async Task<bool> SetRemoveAsync<T>(string key, T item) where T : class
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException("key cannot be empty.", nameof(key));
			}

			if (item == null)
			{
				throw new ArgumentNullException(nameof(item), "item cannot be null.");
			}

			var serializedObject = await Serializer.SerializeAsync(item);

			return await Database.SetRemoveAsync(key, serializedObject);
		}

		/// <summary>
		///     Run SREM command http://redis.io/commands/srem
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="items"></param>
		/// <returns></returns>
		public long SetRemoveAll<T>(string key, params T[] items) where T : class
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException("key cannot be empty.", nameof(key));
			}

			if (items == null)
			{
				throw new ArgumentNullException(nameof(items), "items cannot be null.");
			}

			if (items.Any((item) => item == null))
			{
				throw new ArgumentException("items cannot contains any null item.", nameof(items));
			}

			return Database.SetRemove(key, items.Select(item => Serializer.Serialize(item)).Select((x) => (RedisValue)x).ToArray());
		}

		/// <summary>
		///     Run SREM command http://redis.io/commands/srem
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="items"></param>
		/// <returns></returns>
		public async Task<long> SetRemoveAllAsync<T>(string key, params T[] items) where T : class
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException("key cannot be empty.", nameof(key));
			}

			if (items == null)
			{
				throw new ArgumentNullException(nameof(items), "items cannot be null.");
			}

			if (items.Any((item) => item == null))
			{
				throw new ArgumentException("items cannot contains any null item.", nameof(items));
			}

			return await Database.SetRemoveAsync(key, items.Select(item => Serializer.Serialize(item)).Select((x) => (RedisValue)x).ToArray());
		}

		/// <summary>
		///     Run SMEMBERS command http://redis.io/commands/SMEMBERS
		/// </summary>
		/// <param name="memberName">Name of the member.</param>
		/// <returns></returns>
		public string[] SetMember(string memberName)
		{
			return Database.SetMembers(memberName).Select(x => x.ToString()).ToArray();
		}

		/// <summary>
		///     Run SMEMBERS command see http://redis.io/commands/SMEMBERS
		/// </summary>
		/// <param name="memberName">Name of the member.</param>
		/// <returns></returns>
		public async Task<string[]> SetMemberAsync(string memberName)
		{
			return (await Database.SetMembersAsync(memberName)).Select(x => x.ToString()).ToArray();
		}

		/// <summary>
		///     Run SMEMBERS command see http://redis.io/commands/SMEMBERS
		///     Deserializes the results to T
		/// </summary>
		/// <typeparam name="T">The type of the expected objects in the set</typeparam>
		/// <param name="key">The key</param>
		/// <returns>An array of objects in the set</returns>
		public IEnumerable<T> SetMembers<T>(string key)
		{
			var members = Database.SetMembers(key);
			return members.Select(m => m == RedisValue.Null ? default(T) : Serializer.Deserialize<T>(m));
		}

		/// <summary>
		///     Run SMEMBERS command see http://redis.io/commands/SMEMBERS
		///     Deserializes the results to T
		/// </summary>
		/// <typeparam name="T">The type of the expected objects</typeparam>
		/// <param name="key">The key</param>
		/// <returns>An array of objects in the set</returns>
		public async Task<IEnumerable<T>> SetMembersAsync<T>(string key)
		{
			var members = await Database.SetMembersAsync(key);

			return members.Select(m => m == RedisValue.Null ? default(T) : Serializer.Deserialize<T>(m));
		}

		/// <summary>
		///     Searches the keys from Redis database
		/// </summary>
		/// <remarks>
		///     Consider this as a command that should only be used in production environments with extreme care. It may ruin
		///     performance when it is executed against large databases
		/// </remarks>
		/// <param name="pattern">The pattern.</param>
		/// <example>
		///     if you want to return all keys that start with "myCacheKey" uses "myCacheKey*"
		///     if you want to return all keys that contain with "myCacheKey" uses "*myCacheKey*"
		///     if you want to return all keys that end with "myCacheKey" uses "*myCacheKey"
		/// </example>
		/// <returns>A list of cache keys retrieved from Redis database</returns>
		public IEnumerable<string> SearchKeys(string pattern)
		{
			pattern = $"{keyprefix}{pattern}";
			var keys = new HashSet<RedisKey>();

			var multiplexer = Database.Multiplexer;
			var servers = ServerIteratorFactory.GetServers(multiplexer, serverEnumerationStrategy).ToArray();
			if (!servers.Any())
			{
				throw new Exception("No server found to serve the KEYS command.");
			}

			foreach (var server in servers)
			{
				var dbKeys = server.Keys(Database.Database, pattern);
				foreach (var dbKey in dbKeys)
				{
					if (!keys.Contains(dbKey))
					{
						keys.Add(dbKey);
					}
				}
			}

			return keys.Select(x => (string)x);
		}

		/// <summary>
		///     Searches the keys from Redis database
		/// </summary>
		/// <remarks>
		///     Consider this as a command that should only be used in production environments with extreme care. It may ruin
		///     performance when it is executed against large databases
		/// </remarks>
		/// <param name="pattern">The pattern.</param>
		/// <example>
		///     if you want to return all keys that start with "myCacheKey" uses "myCacheKey*"
		///     if you want to return all keys that contain with "myCacheKey" uses "*myCacheKey*"
		///     if you want to return all keys that end with "myCacheKey" uses "*myCacheKey"
		/// </example>
		/// <returns>A list of cache keys retrieved from Redis database</returns>
		public Task<IEnumerable<string>> SearchKeysAsync(string pattern)
		{
			return Task.Factory.StartNew(() => SearchKeys(pattern));
		}

		/// <summary>
		///     Flushes the database.
		/// </summary>
		public void FlushDb()
		{
			var endPoints = Database.Multiplexer.GetEndPoints();

			foreach (var endpoint in endPoints)
			{
				Database.Multiplexer.GetServer(endpoint).FlushDatabase(Database.Database);
			}
		}

		/// <summary>
		///     Flushes the database asynchronous.
		/// </summary>
		/// <returns></returns>
		public async Task FlushDbAsync()
		{
			var endPoints = Database.Multiplexer.GetEndPoints();

			foreach (var endpoint in endPoints)
			{
				await Database.Multiplexer.GetServer(endpoint).FlushDatabaseAsync(Database.Database);
			}
		}

		/// <summary>
		///     Save the DB in background.
		/// </summary>
		/// <param name="saveType"></param>
		public void Save(SaveType saveType)
		{
			var endPoints = Database.Multiplexer.GetEndPoints();

			foreach (var endpoint in endPoints)
			{
				Database.Multiplexer.GetServer(endpoint).Save(saveType);
			}
		}

		/// <summary>
		///     Save the DB in background asynchronous.
		/// </summary>
		/// <param name="saveType"></param>
		public async Task SaveAsync(SaveType saveType)
		{
			var endPoints = Database.Multiplexer.GetEndPoints();

			foreach (var endpoint in endPoints)
			{
				await Database.Multiplexer.GetServer(endpoint).SaveAsync(saveType);
			}
		}

		/// <summary>
		///     Gets the information about redis.
		///     More info see http://redis.io/commands/INFO
		/// </summary>
		/// <returns></returns>
		public Dictionary<string, string> GetInfo()
		{
			var info = Database.ScriptEvaluate("return redis.call('INFO')").ToString();

			return ParseInfo(info);
		}

		/// <summary>
		///     Gets the information about redis.
		///     More info see http://redis.io/commands/INFO
		/// </summary>
		/// <returns></returns>
		public async Task<Dictionary<string, string>> GetInfoAsync()
		{
			var info = (await Database.ScriptEvaluateAsync("return redis.call('INFO')")).ToString();

			return ParseInfo(info);
		}

		/// <summary>
		///     Publishes a message to a channel.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="channel"></param>
		/// <param name="message"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public long Publish<T>(RedisChannel channel, T message, CommandFlags flags = CommandFlags.None)
		{
			var sub = connectionMultiplexer.GetSubscriber();
			return sub.Publish(channel, Serializer.Serialize(message), flags);
		}

		/// <summary>
		///     Publishes a message to a channel.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="channel"></param>
		/// <param name="message"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public async Task<long> PublishAsync<T>(RedisChannel channel, T message, CommandFlags flags = CommandFlags.None)
		{
			var sub = connectionMultiplexer.GetSubscriber();
			return await sub.PublishAsync(channel, await Serializer.SerializeAsync(message), flags);
		}

		/// <summary>
		///     Registers a callback handler to process messages published to a channel.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="channel"></param>
		/// <param name="handler"></param>
		/// <param name="flags"></param>
		/// <exception cref="System.ArgumentNullException"></exception>
		public void Subscribe<T>(RedisChannel channel, Action<T> handler, CommandFlags flags = CommandFlags.None)
		{
			if (handler == null)
				throw new ArgumentNullException(nameof(handler));

			var sub = connectionMultiplexer.GetSubscriber();
			sub.Subscribe(channel, (redisChannel, value) => handler(Serializer.Deserialize<T>(value)), flags);
		}

		/// <summary>
		///     Registers a callback handler to process messages published to a channel.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="channel"></param>
		/// <param name="handler"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException"></exception>
		public async Task SubscribeAsync<T>(RedisChannel channel, Func<T, Task> handler,
			CommandFlags flags = CommandFlags.None)
		{
			if (handler == null)
				throw new ArgumentNullException(nameof(handler));

			var sub = connectionMultiplexer.GetSubscriber();
			await
				sub.SubscribeAsync(channel, async (redisChannel, value) => await handler(Serializer.Deserialize<T>(value)), flags);
		}

		/// <summary>
		///     Unregisters a callback handler to process messages published to a channel.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="channel"></param>
		/// <param name="handler"></param>
		/// <param name="flags"></param>
		/// <exception cref="System.ArgumentNullException"></exception>
		public void Unsubscribe<T>(RedisChannel channel, Action<T> handler, CommandFlags flags = CommandFlags.None)
		{
			if (handler == null)
				throw new ArgumentNullException(nameof(handler));

			var sub = connectionMultiplexer.GetSubscriber();
			sub.Unsubscribe(channel, (redisChannel, value) => handler(Serializer.Deserialize<T>(value)), flags);
		}

		/// <summary>
		///     Unregisters a callback handler to process messages published to a channel.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="channel"></param>
		/// <param name="handler"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException"></exception>
		public async Task UnsubscribeAsync<T>(RedisChannel channel, Func<T, Task> handler,
			CommandFlags flags = CommandFlags.None)
		{
			if (handler == null)
				throw new ArgumentNullException(nameof(handler));

			var sub = connectionMultiplexer.GetSubscriber();
			await sub.UnsubscribeAsync(channel, (redisChannel, value) => handler(Serializer.Deserialize<T>(value)), flags);
		}

		/// <summary>
		///     Unregisters all callback handlers on a channel.
		/// </summary>
		/// <param name="flags"></param>
		public void UnsubscribeAll(CommandFlags flags = CommandFlags.None)
		{
			var sub = connectionMultiplexer.GetSubscriber();
			sub.UnsubscribeAll(flags);
		}

		/// <summary>
		///     Unregisters all callback handlers on a channel.
		/// </summary>
		/// <param name="flags"></param>
		/// <returns></returns>
		public async Task UnsubscribeAllAsync(CommandFlags flags = CommandFlags.None)
		{
			var sub = connectionMultiplexer.GetSubscriber();
			await sub.UnsubscribeAllAsync(flags);
		}

		/// <summary>
		///     Insert the specified value at the head of the list stored at key. If key does not exist, it is created as empty
		///     list before performing the push operations.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <param name="item">The item.</param>
		/// <returns>
		///     the length of the list after the push operations.
		/// </returns>
		/// <exception cref="System.ArgumentException">key cannot be empty.;key</exception>
		/// <exception cref="System.ArgumentNullException">item;item cannot be null.</exception>
		/// <remarks>
		///     http://redis.io/commands/lpush
		/// </remarks>
		public long ListAddToLeft<T>(string key, T item) where T : class
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException("key cannot be empty.", nameof(key));
			}

			if (item == null)
			{
				throw new ArgumentNullException(nameof(item), "item cannot be null.");
			}

			var serializedItem = Serializer.Serialize(item);

			return Database.ListLeftPush(key, serializedItem);
		}

		/// <summary>
		///     Lists the add to left asynchronous.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">key cannot be empty.;key</exception>
		/// <exception cref="System.ArgumentNullException">item;item cannot be null.</exception>
		public async Task<long> ListAddToLeftAsync<T>(string key, T item) where T : class
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException("key cannot be empty.", nameof(key));
			}

			if (item == null)
			{
				throw new ArgumentNullException(nameof(item), "item cannot be null.");
			}

			var serializedItem = await Serializer.SerializeAsync(item);

			return await Database.ListLeftPushAsync(key, serializedItem);
		}

		/// <summary>
		///     Removes and returns the last element of the list stored at key.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">key cannot be empty.;key</exception>
		/// <remarks>
		///     http://redis.io/commands/rpop
		/// </remarks>
		public T ListGetFromRight<T>(string key) where T : class
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException("key cannot be empty.", nameof(key));
			}

			var item = Database.ListRightPop(key);

			return item == RedisValue.Null ? null : Serializer.Deserialize<T>(item);
		}

		/// <summary>
		///     Removes and returns the last element of the list stored at key.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">key cannot be empty.;key</exception>
		/// <remarks>
		///     http://redis.io/commands/rpop
		/// </remarks>
		public async Task<T> ListGetFromRightAsync<T>(string key) where T : class
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException("key cannot be empty.", nameof(key));
			}

			var item = await Database.ListRightPopAsync(key);

			if (item == RedisValue.Null) return null;

			return item == RedisValue.Null ? null : await Serializer.DeserializeAsync<T>(item);
		}

		private Dictionary<string, string> ParseInfo(string info)
		{
			var lines = info.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			var data = new Dictionary<string, string>();
			foreach (var line in lines)
			{
				if (string.IsNullOrEmpty(line) || line[0] == '#')
				{
					// 2.6+ can have empty lines, and comment lines
					continue;
				}

				var idx = line.IndexOf(':');
				if (idx > 0) // double check this line looks about right
				{
					var key = line.Substring(0, idx);
					var infoValue = line.Substring(idx + 1).Trim();

					data.Add(key, infoValue);
				}
			}

			return data;
		}

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
		public bool HashDelete(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
		{
			return Database.HashDelete(hashKey, key, commandFlags);
		}

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
		public long HashDelete(string hashKey, IEnumerable<string> keys, CommandFlags commandFlags = CommandFlags.None)
		{
			return Database.HashDelete(hashKey, keys.Select(x => (RedisValue)x).ToArray(), commandFlags);
		}

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
		public bool HashExists(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
		{
			return Database.HashExists(hashKey, key, commandFlags);
		}

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
		public T HashGet<T>(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
		{
			var redisValue = Database.HashGet(hashKey, key, commandFlags);
			return redisValue.HasValue ? Serializer.Deserialize<T>(redisValue) : default(T);
		}

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
		public Dictionary<string, T> HashGet<T>(string hashKey, IEnumerable<string> keys, CommandFlags commandFlags = CommandFlags.None)
		{
			return keys.Select(x => new { key = x, value = HashGet<T>(hashKey, x, commandFlags) })
						.ToDictionary(kv => kv.key, kv => kv.value, StringComparer.Ordinal);
		}

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
		public Dictionary<string, T> HashGetAll<T>(string hashKey, CommandFlags commandFlags = CommandFlags.None)
		{
			return Database
						.HashGetAll(hashKey, commandFlags)
						.ToDictionary(
							x => x.Name.ToString(),
							x => Serializer.Deserialize<T>(x.Value),
							StringComparer.Ordinal);
		}

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
		public long HashIncerementBy(string hashKey, string key, long value, CommandFlags commandFlags = CommandFlags.None)
		{
			return Database.HashIncrement(hashKey, key, value, commandFlags);
		}

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
		public double HashIncerementBy(string hashKey, string key, double value, CommandFlags commandFlags = CommandFlags.None)
		{
			return Database.HashIncrement(hashKey, key, value, commandFlags);
		}

		/// <summary>
		///     Returns all field names in the hash stored at key.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the size of the hash.
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>list of fields in the hash, or an empty list when key does not exist.</returns>
		public IEnumerable<string> HashKeys(string hashKey, CommandFlags commandFlags = CommandFlags.None)
		{
			return Database.HashKeys(hashKey, commandFlags).Select(x => x.ToString());
		}

		/// <summary>
		///     Returns the number of fields contained in the hash stored at key.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(1)
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>number of fields in the hash, or 0 when key does not exist.</returns>
		public long HashLength(string hashKey, CommandFlags commandFlags = CommandFlags.None)
		{
			return Database.HashLength(hashKey, commandFlags);
		}

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
		public bool HashSet<T>(string hashKey, string key, T value, bool nx = false, CommandFlags commandFlags = CommandFlags.None)
		{
			return Database.HashSet(hashKey, key, Serializer.Serialize(value), nx ? When.NotExists : When.Always, commandFlags);
		}

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
		public void HashSet<T>(string hashKey, Dictionary<string, T> values, CommandFlags commandFlags = CommandFlags.None)
		{
			var entries = values.Select(kv => new HashEntry(kv.Key, Serializer.Serialize(kv.Value)));
			Database.HashSet(hashKey, entries.ToArray(), commandFlags);
		}

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
		public IEnumerable<T> HashValues<T>(string hashKey, CommandFlags commandFlags = CommandFlags.None)
		{
			return Database.HashValues(hashKey, commandFlags).Select(x => Serializer.Deserialize<T>(x));
		}

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
		public Dictionary<string, T> HashScan<T>(string hashKey, string pattern, int pageSize = 10, CommandFlags commandFlags = CommandFlags.None)
		{
			return Database
						 .HashScan(hashKey, pattern, pageSize, commandFlags)
						 .ToDictionary(x => x.Name.ToString(),
									  x => Serializer.Deserialize<T>(x.Value),
									  StringComparer.Ordinal);
		}

		/// <param name="hashKey">Key of the hash</param>
		/// <param name="key">Key of the entry</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>
		///     If key is deleted returns true.
		///     If key does not exist, it is treated as an empty hash and this command returns false.
		/// </returns>
		public async Task<bool> HashDeleteAsync(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
		{
			return await Database.HashDeleteAsync(hashKey, key, commandFlags);
		}

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
		public async Task<long> HashDeleteAsync(string hashKey, IEnumerable<string> keys, CommandFlags commandFlags = CommandFlags.None)
		{
			return await Database.HashDeleteAsync(hashKey, keys.Select(x => (RedisValue)x).ToArray(), commandFlags);
		}

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
		public async Task<bool> HashExistsAsync(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
		{
			return await Database.HashExistsAsync(hashKey, key, commandFlags);
		}


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
		public async Task<T> HashGetAsync<T>(string hashKey, string key, CommandFlags commandFlags = CommandFlags.None)
		{
			var redisValue = await Database.HashGetAsync(hashKey, key, commandFlags);
			return redisValue.HasValue ? Serializer.Deserialize<T>(redisValue) : default(T);
		}

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
		public async Task<Dictionary<string, T>> HashGetAllAsync<T>(string hashKey, CommandFlags commandFlags = CommandFlags.None)
		{
			return (await Database
						.HashGetAllAsync(hashKey, commandFlags))
						.ToDictionary(
							x => x.Name.ToString(),
							x => Serializer.Deserialize<T>(x.Value),
							StringComparer.Ordinal);
		}

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
		public async Task<long> HashIncerementByAsync(string hashKey, string key, long value, CommandFlags commandFlags = CommandFlags.None)
		{
			return await Database.HashIncrementAsync(hashKey, key, value, commandFlags);
		}

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
		public async Task<double> HashIncerementByAsync(string hashKey, string key, double value, CommandFlags commandFlags = CommandFlags.None)
		{
			return await Database.HashIncrementAsync(hashKey, key, value, commandFlags);
		}

		/// <summary>
		///     Returns all field names in the hash stored at key.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(N) where N is the size of the hash.
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>list of fields in the hash, or an empty list when key does not exist.</returns>
		public async Task<IEnumerable<string>> HashKeysAsync(string hashKey, CommandFlags commandFlags = CommandFlags.None)
		{
			return (await Database.HashKeysAsync(hashKey, commandFlags)).Select(x => x.ToString());
		}

		/// <summary>
		///     Returns the number of fields contained in the hash stored at key.
		/// </summary>
		/// <remarks>
		///     Time complexity: O(1)
		/// </remarks>
		/// <param name="hashKey">Key of the hash</param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns>number of fields in the hash, or 0 when key does not exist.</returns>
		public async Task<long> HashLengthAsync(string hashKey, CommandFlags commandFlags = CommandFlags.None)
		{
			return await Database.HashLengthAsync(hashKey, commandFlags);
		}

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
		public async Task<bool> HashSetAsync<T>(string hashKey, string key, T value, bool nx = false, CommandFlags commandFlags = CommandFlags.None)
		{
			return await Database.HashSetAsync(hashKey, key, Serializer.Serialize(value), nx ? When.NotExists : When.Always, commandFlags);
		}

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
		/// <param name="commandFlags">Command executions flags</param>
		/// <param name="values"></param>
		public async Task HashSetAsync<T>(string hashKey, IDictionary<string, T> values, CommandFlags commandFlags = CommandFlags.None)
		{
			var entries = values.Select(kv => new HashEntry(kv.Key, Serializer.Serialize(kv.Value)));
			await Database.HashSetAsync(hashKey, entries.ToArray(), commandFlags);
		}

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
		public async Task<IEnumerable<T>> HashValuesAsync<T>(string hashKey, CommandFlags commandFlags = CommandFlags.None)
		{
			return (await Database.HashValuesAsync(hashKey, commandFlags)).Select(x => Serializer.Deserialize<T>(x));
		}

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
		/// <param name="pageSize"></param>
		/// <param name="commandFlags">Command execution flags</param>
		/// <returns></returns>
		public async Task<Dictionary<string, T>> HashScanAsync<T>(string hashKey, string pattern, int pageSize = 10, CommandFlags commandFlags = CommandFlags.None)
		{
			return (await Task.Run(() => Database.HashScan(hashKey, pattern, pageSize, commandFlags)))
				.ToDictionary(x => x.Name.ToString(), x => Serializer.Deserialize<T>(x.Value), StringComparer.Ordinal);
		}

        /// <summary>
        /// Updates the expiry time of a redis cache object
        /// </summary>
        /// <param name="key">The key of the object</param>
        /// <param name="expiresAt">The new expiry time of the object</param>
        /// <returns>True if the object is updated, false if the object does not exist</returns>
        public bool UpdateExpiry(string key, DateTimeOffset expiresAt)
        {
            if (Database.KeyExists(key))
            {
                return Database.KeyExpire(key, expiresAt.Subtract(DateTime.Now));
            }

            return false;
        }

        /// <summary>
        /// Updates the expiry time of a redis cache object
        /// </summary>
        /// <param name="key">The key of the object</param>
        /// <param name="expiresIn">Time until the object will expire</param>
        /// <returns>True if the object is updated, false if the object does not exist</returns>
        public bool UpdateExpiry(string key, TimeSpan expiresIn)
        {
            if (Database.KeyExists(key))
            {
                return Database.KeyExpire(key, expiresIn);
            }

            return false;
        }

        /// <summary>
        /// Updates the expiry time of a redis cache object
        /// </summary>
        /// <param name="key">The key of the object</param>
        /// <param name="expiresAt">The new expiry time of the object</param>
        /// <returns>True if the object is updated, false if the object does not exist</returns>
        public async Task<bool> UpdateExpiryAsync(string key, DateTimeOffset expiresAt)
        {
            if (await Database.KeyExistsAsync(key))
            {
                return await Database.KeyExpireAsync(key, expiresAt.Subtract(DateTime.Now));
            }

            return false;
        }

        /// <summary>
        /// Updates the expiry time of a redis cache object
        /// </summary>
        /// <param name="key">The key of the object</param>
        /// <param name="expiresIn">Time until the object will expire</param>
        /// <returns>True if the object is updated, false if the object does not exist</returns>
        public async Task<bool> UpdateExpiryAsync(string key, TimeSpan expiresIn)
        {
            if (await Database.KeyExistsAsync(key))
            {
                return await Database.KeyExpireAsync(key, expiresIn);
            }

            return false;
        }

        /// <summary>
        /// Updates the expiry time of a redis cache object
        /// </summary>
        /// <param name="keys">An array of keys to be updated</param>
        /// <param name="expiresAt">The new expiry time of the object</param>
        /// <returns>An IDictionary object that contains the origional key and the result of the operation</returns>
        public IDictionary<string, bool> UpdateExpiryAll(string[] keys, DateTimeOffset expiresAt)
        {
            var results = new Dictionary<string, bool>(StringComparer.Ordinal);
            for (int i = 0; i < keys.Length; i++)
            {
                results.Add(keys[i], UpdateExpiry(keys[i], expiresAt));
            }
            return results;
        }

        /// <summary>
        /// Updates the expiry time of a redis cache object
        /// </summary>
        /// <param name="keys">An array of keys to be updated</param>
        /// <param name="expiresIn">Time until the object will expire</param>
        /// <returns>An IDictionary object that contains the origional key and the result of the operation</returns>
        public IDictionary<string, bool> UpdateExpiryAll(string[] keys, TimeSpan expiresIn)
        {
            var results = new Dictionary<string, bool>(StringComparer.Ordinal);
            for (int i = 0; i < keys.Length; i++)
            {
                results.Add(keys[i], UpdateExpiry(keys[i], expiresIn));
            }
            return results;
        }

        /// <summary>
        /// Updates the expiry time of a redis cache object
        /// </summary>
        /// <param name="keys">An array of keys to be updated</param>
        /// <param name="expiresAt">The new expiry time of the object</param>
        /// <returns>An IDictionary object that contains the origional key and the result of the operation</returns>
        public async Task<IDictionary<string, bool>> UpdateExpiryAllAsync(string[] keys, DateTimeOffset expiresAt)
        {
            var results = new Dictionary<string, bool>(StringComparer.Ordinal);
            for (int i = 0; i < keys.Length; i++)
            {
                results.Add(keys[i], await UpdateExpiryAsync(keys[i], expiresAt));
            }
            return results;
        }

        /// <summary>
        /// Updates the expiry time of a redis cache object
        /// </summary>
        /// <param name="keys">An array of keys to be updated</param>
        /// <param name="expiresIn">Time until the object will expire</param>
        /// <returns>An IDictionary object that contains the origional key and the result of the operation</returns>
        public async Task<IDictionary<string, bool>> UpdateExpiryAllAsync(string[] keys, TimeSpan expiresIn)
        {
            var results = new Dictionary<string, bool>(StringComparer.Ordinal);
            for (int i = 0; i < keys.Length; i++)
            {
                results.Add(keys[i], await UpdateExpiryAsync(keys[i], expiresIn));
            }
            return results;
        }
    }
}