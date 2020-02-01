using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;

namespace StackExchange.Redis.Extensions.Core.Abstractions
{
    /// <summary>
    /// Allows a client to access to an instance of Redis database.
    /// </summary>
    public class RedisCacheClient : IRedisCacheClient
    {
        private readonly IRedisCacheConnectionPoolManager connectionPoolManager;
        private readonly RedisConfiguration redisConfiguration;

        /// <summary>
        /// Create an instace of <see cref="RedisCacheClient" />
        /// </summary>
        /// <param name="connectionPoolManager">An instance of the <see cref="IRedisCacheConnectionPoolManager" />.</param>
        /// <param name="serializer">An instance of the <see cref="ISerializer" />.</param>
        /// <param name="redisConfiguration">An instance of the <see cref="RedisConfiguration" />.</param>
        public RedisCacheClient(
            IRedisCacheConnectionPoolManager connectionPoolManager,
            ISerializer serializer,
            RedisConfiguration redisConfiguration)
        {
            this.connectionPoolManager = connectionPoolManager;
            Serializer = serializer;
            this.redisConfiguration = redisConfiguration;
        }

        /// <summary>
        /// Return an instance of the Redis database for the database 0.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        public IRedisDatabase Db0 => GetDb(0);

        /// <summary>
        /// Return an instance of the Redis database for the database 1.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        public IRedisDatabase Db1 => GetDb(1);

        /// <summary>
        /// Return an instance of the Redis database for the database 2.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        public IRedisDatabase Db2 => GetDb(2);

        /// <summary>
        /// Return an instance of the Redis database for the database  3.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        public IRedisDatabase Db3 => GetDb(3);

        /// <summary>
        /// Return an instance of the Redis database for the database 4.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        public IRedisDatabase Db4 => GetDb(4);

        /// <summary>
        /// Return an instance of the Redis database for the database 5.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        public IRedisDatabase Db5 => GetDb(5);

        /// <summary>
        /// Return an instance of the Redis database for the database 6.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        public IRedisDatabase Db6 => GetDb(6);

        /// <summary>
        /// Return an instance of the Redis database for the database 7.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        public IRedisDatabase Db7 => GetDb(7);

        /// <summary>
        /// Return an instance of the Redis database for the database 8.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        public IRedisDatabase Db8 => GetDb(8);

        /// <summary>
        /// Return an instance of the Redis database for the database 9.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        public IRedisDatabase Db9 => GetDb(9);

        /// <summary>
        /// Return an instance of the Redis database for the database 10.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        public IRedisDatabase Db10 => GetDb(10);

        /// <summary>
        /// Return an instance of the Redis database for the database 11.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        public IRedisDatabase Db11 => GetDb(11);

        /// <summary>
        /// Return an instance of the Redis database for the database 12.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        public IRedisDatabase Db12 => GetDb(12);

        /// <summary>
        /// Return an instance of the Redis database for the database 13.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        public IRedisDatabase Db13 => GetDb(13);

        /// <summary>
        /// Return an instance of the Redis database for the database 14.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>

        public IRedisDatabase Db14 => GetDb(14);

        /// <summary>
        /// Return an instance of the Redis database for the database 15.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        public IRedisDatabase Db15 => GetDb(15);

        /// <summary>
        /// Return an instance of the Redis database for the database 16.
        /// </summary>
        /// <returns>An instance of <see cref="ISerializer"/>.</returns>

        public IRedisDatabase Db16 => GetDb(16);

        /// <summary>
        /// Return an instance of the configured serializer.
        /// </summary>
        /// <returns>An instance of <see cref="ISerializer"/>.</returns>
        public ISerializer Serializer { get; }

        /// <summary>
        /// Returns an instance a Redis databse for the specific database;
        /// </summary>
        /// <param name="dbNumber">The databse number.</param>
        /// <param name="keyPrefix">The key prefix.</param>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
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

        /// <summary>
        /// Returns an instance a Redis database for the default database present into the configuration file;
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        public IRedisDatabase GetDbFromConfiguration()
        {
            return GetDb(redisConfiguration.Database, redisConfiguration.KeyPrefix);
        }
    }
}
