using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;

namespace StackExchange.Redis.Extensions.Core.Abstractions
{
    public class RedisCacheClient : IRedisCacheClient
    {
        private readonly IRedisCacheConnectionPoolManager connectionPoolManager;
        private readonly RedisConfiguration redisConfiguration;

        public RedisCacheClient(
            IRedisCacheConnectionPoolManager connectionPoolManager, 
            ISerializer serializer, 
            RedisConfiguration redisConfiguration)
        {
            this.connectionPoolManager = connectionPoolManager;
            Serializer = serializer;
            this.redisConfiguration = redisConfiguration;
        }

        public IRedisDatabase Db0 => GetDb(0);

        public IRedisDatabase Db1 => GetDb(1);

        public IRedisDatabase Db2 => GetDb(2);

        public IRedisDatabase Db3 => GetDb(3);

        public IRedisDatabase Db4 => GetDb(4);

        public IRedisDatabase Db5 => GetDb(5);

        public IRedisDatabase Db6 => GetDb(6);

        public IRedisDatabase Db7 => GetDb(7);

        public IRedisDatabase Db8 => GetDb(8);

        public IRedisDatabase Db9 => GetDb(9);

        public IRedisDatabase Db10 => GetDb(10);

        public IRedisDatabase Db11 => GetDb(11);

        public IRedisDatabase Db12 => GetDb(12);

        public IRedisDatabase Db13 => GetDb(13);

        public IRedisDatabase Db14 => GetDb(14);

        public IRedisDatabase Db15 => GetDb(15);

        public IRedisDatabase Db16 => GetDb(16);

        public ISerializer Serializer { get; }

        public IRedisDatabase GetDb(int dbNumber, string keyPrefix = null)
        {
            var connection = connectionPoolManager.GetConnection();
            var db = connection.GetDatabase(dbNumber);

            return new RedisDatabase(
                connection,
                Serializer,
                redisConfiguration.ServerEnumerationStrategy,
                db,
                redisConfiguration.MaxValueLength,
                keyPrefix);
        }

        public IRedisDatabase GetDbFromConfiguration()
        {
            return GetDb(redisConfiguration.Database, redisConfiguration.KeyPrefix);
        }
	}
}