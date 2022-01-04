using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Extensions;
using StackExchange.Redis.Extensions.Core.Models;

namespace StackExchange.Redis.Extensions.Core.Implementations;

/// <inheritdoc/>
public sealed partial class RedisCacheConnectionPoolManager : IRedisCacheConnectionPoolManager
{
    private static readonly object @lock = new();
    private readonly IStateAwareConnection[] connections;
    private readonly RedisConfiguration redisConfiguration;
    private readonly ILogger<RedisCacheConnectionPoolManager> logger;
    private readonly Random random = new();
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisCacheConnectionPoolManager"/> class.
    /// </summary>
    /// <param name="redisConfiguration">The redis configuration.</param>
    /// <param name="logger">The logger.</param>
    public RedisCacheConnectionPoolManager(RedisConfiguration redisConfiguration, ILogger<RedisCacheConnectionPoolManager> logger = null)
    {
        this.redisConfiguration = redisConfiguration ?? throw new ArgumentNullException(nameof(redisConfiguration));
        this.logger = logger ?? NullLogger<RedisCacheConnectionPoolManager>.Instance;

        lock (@lock)
        {
            connections = new IStateAwareConnection[redisConfiguration.PoolSize];
            EmitConnections();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    private void Dispose(bool disposing)
    {
        if (isDisposed)
            return;

        if (disposing)
        {
            // free managed resources
            foreach (var connection in connections)
                connection.Dispose();
        }

        isDisposed = true;
    }

    /// <inheritdoc/>
    public IConnectionMultiplexer GetConnection()
    {
        IStateAwareConnection connection;

        switch (redisConfiguration.ConnectionSelectionStrategy)
        {
            case ConnectionSelectionStrategy.RoundRobin:
                var nextIdx = random.Next(0, redisConfiguration.PoolSize);
                connection = connections[nextIdx];
                break;

            case ConnectionSelectionStrategy.LeastLoaded:
                connection = connections.MinBy(x => x.TotalOutstanding());
                break;

            default:
                throw new InvalidEnumArgumentException(nameof(redisConfiguration.ConnectionSelectionStrategy), (int)redisConfiguration.ConnectionSelectionStrategy, typeof(ConnectionSelectionStrategy));
        }

        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug("Using connection {HashCode} with {OutStanding} outstanding!", connection.Connection.GetHashCode().ToString(), connection.TotalOutstanding().ToString());

        return connection.Connection;
    }

    /// <inheritdoc/>
    public ConnectionPoolInformation GetConnectionInformations()
    {
        var activeConnections = 0;
        var invalidConnections = 0;

        foreach (var connection in connections)
        {
            if (!connection.IsConnected())
            {
                invalidConnections++;
                continue;
            }

            activeConnections++;
        }

        return new()
        {
            RequiredPoolSize = redisConfiguration.PoolSize,
            ActiveConnections = activeConnections,
            InvalidConnections = invalidConnections
        };
    }

    private void EmitConnections()
    {
        for (var i = 0; i < redisConfiguration.PoolSize; i++)
        {
            var multiplexer = ConnectionMultiplexer.Connect(redisConfiguration.ConfigurationOptions);

            if (redisConfiguration.ProfilingSessionProvider != null)
                multiplexer.RegisterProfiler(redisConfiguration.ProfilingSessionProvider);

            connections[i] = redisConfiguration.StateAwareConnectionFactory(multiplexer, logger);
        }
    }
}
