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

		public void Dispose()
		{
			connectionMultiplexer.Dispose();
		}

		public IDatabase Database
		{
			get { return db; }
		}

		public bool Exists(string key)
		{
			return db.KeyExists(key);
		}

		public Task<bool> ExistsAsync(string key)
		{
			return db.KeyExistsAsync(key);
		}

		public bool Remove(string key)
		{
			return db.KeyDelete(key);
		}

		public Task<bool> RemoveAsync(string key)
		{
			return db.KeyDeleteAsync(key);
		}

		public void RemoveAll(IEnumerable<string> keys)
		{
			keys.ForEach(x => Remove(x));
		}

		public Task RemoveAllAsync(IEnumerable<string> keys)
		{
			return keys.ForEachAsync(RemoveAsync);
		}

		public T Get<T>(string key) where T : class
		{
			var valueBytes = db.StringGet(key);

			if (!valueBytes.HasValue)
			{
				return default(T);
			}

			return (T) serializer.Deserialize(valueBytes);
		}

		public async Task<T> GetAsync<T>(string key) where T : class
		{
			var valueBytes = await db.StringGetAsync(key);

			if (!valueBytes.HasValue)
			{
				return default(T);
			}

			return (T) serializer.Deserialize(valueBytes);
		}

		public bool Add<T>(string key, T value) where T : class
		{
			var entryBytes = serializer.Serialize(value);

			return db.StringSet(key, entryBytes);
		}

		public Task<bool> AddAsync<T>(string key, T value) where T : class
		{
			var entryBytes = serializer.Serialize(value);

			return db.StringSetAsync(key, entryBytes);
		}

		public bool Replace<T>(string key, T value) where T : class
		{
			Remove(key);
			return Add(key, value);
		}

		public async Task<bool> ReplaceAsync<T>(string key, T value) where T : class
		{
			await RemoveAsync(key);
			return await AddAsync(key, value);
		}

		public bool Add<T>(string key, T value, DateTimeOffset expiresAt) where T : class
		{
			var entryBytes = serializer.Serialize(value);
			var expiration = expiresAt.Subtract(DateTimeOffset.Now);

			return db.StringSet(key, entryBytes, expiration);
		}

		public Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt) where T : class
		{
			var entryBytes = serializer.Serialize(value);
			var expiration = expiresAt.Subtract(DateTimeOffset.Now);

			return db.StringSetAsync(key, entryBytes, expiration);
		}

		public bool Replace<T>(string key, T value, DateTimeOffset expiresAt) where T : class
		{
			Remove(key);
			return Add(key, value, expiresAt);
		}

		public async Task<bool> ReplaceAsync<T>(string key, T value, DateTimeOffset expiresAt) where T : class
		{
			await RemoveAsync(key);
			return await AddAsync(key, value, expiresAt);
		}

		public bool Add<T>(string key, T value, TimeSpan expiresIn) where T : class
		{
			var entryBytes = serializer.Serialize(value);

			return db.StringSet(key, entryBytes, expiresIn);
		}

		public Task<bool> AddAsync<T>(string key, T value, TimeSpan expiresIn) where T : class
		{
			var entryBytes = serializer.Serialize(value);

			return db.StringSetAsync(key, entryBytes, expiresIn);
		}

		public bool Replace<T>(string key, T value, TimeSpan expiresIn) where T : class
		{
			Remove(key);
			return Add(key, value, expiresIn);
		}

		public async Task<bool> ReplaceAsync<T>(string key, T value, TimeSpan expiresIn) where T : class
		{
			await RemoveAsync(key);
			return await AddAsync(key, value, expiresIn);
		}

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