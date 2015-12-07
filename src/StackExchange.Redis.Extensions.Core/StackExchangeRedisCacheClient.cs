﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Extensions;

namespace StackExchange.Redis.Extensions.Core
{
	/// <summary>
	///     The implementation of <see cref="ICacheClient" />
	/// </summary>
	public class StackExchangeRedisCacheClient : ICacheClient
	{
		private readonly ConnectionMultiplexer connectionMultiplexer;

		/// <summary>
		///     Initializes a new instance of the <see cref="StackExchangeRedisCacheClient" /> class.
		/// </summary>
		/// <param name="serializer">The serializer.</param>
		/// <param name="configuration">The configuration.</param>
		public StackExchangeRedisCacheClient(ISerializer serializer, IRedisCachingConfiguration configuration = null)
		{
			if (serializer == null)
			{
				throw new ArgumentNullException(nameof(serializer));
			}

			if (configuration == null)
			{
				configuration = RedisCachingSectionHandler.GetConfig();
			}

			if (configuration == null)
			{
				throw new ConfigurationErrorsException(
					"Unable to locate <redisCacheClient> section into your configuration file. Take a look https://github.com/imperugo/StackExchange.Redis.Extensions");
			}

			var options = new ConfigurationOptions
			{
				Ssl = configuration.Ssl,
				AllowAdmin = configuration.AllowAdmin,
				Password = configuration.Password
			};

			foreach (RedisHost redisHost in configuration.RedisHosts)
			{
				options.EndPoints.Add(redisHost.Host, redisHost.CachePort);
			}

			connectionMultiplexer = ConnectionMultiplexer.Connect(options);
			Database = connectionMultiplexer.GetDatabase(configuration.Database);
			Serializer = serializer;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="StackExchangeRedisCacheClient" /> class.
		/// </summary>
		/// <param name="serializer">The serializer.</param>
		/// <param name="connectionString">The connection string.</param>
		/// <param name="database">The database.</param>
		/// <exception cref="System.ArgumentNullException">serializer</exception>
		public StackExchangeRedisCacheClient(ISerializer serializer, string connectionString, int database = 0)
		{
			if (serializer == null)
			{
				throw new ArgumentNullException(nameof(serializer));
			}

			Serializer = serializer;
			connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
			Database = connectionMultiplexer.GetDatabase(database);
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="StackExchangeRedisCacheClient" /> class.
		/// </summary>
		/// <param name="connectionMultiplexer">The connection multiplexer.</param>
		/// <param name="serializer">The serializer.</param>
		/// <param name="database">The database.</param>
		/// <exception cref="System.ArgumentNullException">
		///     connectionMultiplexer
		///     or
		///     serializer
		/// </exception>
		public StackExchangeRedisCacheClient(ConnectionMultiplexer connectionMultiplexer, ISerializer serializer,
			int database = 0)
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
			keys.ForEach(x => Remove(x));
		}

		/// <summary>
		///     Removes all specified keys from Redis Database
		/// </summary>
		/// <param name="keys">The key.</param>
		/// <returns></returns>
		public Task RemoveAllAsync(IEnumerable<string> keys)
		{
			return keys.ForEachAsync(RemoveAsync);
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
			var redisKeys = keys.Select(x => (RedisKey) x).ToArray();
			var result = Database.StringGet(redisKeys);
			return redisKeys.ToDictionary(key => (string) key, key =>
			{
				{
					var index = Array.IndexOf(redisKeys, key);
					var value = result[index];
					return value == RedisValue.Null ? default(T) : Serializer.Deserialize<T>(result[index]);
				}
			});
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
			var redisKeys = keys.Select(x => (RedisKey) x).ToArray();
			var result = await Database.StringGetAsync(redisKeys);
			return redisKeys.ToDictionary(key => (string) key, key =>
			{
				{
					var index = Array.IndexOf(redisKeys, key);
					var value = result[index];
					return value == RedisValue.Null ? default(T) : Serializer.Deserialize<T>(result[index]);
				}
			});
		}

		/// <summary>
		///     Adds all.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="items">The items.</param>
		public bool AddAll<T>(IList<Tuple<string, T>> items)
		{
			var values = items.ToDictionary<Tuple<string, T>, RedisKey, RedisValue>(item => item.Item1,
				item => Serializer.Serialize(item.Item2));

			return Database.StringSet(values.ToArray());
		}

		/// <summary>
		///     Adds all asynchronous.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="items">The items.</param>
		/// <returns></returns>
		public async Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items)
		{
			var values = items.ToDictionary<Tuple<string, T>, RedisKey, RedisValue>(item => item.Item1,
				item => Serializer.Serialize(item.Item2));

			return await Database.StringSetAsync(values.ToArray());
		}

		/// <summary>
		///     Run SADD command see http://redis.io/commands/sadd
		/// </summary>
		/// <param name="memberName">Name of the member.</param>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		[Obsolete(
			"Parameters are a little misleading. Digging further reveals the parameters should be swapped. Use SetAdd<T> instead."
			)]
		public bool SetAdd(string memberName, string key)
		{
			return Database.SetAdd(memberName, key);
		}

		/// <summary>
		///     Run SADD command see http://redis.io/commands/sadd
		/// </summary>
		/// <param name="memberName">Name of the member.</param>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		[Obsolete(
			"Parameters are a little misleading. Digging further reveals the parameters should be swapped. Use SetAddAsync<T> instead."
			)]
		public Task<bool> SetAddAsync(string memberName, string key)
		{
			return Database.SetAddAsync(memberName, key);
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
		///     Run SMEMBERS command http://redis.io/commands/SMEMBERS
		/// </summary>
		/// <param name="memberName">Name of the member.</param>
		/// <returns></returns>
		/// <exception cref="System.NotImplementedException"></exception>
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
			var keys = new HashSet<RedisKey>();

			var endPoints = Database.Multiplexer.GetEndPoints();

			foreach (var endpoint in endPoints)
			{
				var dbKeys = Database.Multiplexer.GetServer(endpoint).Keys(Database.Database, pattern);

				foreach (var dbKey in dbKeys)
				{
					if (!keys.Contains(dbKey))
					{
						keys.Add(dbKey);
					}
				}
			}

			return keys.Select(x => (string) x);
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
		public async void SaveAsync(SaveType saveType)
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

			return Serializer.Deserialize<T>(item);
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

			return await Serializer.DeserializeAsync<T>(item);
		}

		private Dictionary<string, string> ParseInfo(string info)
		{
			var lines = info.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
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
	}
}