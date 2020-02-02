namespace StackExchange.Redis.Extensions.Core.Abstractions
{
    /// <summary>
    /// Allows a client to access to an instance of Redis database.
    /// </summary>
    public interface IRedisCacheClient
    {
        /// <summary>
        /// Gets an instance of the Redis database for the database 0.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        IRedisDatabase Db0 { get; }

        /// <summary>
        /// Gets an instance of the Redis database for the database 1.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        IRedisDatabase Db1 { get; }

        /// <summary>
        /// Gets an instance of the Redis database for the database 2.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        IRedisDatabase Db2 { get; }

        /// <summary>
        /// Gets an instance of the Redis database for the database  3.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        IRedisDatabase Db3 { get; }

        /// <summary>
        /// Gets an instance of the Redis database for the database 4.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        IRedisDatabase Db4 { get; }

        /// <summary>
        /// Gets an instance of the Redis database for the database 5.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        IRedisDatabase Db5 { get; }

        /// <summary>
        /// Gets an instance of the Redis database for the database 6.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        IRedisDatabase Db6 { get; }

        /// <summary>
        /// Gets an instance of the Redis database for the database 7.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        IRedisDatabase Db7 { get; }

        /// <summary>
        /// Gets an instance of the Redis database for the database 8.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        IRedisDatabase Db8 { get; }

        /// <summary>
        /// Gets an instance of the Redis database for the database 9.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        IRedisDatabase Db9 { get; }

        /// <summary>
        /// Gets an instance of the Redis database for the database 10.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        IRedisDatabase Db10 { get; }

        /// <summary>
        /// Gets an instance of the Redis database for the database 11.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        IRedisDatabase Db11 { get; }

        /// <summary>
        /// Gets an instance of the Redis database for the database 12.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        IRedisDatabase Db12 { get; }

        /// <summary>
        /// Gets an instance of the Redis database for the database 13.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        IRedisDatabase Db13 { get; }

        /// <summary>
        /// Gets an instance of the Redis database for the database 14.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        IRedisDatabase Db14 { get; }

        /// <summary>
        /// Gets an instance of the Redis database for the database 15.
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        IRedisDatabase Db15 { get; }

        /// <summary>
        /// Gets an instance of the Redis database for the database 16.
        /// </summary>
        /// <returns>An instance of <see cref="ISerializer"/>.</returns>
        IRedisDatabase Db16 { get; }

        /// <summary>
        /// Gets an instance of the configured serializer.
        /// </summary>
        /// <returns>An instance of <see cref="ISerializer"/>.</returns>
        ISerializer Serializer { get; }

        /// <summary>
        /// Returns an instance a Redis databse for the specific database;
        /// </summary>
        /// <param name="dbNumber">The databse number.</param>
        /// <param name="keyPrefix">The key prefix.</param>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        IRedisDatabase GetDb(int dbNumber, string keyPrefix = null);

        /// <summary>
        /// Returns an instance a Redis database for the default database present into the configuration file;
        /// </summary>
        /// <returns>An instance of <see cref="IRedisDatabase"/>.</returns>
        IRedisDatabase GetDbFromConfiguration();
    }
}
