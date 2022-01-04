using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;

namespace StackExchange.Redis.Extensions.Core.Implementations;

/// <inheritdoc/>
public class RedisDatabaseFactory :IRedisDatabaseFactory
{
    private readonly Dictionary<string, IRedisCacheClient> redisCacheClients;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisDatabaseFactory"/> class.
    /// </summary>
    /// <param name="redisConfigurations">The connection configurations.</param>
    /// <param name="loggerFactory">The logger factory</param>
    /// <param name="serializer">The cache serializer</param>
    public RedisDatabaseFactory(RedisConfiguration[] redisConfigurations, ILoggerFactory loggerFactory, ISerializer serializer)
    {
        // First of all, I need to validate the configurations
        var hashSet = new HashSet<string>();

        for (var i = 0; i < redisConfigurations.Length; i++)
        {
            var configuration = redisConfigurations[i];

            if (hashSet.Contains(configuration.Name))
                throw new ArgumentException($"{nameof(RedisConfiguration.Name)} must be unique");

            hashSet.Add(configuration.Name);
        }

        if(!hashSet.Contains(Constants.DefaultConnectionName))
            throw new ArgumentException("Unable to locale the default connection. Please add a connection named 'Default'");

        redisCacheClients = new(redisConfigurations.Length);

        var poolManagerLogger = loggerFactory?.CreateLogger<RedisCacheConnectionPoolManager>();

        for (var i = 0; i < redisConfigurations.Length; i++)
        {
            var configuration = redisConfigurations[i];

            var poolManager = new RedisCacheConnectionPoolManager(configuration, poolManagerLogger);

            redisCacheClients.Add(configuration.Name, new RedisCacheClient(poolManager, serializer, configuration));
        }
    }

    /// <inheritdoc/>
    public IRedisCacheClient GetDefaultRedisClient()
    {
        return redisCacheClients[Constants.DefaultConnectionName];
    }

    /// <inheritdoc/>
    public IRedisCacheClient GetRedisClient(string name = null)
    {
        name ??= Constants.DefaultConnectionName;

        return redisCacheClients[name];
    }
}
