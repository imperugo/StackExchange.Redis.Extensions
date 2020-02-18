using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;

namespace StackExchange.Redis.Extensions.Tests.Pool
{
	internal class SinglePool : IRedisCacheConnectionPoolManager
    {
        private readonly RedisConfiguration redisConfiguration;

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
    }
}
