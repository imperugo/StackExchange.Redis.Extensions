using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions
{
	public class StackExchangeRedisCacheClient : ICacheClient
	{
		private readonly ConnectionMultiplexer connectionMultiplexer;
		private readonly IDatabase db;
		private readonly ISerializer serializer;

		/// <summary>
		/// Initializes a new instance of the <see cref="StackExchangeRedisCacheClient"/> class.
		/// </summary>
		/// <param name="connectionMultiplexer">The connection multiplexer.</param>
		/// <param name="serializer">The serializer.</param>
		public StackExchangeRedisCacheClient(ConnectionMultiplexer connectionMultiplexer, ISerializer serializer)
		{
			if (connectionMultiplexer == null)
			{
				connectionMultiplexer = GetInstanceFromConfigurationFile();
			}

			if (serializer == null)
			{
				serializer = new BinarySerializer();
			}

			this.serializer = serializer;
			this.connectionMultiplexer = connectionMultiplexer;

			db = connectionMultiplexer.GetDatabase();
		}

		private ConnectionMultiplexer GetInstanceFromConfigurationFile()
		{
			return ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["RedisConnectionString"]);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			connectionMultiplexer.Dispose();
		}

		/// <summary>
		/// Return the instance of <see cref="StackExchange.Redis.IDatabase" /> used be ICacheClient implementation
		/// </summary>
		public IDatabase Database
		{
			get { return db; }
		}

		/// <summary>
		/// Verify that the specified cache key exists
		/// </summary>
		/// <param name="key">The cache key.</param>
		/// <returns>
		/// True if the key is present into Redis. Othwerwise False
		/// </returns>
		public bool Exists(string key)
		{
			return db.KeyExists(key);
		}

		/// <summary>
		/// Verify that the specified cache key exists
		/// </summary>
		/// <param name="key">The cache key.</param>
		/// <returns>
		/// True if the key is present into Redis. Othwerwise False
		/// </returns>
		public Task<bool> ExistsAsync(string key)
		{
			return db.KeyExistsAsync(key);
		}

		/// <summary>
		/// Removes the specified key from Redis Database
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		/// True if the key has removed. Othwerwise False
		/// </returns>
		public bool Remove(string key)
		{
			return db.KeyDelete(key);
		}

		/// <summary>
		/// Removes the specified key from Redis Database
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		/// True if the key has removed. Othwerwise False
		/// </returns>
		public Task<bool> RemoveAsync(string key)
		{
			return db.KeyDeleteAsync(key);
		}

		/// <summary>
		/// Removes all specified keys from Redis Database
		/// </summary>
		/// <param name="keys">The key.</param>
		public void RemoveAll(IEnumerable<string> keys)
		{
			keys.ForEach(x => Remove(x));
		}

		/// <summary>
		/// Removes all specified keys from Redis Database
		/// </summary>
		/// <param name="keys">The key.</param>
		/// <returns></returns>
		public Task RemoveAllAsync(IEnumerable<string> keys)
		{
			return keys.ForEachAsync(RemoveAsync);
		}

		/// <summary>
		/// Get the object with the specified key from Redis database
		/// </summary>
		/// <typeparam name="T">The type of the expected object</typeparam>
		/// <param name="key">The cache key.</param>
		/// <returns>
		/// Null if not present, otherwise the instance of T.
		/// </returns>
		public T Get<T>(string key) where T : class
		{
			var valueBytes = db.StringGet(key);

			if (!valueBytes.HasValue)
			{
				return default(T);
			}

			return (T) serializer.Deserialize(valueBytes);
		}

		/// <summary>
		/// Get the object with the specified key from Redis database
		/// </summary>
		/// <typeparam name="T">The type of the expected object</typeparam>
		/// <param name="key">The cache key.</param>
		/// <returns>
		/// Null if not present, otherwise the instance of T.
		/// </returns>
		public async Task<T> GetAsync<T>(string key) where T : class
		{
			var valueBytes = await db.StringGetAsync(key);

			if (!valueBytes.HasValue)
			{
				return default(T);
			}

			return (T) serializer.Deserialize(valueBytes);
		}

		/// <summary>
		/// Adds the specified instance to the Redis database.
		/// </summary>
		/// <typeparam name="T">The type of the class to add to Redis</typeparam>
		/// <param name="key">The cache key.</param>
		/// <param name="value">The instance of T.</param>
		/// <returns>
		/// True if the object has been added. Otherwise false
		/// </returns>
		public bool Add<T>(string key, T value) where T : class
		{
			var entryBytes = serializer.Serialize(value);

			return db.StringSet(key, entryBytes);
		}

		/// <summary>
		/// Adds the specified instance to the Redis database.
		/// </summary>
		/// <typeparam name="T">The type of the class to add to Redis</typeparam>
		/// <param name="key">The cache key.</param>
		/// <param name="value">The instance of T.</param>
		/// <returns>
		/// True if the object has been added. Otherwise false
		/// </returns>
		public Task<bool> AddAsync<T>(string key, T value) where T : class
		{
			var entryBytes = serializer.Serialize(value);

			return db.StringSetAsync(key, entryBytes);
		}

		/// <summary>
		/// Replaces the object with specified key into Redis database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <param name="value">The instance of T</param>
		/// <returns>
		/// True if the object has been added. Otherwise false
		/// </returns>
		public bool Replace<T>(string key, T value) where T : class
		{
			Remove(key);
			return Add(key, value);
		}

		/// <summary>
		/// Replaces the object with specified key into Redis database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The key.</param>
		/// <param name="value">The instance of T</param>
		/// <returns>
		/// True if the object has been added. Otherwise false
		/// </returns>
		public async Task<bool> ReplaceAsync<T>(string key, T value) where T : class
		{
			await RemoveAsync(key);
			return await AddAsync(key, value);
		}

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
		public bool Add<T>(string key, T value, DateTimeOffset expiresAt) where T : class
		{
			var entryBytes = serializer.Serialize(value);
			var expiration = expiresAt.Subtract(DateTimeOffset.Now);

			return db.StringSet(key, entryBytes, expiration);
		}

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
		public Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt) where T : class
		{
			var entryBytes = serializer.Serialize(value);
			var expiration = expiresAt.Subtract(DateTimeOffset.Now);

			return db.StringSetAsync(key, entryBytes, expiration);
		}

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
		public bool Replace<T>(string key, T value, DateTimeOffset expiresAt) where T : class
		{
			Remove(key);
			return Add(key, value, expiresAt);
		}

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
		public async Task<bool> ReplaceAsync<T>(string key, T value, DateTimeOffset expiresAt) where T : class
		{
			await RemoveAsync(key);
			return await AddAsync(key, value, expiresAt);
		}

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
		public bool Add<T>(string key, T value, TimeSpan expiresIn) where T : class
		{
			var entryBytes = serializer.Serialize(value);

			return db.StringSet(key, entryBytes, expiresIn);
		}

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
		public Task<bool> AddAsync<T>(string key, T value, TimeSpan expiresIn) where T : class
		{
			var entryBytes = serializer.Serialize(value);

			return db.StringSetAsync(key, entryBytes, expiresIn);
		}

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
		public bool Replace<T>(string key, T value, TimeSpan expiresIn) where T : class
		{
			Remove(key);
			return Add(key, value, expiresIn);
		}

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
		public async Task<bool> ReplaceAsync<T>(string key, T value, TimeSpan expiresIn) where T : class
		{
			await RemoveAsync(key);
			return await AddAsync(key, value, expiresIn);
		}

		/// <summary>
		/// Get the objects with the specified keys from Redis database with one roundtrip
		/// </summary>
		/// <typeparam name="T">The type of the expected object</typeparam>
		/// <param name="keys">The keys.</param>
		/// <returns>
		/// Empty list if there are no results, otherwise the instance of T.
		/// If a cache key is not present on Redis the specified object into the returned Dictionary will be null
		/// </returns>
		public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys) where T : class
		{
			var keysList = keys.ToList();
			var redisKeys = new RedisKey[keysList.Count];
			var sb = CreateLuaScriptForMget(redisKeys, keysList);

			var redisResults = (RedisResult[]) db.ScriptEvaluate(sb, redisKeys);

			var result = new Dictionary<string, T>();

			for (var i = 0; i < redisResults.Count(); i++)
			{
				var obj = default(T);

				if (!redisResults[i].IsNull)
				{
					obj = (T) serializer.Deserialize((byte[]) redisResults[i]);
				}
				result.Add(keysList[i], obj);
			}

			return result;
		}

		/// <summary>
		/// Get the objects with the specified keys from Redis database with one roundtrip
		/// </summary>
		/// <typeparam name="T">The type of the expected object</typeparam>
		/// <param name="keys">The keys.</param>
		/// <returns>
		/// Empty list if there are no results, otherwise the instance of T.
		/// If a cache key is not present on Redis the specified object into the returned Dictionary will be null
		/// </returns>
		public async Task<IDictionary<string, T>> GetAllAsync<T>(IEnumerable<string> keys) where T : class
		{
			var keysList = keys.ToList();
			var redisKeys = new RedisKey[keysList.Count];
			var sb = CreateLuaScriptForMget(redisKeys, keysList);

			var redisResults = (RedisResult[]) await db.ScriptEvaluateAsync(sb, redisKeys);

			var result = new Dictionary<string, T>();

			for (var i = 0; i < redisResults.Count(); i++)
			{
				var obj = default(T);

				if (!redisResults[i].IsNull)
				{
					obj = (T) serializer.Deserialize((byte[]) redisResults[i]);
				}
				result.Add(keysList[i], obj);
			}

			return result;
		}

		/// <summary>
		/// Searches the keys from Redis database
		/// </summary>
		/// <param name="pattern">The pattern.</param>
		/// <returns>
		/// A list of cache keys retrieved from Redis database
		/// </returns>
		/// <example>
		/// if you want to return all keys that start with "myCacheKey" uses "myCacheKey*"
		/// if you want to return all keys that contain with "myCacheKey" uses "*myCacheKey*"
		/// if you want to return all keys that end with "myCacheKey" uses "*myCacheKey"
		/// </example>
		public IEnumerable<string> SearchKeys(string pattern)
		{
			var keys = new HashSet<RedisKey>();

			var endPoints = db.Multiplexer.GetEndPoints();

			foreach (var endpoint in endPoints)
			{
				var dbKeys = db.Multiplexer.GetServer(endpoint).Keys(pattern: pattern);

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
		/// Searches the keys from Redis database
		/// </summary>
		/// <param name="pattern">The pattern.</param>
		/// <returns>
		/// A list of cache keys retrieved from Redis database
		/// </returns>
		/// <example>
		/// if you want to return all keys that start with "myCacheKey" uses "myCacheKey*"
		/// if you want to return all keys that contain with "myCacheKey" uses "*myCacheKey*"
		/// if you want to return all keys that end with "myCacheKey" uses "*myCacheKey"
		/// </example>
		public Task<IEnumerable<string>> SearchKeysAsync(string pattern)
		{
			return Task.Run(() => SearchKeys(pattern));
		}

		private string CreateLuaScriptForMget(RedisKey[] redisKeys, List<string> keysList)
		{
			var sb = new StringBuilder("return redis.call('mget',");


			for (var i = 0; i < keysList.Count; i++)
			{
				redisKeys[i] = keysList[i];
				sb.AppendFormat("KEYS[{0}]", i + 1);

				if (i < keysList.Count - 1)
				{
					sb.Append(",");
				}
			}

			sb.Append(")");

			return sb.ToString();
		}
	}
}