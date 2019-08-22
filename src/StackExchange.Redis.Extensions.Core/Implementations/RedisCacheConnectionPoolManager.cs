using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;

namespace StackExchange.Redis.Extensions.Core.Implementations
{
	public class RedisCacheConnectionPoolManager : IRedisCacheConnectionPoolManager
	{
		private static ConcurrentBag<Lazy<Task<ConnectionMultiplexer>>> connections;
		private readonly RedisConfiguration redisConfiguration;

		public RedisCacheConnectionPoolManager(RedisConfiguration redisConfiguration)
		{
			this.redisConfiguration =  redisConfiguration;
			Initialize();
		}

		public void Dispose()
		{
			var activeConnections = connections.Where(lazy => lazy.IsValueCreated).ToList();

			foreach (var connection in activeConnections)
			{
				connection.Value.Dispose();
			}

			Initialize();
		}

		public IConnectionMultiplexer GetConnection()
        {
            Lazy<Task<ConnectionMultiplexer>> response;
	        var loadedLazys = connections.Where(lazy => lazy.IsValueCreated);
	        
            if (loadedLazys.Count() == connections.Count) {
		        response = connections.OrderBy(x=>x.Value.Result.GetCounters().TotalOutstanding).First();
	        }
	        else {
		        response = connections.First(lazy => !lazy.IsValueCreated);
	        }
	        
            return response.Value.Result;
        }

		private void Initialize()
		{
			connections = new ConcurrentBag<Lazy<Task<ConnectionMultiplexer>>>();

			for (int i = 0; i < redisConfiguration.PoolSize; i++)
			{
				connections.Add(new Lazy<Task<ConnectionMultiplexer>>(async () => await ConnectionMultiplexer.ConnectAsync(redisConfiguration.ConfigurationOptions).ConfigureAwait(false)));
			}
		}

	}
}