using System;
using System.Collections.Concurrent;
using System.Linq;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;

namespace StackExchange.Redis.Extensions.Core.Implementations
{
	public class RedisCacheConnectionPoolManager : IRedisCacheConnectionPoolManager
	{
		private const int POOL_SIZE = 10;
		private static ConcurrentBag<Lazy<ConnectionMultiplexer>> connections = new ConcurrentBag<Lazy<ConnectionMultiplexer>>();

		public RedisCacheConnectionPoolManager(RedisConfiguration redisConfiguration)
		{
			for (int i = 0; i < POOL_SIZE; i++)
			{
				connections.Add(new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(redisConfiguration.ConfigurationOptions)));
			}
		}

		public IConnectionMultiplexer GetConnection()
		{
			Lazy<ConnectionMultiplexer> response;

			var loadedLazys = connections.Where(lazy => lazy.IsValueCreated);

			if (loadedLazys.Count() == connections.Count)
			{
				var minValue = connections.Min(lazy => lazy.Value.GetCounters().TotalOutstanding);
				response = connections.First(lazy => lazy.Value.GetCounters().TotalOutstanding == minValue);
			}
			else
			{
				response = connections.First(lazy => !lazy.IsValueCreated);
			}

			return response.Value;
		}
	}
}
