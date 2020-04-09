using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Models;

namespace StackExchange.Redis.Extensions.Tests.Pool
{
    internal class SinglePool : IRedisCacheConnectionPoolManager
    {
        private readonly RedisConfiguration redisConfiguration;

        public SinglePool()
        {
        }

        public SinglePool(RedisConfiguration redisConfiguration)
        {
            this.redisConfiguration = redisConfiguration;
        }

        public void Dispose()
        {
            redisConfiguration.Connection.Dispose();
        }

        public IConnectionMultiplexer GetConnection()
        {
            return redisConfiguration.Connection;
        }

        public ConnectionPoolInformation GetConnectionInformations()
        {
            return new ConnectionPoolInformation()
            {
                RequiredPoolSize = 1,
                ActiveConnections = redisConfiguration.Connection.IsConnected ? 1 : 0,
                InvalidConnections = !redisConfiguration.Connection.IsConnected ? 1 : 0,
                ReadyNotUsedYet = 0
            };
        }
    }
}
