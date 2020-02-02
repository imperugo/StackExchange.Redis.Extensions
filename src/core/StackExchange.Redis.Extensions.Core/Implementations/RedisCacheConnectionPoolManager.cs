using System;
using System.Collections.Concurrent;
using System.Linq;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;

namespace StackExchange.Redis.Extensions.Core.Implementations
{
    /// <inheritdoc/>
    public class RedisCacheConnectionPoolManager : IRedisCacheConnectionPoolManager
    {
        private static ConcurrentBag<Lazy<ConnectionMultiplexer>> connections;
        private readonly RedisConfiguration redisConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCacheConnectionPoolManager"/> class.
        /// </summary>
        /// <param name="redisConfiguration">The redis configurartion.</param>
        public RedisCacheConnectionPoolManager(RedisConfiguration redisConfiguration)
        {
            this.redisConfiguration = redisConfiguration;
            Initialize();
        }

        /// <inheritdoc/>
        public IConnectionMultiplexer GetConnection()
        {
            Lazy<ConnectionMultiplexer> response;
            var loadedLazys = connections.Where(lazy => lazy.IsValueCreated);

            if (loadedLazys.Count() == connections.Count)
            {
                response = connections.OrderBy(x => x.Value.GetCounters().TotalOutstanding).First();
            }
            else
            {
                response = connections.First(lazy => !lazy.IsValueCreated);
            }

            return response.Value;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            var activeConnections = connections.Where(lazy => lazy.IsValueCreated).ToList();

            foreach (var connection in activeConnections)
                connection.Value.Dispose();

            Initialize();
        }

        private void Initialize()
        {
            connections = new ConcurrentBag<Lazy<ConnectionMultiplexer>>();

            for (var i = 0; i < redisConfiguration.PoolSize; i++)
            {
                connections.Add(new Lazy<ConnectionMultiplexer>(() =>
                {
                    var multiplexer = ConnectionMultiplexer.Connect(redisConfiguration.ConfigurationOptions);

                    if (redisConfiguration.ProfilingSessionProvider != null)
                        multiplexer.RegisterProfiler(redisConfiguration.ProfilingSessionProvider);

                    return multiplexer;
                }));
            }
        }
    }
}
