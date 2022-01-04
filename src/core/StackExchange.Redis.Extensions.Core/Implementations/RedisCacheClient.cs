using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;

namespace StackExchange.Redis.Extensions.Core.Abstractions;

/// <inheritdoc/>
public class RedisCacheClient : IRedisCacheClient
{
    private readonly IRedisCacheConnectionPoolManager connectionPoolManager;
    private readonly RedisConfiguration redisConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisCacheClient"/> class.
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

    /// <inheritdoc/>
    public IRedisDatabase Db0 => GetDb(0);

    /// <inheritdoc/>
    public IRedisDatabase Db1 => GetDb(1);

    /// <inheritdoc/>
    public IRedisDatabase Db2 => GetDb(2);

    /// <inheritdoc/>
    public IRedisDatabase Db3 => GetDb(3);

    /// <inheritdoc/>
    public IRedisDatabase Db4 => GetDb(4);

    /// <inheritdoc/>
    public IRedisDatabase Db5 => GetDb(5);

    /// <inheritdoc/>
    public IRedisDatabase Db6 => GetDb(6);

    /// <inheritdoc/>
    public IRedisDatabase Db7 => GetDb(7);

    /// <inheritdoc/>
    public IRedisDatabase Db8 => GetDb(8);

    /// <inheritdoc/>
    public IRedisDatabase Db9 => GetDb(9);

    /// <inheritdoc/>
    public IRedisDatabase Db10 => GetDb(10);

    /// <inheritdoc/>
    public IRedisDatabase Db11 => GetDb(11);

    /// <inheritdoc/>
    public IRedisDatabase Db12 => GetDb(12);

    /// <inheritdoc/>
    public IRedisDatabase Db13 => GetDb(13);

    /// <inheritdoc/>
    public IRedisDatabase Db14 => GetDb(14);

    /// <inheritdoc/>
    public IRedisDatabase Db15 => GetDb(15);

    /// <inheritdoc/>
    public IRedisDatabase Db16 => GetDb(16);

    /// <inheritdoc/>
    public ISerializer Serializer { get; }

    /// <inheritdoc/>
    public IRedisDatabase GetDb(int dbNumber, string? keyPrefix = null)
    {
        if (string.IsNullOrEmpty(keyPrefix))
            keyPrefix = redisConfiguration.KeyPrefix;

        return new RedisDatabase(
            connectionPoolManager,
            Serializer,
            redisConfiguration.ServerEnumerationStrategy,
            dbNumber,
            redisConfiguration.MaxValueLength,
            keyPrefix!);
    }

    /// <inheritdoc/>
    public IRedisDatabase GetDbFromConfiguration()
    {
        return GetDb(redisConfiguration.Database, redisConfiguration.KeyPrefix);
    }
}
